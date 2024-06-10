

using UnityEngine;

namespace RandomFortress
{
    public class DebuffBase : MonoBehaviour
    {
        protected float duration = 0f;
        
        protected MonsterBase monster;
        public DebuffIndex debuffIndex { get; protected set; }
        
        // public DebuffType debuffType { get; protected set; }

        public virtual void Init(params object[] values)
        {
            duration = (int)values[0] / 100f;
            monster = GetComponent<MonsterBase>();
            SetBossDebuff();
        }
        
        public virtual void UpdateDebuff(){}
        
        public virtual void Remove(){}
        
        public virtual void CombineDebuff() {}

        protected void SetBossDebuff()
        {
            if (monster.monsterType == MonsterType.Boss)
            {
                int stage = GameManager.Instance.myPlayer.stageProcess / 10;
                float i = (10 - (float)stage) / 10;
                duration *= i;
            }
        }
    }
}