
using Photon.Pun;
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
            data =  DataManager.I.skillDataDic[skillIndex];
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
            StartCoroutine(WaitAndForceSkillUseCor());
        }

        public override void UseSkill(params object[] values)
        {
            Debug.Log("Use SKill "+data.skillName);
            
            if (GameManager.I.focusTower == null)
                return;

            int towerPosIndex = GameManager.I.focusTower.TowerPosIndex;
            int getGold = GameManager.I.focusTower.Info.salePrice*2;
            
            player.SellTower(true);
            StopCoroutine(WaitAndForceSkillUseCor());
            
            GameManager.I.SetSkillDim(false,player);
            StartCoroutine(WaitCoolTimeCor(_coolTime));
            
            if (player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                player.SkillEnd(data.index, new object[]{towerPosIndex,getGold});
        }
    }
}