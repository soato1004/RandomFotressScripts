using System.Collections;
using RandomFortress.Manager;
using UnityEngine;
using System.Collections.Generic;

namespace RandomFortress.Game
{
    /// <summary>
    /// 히어로스킬. 적 몬스터 모두에게 수속성 데미지
    /// </summary>
    public class WaterSliceSkill : BaseSkill
    {
        
        private GameObject _prefab;
        private SkillButton _skillButton;
        private int _damage = 100;
        private float _coolTime = 60;
        private bool _canUseSkill = true;
        private List<ParticleSystem> particleList = new List<ParticleSystem>();
        
        
        public float skillCoolTime => _coolTime; // 스킬의 사용딜레이
        public float skillCurrentCoolTime { get; private set; } // 현재 스킬 대기시간

        public override void Init(GamePlayer gPlayer, SkillButton button)
        {
            player = gPlayer;
            _skillButton = button;
            skillType = SkillType.Hero;
            _prefab = ResourceManager.Instance.GetPrefab("WaterSlice");
            UseSkill();
        }
        
        private IEnumerator WaitCoolTimeCor()
        {
            _canUseSkill = false;
            skillCurrentCoolTime = 0;
            _skillButton.wait.SetActive(true);
            while (_coolTime > skillCurrentCoolTime)
            {
                if (GameManager.Instance.isGameOver)
                    yield break;
                
                _skillButton.coolTime.text = "" + (int)(_coolTime - skillCurrentCoolTime);
                skillCurrentCoolTime += Time.deltaTime;
                yield return null;
            }
            _skillButton.wait.SetActive(false);
            _canUseSkill = true;
            Debug.Log("Skill Ready!!");
        }
        
        public override bool CanUseSkill() => _canUseSkill;

        public override void UseSkill(params object[] _params)
        {
            StartCoroutine(ShowCor());
        }
        
        public IEnumerator ShowCor()
        {
            StartCoroutine(WaitCoolTimeCor());
            
            // 적 몬스터 
            List<MonsterBase> monsterList = new List<MonsterBase>(player.monsterList);
            
            foreach (var monster in monsterList)
            {
                if (!monster.gameObject.activeSelf)
                    continue;
                
                GameObject go = Instantiate(_prefab, GameManager.Instance.gameMode.effectParent);
                go.transform.position = monster.transform.position;
                
                ParticleSystem particle = go.GetComponent<ParticleSystem>();
                particle.Play();
                particleList.Add(particle);
                monster.Hit(_damage);
                yield return new WaitForSeconds(0.02f);
            }
            
            yield return new WaitForSeconds(1f);
            
            foreach (var particle in particleList)
            {
                Destroy(particle);
            }
        }
    }
}