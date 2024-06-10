// 
// 
// using UnityEngine;
//
// namespace RandomFortress
// {
//     public class PoisonDebuff : DebuffBase
//     {
//         [SerializeField] private int poisonDamage; // 독 총 데미지
//         
//         private float ticTime; // 한틱 경과시간
//         private float elapsedTime; // 경과 시간
//         private float tickInterval = 0.5f; // 데버프 틱 간격
//         
//         public override void Init(params object[] values)
//         {
//             base.Init(values);
//             
//             poisonDamage = (int)values[1];
//             tickInterval = (int)values[2];
//         }
//
//         public override void UpdateDebuff()
//         {
//             ticTime += Time.deltaTime * GameManager.Instance.TimeScale;
//
//             // 틱당 데미지
//             if (ticTime >= tickInterval)
//             {
//                 elapsedTime += ticTime;
//                 monster.Hit(poisonDamage);
//                 ticTime = 0;
//             }
//
//             // 디버프 종료
//             if (elapsedTime >= duration)
//             {
//                 Remove();
//             }
//         }
//         
//         public override void Remove()
//         {
//             monster.RemoveDebuff(this);
//             Destroy(this);
//         }
//     }
// }