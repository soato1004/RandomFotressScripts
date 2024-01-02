using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/MonsterData", order = 1)]
    public class MonsterData : ScriptableObject
    {
        public int index;
        public string unitName;
        public int hp;
        public int moveSpeed;
        public int monsterType;
        public int damage;
        public int reward;
    }
    
    public enum MonsterIndex
    {
        WolfBlue = 0,
        WolfGreen,
        WolfRed,
        GoblinBerserkr,
        GoblinMagician,
        GoblinWarrior,
        SkeletonArcher,
        SkeletonWarrior,
        SkeletonWizard,
        Ogre,
        OgreBerserkr,
        OgreWarrior,
    }
}