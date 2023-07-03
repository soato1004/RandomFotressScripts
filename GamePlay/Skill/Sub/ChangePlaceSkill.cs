using System.Collections;

using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    /// <summary>
    /// 조건없이 타워이동. 타워가 지어진 자리일경우 자리교체
    /// </summary>
    public class ChangePlaceSkill : BaseSkill
    {
        private GameObject prefab;
        private SkillButton skillButton;
        // private int damage = 100;
        private float coolTime = 60;
        private bool canUseSkill = true;
        public float Skill_CoolTime => coolTime; // 스킬의 사용딜레이
        public float Skill_CurrentCoolTime { get; private set; } // 현재 스킬 대기시간

        public override void Init(GamePlayer gPlayer, SkillButton button)
        {
            player = gPlayer;
            skillButton = button;
            canUseSkill = true;
            skillType = SkillType.Sub;
            _skillActions.Add(SkillAction.Touch_Start); // 최초 클릭시
            _skillActions.Add(SkillAction.Touch_OtherTower); // 두번째 클릭시
            prefab = ResourceManager.Instance.GetPrefab("Skill_1_1");
        }

        public override bool CanUseSkill() => canUseSkill;

        private IEnumerator WaitCoolTimeCor()
        {
            canUseSkill = false;
            Skill_CurrentCoolTime = 0;
            skillButton.wait.SetActive(true);
            while (coolTime > Skill_CurrentCoolTime)
            {
                if (GameManager.Instance.isGameOver)
                    yield break;
                
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
            if (player.Towers == null)
                return;
            
            int a, b;
            a = b = -1;
            
            TowerBase[] Towers = player.Towers;
            TowerBase Tower1 = GameManager.Instance.focusTower;
            TowerBase Tower2 = values[0] as TowerBase;
            
            Tower1.SetFocus(false);
            
            for (int i = 0; i < Towers.Length; ++i)
            {
                if (Towers[i] == Tower1)
                {
                    a = i;
                }
                if (Towers[i] == Tower2)
                {
                    b = i;
                }
            }
            
            if (a >= 0 && b >= 0)
            {
                Tower1.Swap(Tower2);
            
                // 인덱스 변경
                (Towers[a], Towers[b]) = (Towers[b], Towers[a]);
                
                Skill_CurrentCoolTime = 0;
            }
            
            StartCoroutine(WaitCoolTimeCor());
        }

        public IEnumerator UseSkillCor(params object[] values)
        {
            yield return null;
        }
    }
}