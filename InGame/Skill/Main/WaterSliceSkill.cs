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
        private int _damage;
        private List<ParticleSystem> particleList = new List<ParticleSystem>();
        
        public float skillCoolTime => _coolTime; // 스킬의 사용딜레이
        public float skillCurrentCoolTime { get; private set; } // 현재 스킬 대기시간

        public override void Init(int skillIndex, GamePlayer gPlayer, SkillButton button)
        {
            data =  DataManager.Instance.skillDataDic[skillIndex];

            _coolTime = data.coolTime;
            _damage = data.dynamicData[0];
            
            player = gPlayer;
            _skillButton = button;
            skillType = SkillType.Hero;
            useSkill = false;
            _prefab = ResourceManager.Instance.GetPrefab("WaterSlice");
        }

        public override void SkillStart()
        {
            useSkill = true;
            UseSkill();
        }

        public override void UseSkill(params object[] _params)
        {
            StartCoroutine(ShowCor());
        }
        
        public IEnumerator ShowCor()
        {
            StartCoroutine(WaitCoolTimeCor(_coolTime));
            
            // 적 몬스터 
            List<MonsterBase> monsterList = new List<MonsterBase>(player.monsterList);

            int dmg = _damage * GameManager.Instance.myPlayer.stageProcess;
            
            foreach (var monster in monsterList)
            {
                if (!monster.gameObject.activeSelf)
                    continue;
                
                GameObject go = Instantiate(_prefab, GameManager.Instance.gameMode.effectParent);
                go.transform.position = monster.transform.position;
                
                ParticleSystem particle = go.GetComponent<ParticleSystem>();
                particle.Play();
                particleList.Add(particle);
                monster.Hit(dmg);
                yield return new WaitForSeconds(0.02f);
            }
            
            yield return new WaitForSeconds(1f);
            
            GameUIManager.Instance.HideSkillDim();
            foreach (var particle in particleList)
                Destroy(particle);
            
        }
    }
}