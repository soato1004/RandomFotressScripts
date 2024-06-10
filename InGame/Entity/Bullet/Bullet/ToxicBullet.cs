using System.Collections;


using UnityEngine;

namespace RandomFortress
{
    public class ToxicBullet : BulletBase
    {
        [SerializeField] protected GameObject bulletImage;
        [SerializeField] private ToxicStiky toxicStiky;
        
        // 독웅덩이
        [SerializeField] protected int poisonDamage; // 틱별 독 데미지 
        [SerializeField] protected int poisonDuration; // 독 지속시간. 100 = 1초
        [SerializeField] protected int tickTime; // 틱 시간. 100 = 1초
        
        // 독 디버프
        // private PoisonDebuff poisonDebuff;
        
        // public float movementSpeed = 5f; // 이동 속도
        public float rotationSpeed = 300f; // 회전 속도

        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            base.Init(gPlayer, index, monster, values);
         
            bulletImage = transform.GetChild(0).gameObject;
            
            poisonDuration = (int)values[1];
            poisonDamage = (int)values[2];
            tickTime = (int)values[3];
            
            bulletImage.gameObject.SetActive(true);
            toxicStiky.gameObject.SetActive(false);
        }
        
        protected override IEnumerator HitCor()
        {
            Hit();
            
            yield return null;

            CreateToxicStiky();
            
            yield return new WaitForSeconds(poisonDuration/100f);
            
            Remove();
        }
        
        protected override void Hit()
        {
            if (Target == null) return;
            
            SoundManager.Instance.PlayOneShot("bullet_hit_base");
            Target.Hit(Damage, textType);
            TargetPos.y += 8f;
            SpawnManager.Instance.GetEffect(BulletData.hitEffName, TargetPos);

        }
        
        private void CreateToxicStiky()
        {
            bulletImage.gameObject.SetActive(false);
            toxicStiky.gameObject.SetActive(true);
            
            // 피격 지역에 독웅덩이를 생성
            GameObject go = toxicStiky.gameObject;
            go.SetActive(true);
                
            // TODO: 모드별 크기다르게
            if (GameManager.Instance.gameType == GameType.Solo)
            {
                go.transform.localScale = new Vector3(1.333f, 1.333f, 1.333f);
            }

            TargetPos.y -= 7f;
            go.transform.position = TargetPos;
            
            object[] paramsArr = { poisonDamage, tickTime };
            toxicStiky.Init(player, paramsArr);
        }
    }
}