using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "ScriptableObjects/SkillData", order = 1)]
    public class SkillData : ScriptableObject
    {
        public int index;        
        public string skillName;
        public float coolTime;
        public int[] dynamicData = { 0, 0, 0 };
    }
    
    // main 스킬은 1000 + index
    // sub 스킬은 2000 + index
}