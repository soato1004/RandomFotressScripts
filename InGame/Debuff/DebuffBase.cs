

using UnityEngine;

namespace RandomFortress
{
    public class DebuffBase : MonoBehaviour
    {
        [SerializeField] protected float duration = 0f; // 버프 지속시간
        [SerializeField] protected float elapsedTimer; // 버프 경과 시간
        [SerializeField] protected float tickInterval; // 데버프 틱 간격
        [SerializeField] protected float tickTimer = 0; // 틱 타이머
        
        protected MonsterBase monster; // 현재 디버프 적용중인 타겟
        public DebuffIndex debuffIndex { get; protected set; } // 디버프 인덱스값
        
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
                int stage = GameManager.I.myPlayer.stageProcess / 10;
                float i = (10 - (float)stage) / 10;
                duration *= i;
            }
        }
    }
}