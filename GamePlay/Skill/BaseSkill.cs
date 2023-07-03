using System.Collections.Generic;

using UnityEngine;

namespace RandomFortress.Game
{
    public abstract class BaseSkill : MonoBehaviour
    {
        protected GamePlayer player;
        
        // 특정 행동을 취할때 반응하는 스킬을 찾는다
        public enum SkillAction 
        {
            Touch_Start,
            Touch_End,
            Touch_OtherTower,
        }
        public List<SkillAction> _skillActions = new List<SkillAction>();
        
        // 영웅, 서브 스킬의 구분
        public enum SkillType
        {
            None, 
            Hero, 
            Sub
        }
        public SkillType skillType = SkillType.None;

        public abstract void Init(GamePlayer gPlayer,SkillButton button);
        
        public abstract void UseSkill(params object[] values);

        public abstract bool CanUseSkill();
    }
}