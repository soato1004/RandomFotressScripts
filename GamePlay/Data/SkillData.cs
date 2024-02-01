using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SkillData", order = 1)]
    public class SkillData : ScriptableObject
    {
        public int index;        
        public string skillName;
        public int damage;
        public float coolTime;

        public SkillData() { }

        public SkillData(SkillData other)
        {

        }
    }
    
    // main 스킬은 1000 + index
    // sub 스킬은 2000 + index
}