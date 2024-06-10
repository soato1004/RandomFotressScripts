

using UnityEngine;

namespace RandomFortress
{
    public class StunDebuff : DebuffBase
    {
        private float elapsedTime; // 경과 시간
        
        public override void Init(params object[] values)
        {
            base.Init(values);
            
            monster.SetState(MonsterState.stun);
        }

        public override void UpdateDebuff()
        {
            elapsedTime += Time.deltaTime * GameManager.Instance.TimeScale;

            // 스턴
            if (monster != null)
            {
                monster.moveDebuff *= 0;
            }
            
            // 디버프 종료
            if (elapsedTime >= duration)
            {
                Remove();
            }
        }
        
        public override void Remove()
        {
            monster.SetState(MonsterState.walk);
            monster.RemoveDebuff(this);
            Destroy(this);
        }
    }
}