
using UnityEngine;

namespace RandomFortress
{
    /// <summary>
    /// 타워 손해없는 판매
    /// </summary>
    public class GoodSellSKill : BaseSkill
    {
        public override void Init(int skillIndex, GamePlayer gPlayer, SkillButton button)
        {
            data =  DataManager.Instance.skillDataDic[skillIndex];
            Debug.Log("Skill Create 7002 : " + data.index);
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
            StartCoroutine(WaitSkillUseCor());
        }

        public override void UseSkill(params object[] values)
        {
            if (GameManager.Instance.focusTower == null)
                return;

            int towerPosIndex = GameManager.Instance.focusTower.TowerPosIndex;
            int getGold = GameManager.Instance.focusTower.Info.salePrice*2;
            
            player.SellTower(true);
            StopCoroutine(WaitSkillUseCor());
            
            bool isMine = GameManager.Instance.myPlayer == player;
            GameUIManager.Instance.HideSkillDim(isMine);
            StartCoroutine(WaitCoolTimeCor(_coolTime));
            
            Debug.Log("Skill Use 7002 : " + data.index);
            
            player.SkillEnd(data.index, new object[]{towerPosIndex,getGold});
        }
    }
}