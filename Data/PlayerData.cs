using UnityEngine;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/PlayerData", order = 1)]
    public class PlayerData : ScriptableObject
    {
        public int id;
        public string nickname;
        public int mainSkill;
        public int subSkill_1;
        public int subSkill_2;
    }
}