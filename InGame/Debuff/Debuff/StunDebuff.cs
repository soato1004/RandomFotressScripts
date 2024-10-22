

using UnityEngine;

namespace RandomFortress
{
    public class StunDebuff : DebuffBase
    {
        public override void Init(params object[] values)
        {
            base.Init(values);
            
            monster.SetState(MonsterState.stun);
        }

        public override void UpdateDebuff()
        {
            elapsedTimer += Time.deltaTime * GameManager.I.gameSpeed;

            // 스턴
            if (monster != null)
            {
                monster.moveDebuff *= 0;
            }
            
            // 디버프 종료
            if (elapsedTimer >= duration)
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