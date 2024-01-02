using UnityEngine;

namespace RandomFortress.Game
{
    public class PoisonDebuff : DebuffBase
    {
        [SerializeField] private int poisonDuration; // 독 지속시간
        [SerializeField] private int poisonDamage; // 독 총 데미지

        private int damagePerTic; // 틱당 데미지
        private float ticTime; // 한틱 경과시간
        private float elapsedTime; // 경과 시간
        private float tickInterval = 0.5f; // 데버프 틱 간격
        
        public override void Init(params object[] valus)
        {
            poisonDamage = (int)valus[0];
            poisonDuration = (int)valus[1];
            
            monster = GetComponent<MonsterBase>();
        }

        public override void UpdateDebuff()
        {
            ticTime += Time.deltaTime;

            // 틱당 데미지
            if (ticTime >= tickInterval)
            {
                elapsedTime += ticTime;
                monster.Hit(damagePerTic);
                ticTime = 0;
            }

            // 디버프 종료
            if (elapsedTime >= poisonDuration)
            {
                Remove();
            }
        }
        
        public override void Remove()
        {
            monster.RemoveDebuff(this);
            Destroy(this);
        }
    }
}