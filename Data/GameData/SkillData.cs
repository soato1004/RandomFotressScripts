using UnityEngine;

namespace RandomFortress
{
    public enum SkillType
    {
        Attack,
        MultiAttack,
        Support,
        Opponent
    };

    public enum SkillUseType
    {
        Normal,
        Choice1,
        Choice2
    }
    
    [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/SkillData", order = 1)]
    public class SkillData : ScriptableObject
    {
        public int index;        
        public string skillName;
        public float coolTime;
        public int[] dynamicData = { 0, 0, 0 };
        public SkillType skillType;
        public SkillUseType skillUseType;
    }
}