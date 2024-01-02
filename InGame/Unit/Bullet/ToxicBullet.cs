using RandomFortress.Common.Utils;
using RandomFortress.Data;
using RandomFortress.Manager;
using UnityEngine;

namespace RandomFortress.Game
{
    public class ToxicBullet : BulletBase
    {
        protected int poisonDamage; // 독 총 데미지 
        protected int poisonDuration; // 독 지속시간. 100 = 1초
        private PoisonDebuff poisonDebuff;
        
        // public float movementSpeed = 5f; // 이동 속도
        public float rotationSpeed = 300f; // 회전 속도

        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            poisonDamage = (int)values[1];
            poisonDuration = (int)values[2];
            
            base.Init(gPlayer, index, monster, values);
            
            // 시작 이펙트
            SpawnManager.Instance.GetEffect(BulletData.startName, transform.position);
        }

        protected override void Hit()
        {
            if (Target != null)
            {
                // 최초 피격시 데미지 
                Target.Hit(Damage);
            }
            
            // 피격 이펙트
            AudioManager.Instance.PlayOneShot("rock_impact_heavy_slam_01");
            TargetPos.y += 8f;
            SpawnManager.Instance.GetEffect(BulletData.hitName, TargetPos);


            DelayCallUtils.DelayCall(0.2f, () =>
            {
                // 피격 지역에 독웅덩이를 생성
                GameObject go = Instantiate(ResourceManager.Instance.GetPrefab("ToxicStiky"),
                    GameManager.Instance.gameMode.bulletParent);
                
                // 모드별 크기다르게
                if (MainManager.Instance.gameType == GameType.Solo)
                {
                    go.transform.localScale = new Vector3(1.333f, 1.333f, 1.333f);
                }

                TargetPos.y -= 7f;
                go.transform.position = TargetPos;
            
                object[] paramsArr = { poisonDamage, poisonDuration };
                go.GetComponent<StikyBase>().Init(player, paramsArr);
            });
        }
    }
}