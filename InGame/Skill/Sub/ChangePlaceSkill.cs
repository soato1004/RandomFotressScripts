using RandomFortress.Manager;

namespace RandomFortress.Game
{
    /// <summary>
    /// 조건없이 타워이동. 타워가 지어진 자리일경우 자리교체
    /// </summary>
    public class ChangePlaceSkill : BaseSkill
    {
        private TowerBase safeTower; // 클릭 두번으로 스왑시에 사용
        
        public float Skill_CoolTime => _coolTime; // 스킬의 사용딜레이
        public float Skill_CoolTime_Timer { get; private set; } // 현재 스킬 대기시간

        public override void Init(int skillIndex, GamePlayer gPlayer, SkillButton button)
        {
            data =  DataManager.Instance.skillDataDic[skillIndex];
            _coolTime = data.coolTime;
            
            player = gPlayer;
            _skillButton = button;
            _canUseSkill = true;
            useSkill = false;
            skillType = SkillType.Sub;
            _skillActions.Add(SkillAction.Touch_Tower); // 최초 클릭시
            _skillActions.Add(SkillAction.Touch_Tower); // 두번째 클릭시
        }
        
        public override void SkillStart()
        {
            useSkill = true;
            safeTower = null;
            GameManager.Instance.canTowerDrag = false;
            
            StartCoroutine(UseSkillCor());
        }
        
        public override void UseSkill(params object[] values)
        {
            if (player.Towers == null)
                return;
            
            if (safeTower == null)
            {
                safeTower = values[0] as TowerBase;
                return;
            }
            
            TowerBase[] Towers = player.Towers;
            TowerBase target = values[0] as TowerBase;
            
            int a, b;
            a = b = -1;
            
            safeTower.SetFocus(false);
            target.SetFocus(false);
            
            for (int i = 0; i < Towers.Length; ++i)
            {
                if (Towers[i] == safeTower)
                {
                    a = i;
                }
                if (Towers[i] == target)
                {
                    b = i;
                }
            }
            
            if (a >= 0 && b >= 0)
            {
                safeTower.Swap(target);
            
                // 인덱스 변경
                (Towers[a], Towers[b]) = (Towers[b], Towers[a]);
                
                Skill_CoolTime_Timer = 0;
            }
            
            StopCoroutine(UseSkillCor());
            
            safeTower = null;
            useSkill = false;
            GameManager.Instance.canTowerDrag = true;
            GameUIManager.Instance.HideSkillDim();
            StartCoroutine(WaitCoolTimeCor(_coolTime));
        }
    }
}