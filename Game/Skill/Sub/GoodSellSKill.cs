using System;
using System.Collections;
using System.Collections.Generic;
using RandomFortress.Common.Util;
using RandomFortress.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomFortress.Game.Skill
{
    /// <summary>
    /// 조건없이 타워이동. 타워가 지어진 자리일경우 자리교체
    /// </summary>
    public class GoodSellSKill : BaseSkill
    {
        private GameObject prefab;
        private SkillButton skillButton;
        private int damage = 100;
        private float coolTime = 60;
        private bool canUseSkill = true;
        public float Skill_CoolTime => coolTime; // 스킬의 사용딜레이
        public float Skill_CurrentCoolTime { get; private set; } // 현재 스킬 대기시간

        public override void Init(SkillButton button)
        {
            skillButton = button;
            canUseSkill = true;
            skillType = SkillType.Sub;
            // _skillActions.Add(SkillAction.Touch_Start); // 최초 클릭시
            UseSkill();
        }
        
        public override bool CanUseSkill() => canUseSkill;

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

        public override void UseSkill(params object[] values)
        {
            if (GameManager.Instance.focusTower == null)
                return;
            
            GameManager.Instance.SellTower(true);

            StartCoroutine(WaitCoolTimeCor());
        }

        public IEnumerator UseSkillCor(params object[] values)
        {
            yield return null;
        }
    }
}