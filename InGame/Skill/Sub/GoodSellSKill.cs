using RandomFortress.Manager;

namespace RandomFortress.Game
{
    /// <summary>
    /// 타워 손해없는 판매
    /// </summary>
    public class GoodSellSKill : BaseSkill
    {
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
        }
        
        public override void SkillStart()
        {
            useSkill = true;
            StartCoroutine(UseSkillCor());
        }

        public override void UseSkill(params object[] values)
        {
            if (GameManager.Instance.focusTower == null)
                return;
            
            player.SellTower(true);
            StopCoroutine(UseSkillCor());
            
            GameUIManager.Instance.HideSkillDim();
            StartCoroutine(WaitCoolTimeCor(_coolTime));
        }
    }
}