using System;
using System.Collections;
using RandomFortress.Manager;
using UnityEngine;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

namespace RandomFortress.Game.Skill
{
    /// <summary>
    /// 히어로스킬. 적 몬스터 모두에게 수속성 데미지
    /// </summary>
    public class WaterSliceSkill : BaseSkill
    {
        
        private GameObject prefab;
        private SkillButton skillButton;
        private int damage = 100;
        private float coolTime = 60;
        private bool canUseSkill = true;
        public float Skill_CoolTime => coolTime; // 스킬의 사용딜레이
        public float Skill_CurrentCoolTime { get; private set; } // 현재 스킬 대기시간
        
        List<ParticleSystem> particleList = new List<ParticleSystem>();
        
        public override void Init(SkillButton button)
        {
            skillButton = button;
            skillType = SkillType.Hero;
            prefab = ResourceManager.Instance.GetPrefab("WaterSlice");
            UseSkill();
        }
        
        private IEnumerator WaitCoolTimeCor()
        {
            canUseSkill = false;
            Skill_CurrentCoolTime = 0;
            skillButton.wait.SetActive(true);
            while (coolTime > Skill_CurrentCoolTime)
            {
                skillButton.coolTime.text = "" + (int)(coolTime - Skill_CurrentCoolTime);
                Skill_CurrentCoolTime += Time.deltaTime;
                yield return null;
            }
            skillButton.wait.SetActive(false);
            canUseSkill = true;
            Debug.Log("Skill Ready!!");
        }
        
        public override bool CanUseSkill() => canUseSkill;

        public override void UseSkill(params object[] _params)
        {
            StartCoroutine(ShowCor());
        }
        
        public IEnumerator ShowCor()
        {
            StartCoroutine(WaitCoolTimeCor());
            
            // 적 몬스터 
            List<MonsterBase> monsterList = new List<MonsterBase>(GameManager.Instance.monsterList);

            foreach (var monster in monsterList)
            {
                // MonsterBase monster = monsterList[i];
                if (!monster.gameObject.activeSelf)
                    continue;
                
                GameObject go = Instantiate(prefab);
                go.transform.parent = GameManager.Instance.game.effectParent;
                ParticleSystem particle = go.GetComponent<ParticleSystem>();
                particle.Play();
                go.transform.position = monster.transform.position;
                particleList.Add(particle);
                monster.Hit(damage);
                yield return new WaitForSeconds(0.05f);
            }

            yield return new WaitForSeconds(1f);

            foreach (var particle in particleList)
            {
                Destroy(particle);
            }
        }
    }
}