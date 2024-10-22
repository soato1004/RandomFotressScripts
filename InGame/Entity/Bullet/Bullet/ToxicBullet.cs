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
        [SerializeField] protected int poisonDurationMs; // 독 지속시간
        [SerializeField] protected int tickTimeMs; // 틱 인터벌 시간

        [SerializeField] protected TowerBase attacker;
        
        // 독 디버프
        // private PoisonDebuff poisonDebuff;
        
        // public float movementSpeed = 5f; // 이동 속도
        public float rotationSpeed = 300f; // 회전 속도

        public override void Init(GamePlayer gPlayer, int index, MonsterBase monster, params object[] values)
        {
            base.Init(gPlayer, index, monster, values);
         
            bulletImage = transform.GetChild(0).gameObject;
            
            poisonDurationMs = (int)values[1];
            poisonDamage = (int)values[2];
            tickTimeMs = (int)values[3];
            attacker = (TowerBase)values[4];
            
            bulletImage.gameObject.SetActive(true);
            toxicStiky.gameObject.SetActive(false);
        }
        
        protected override IEnumerator HitCor()
        {
            Hit();
            
            yield return null;

            CreateToxicStiky();
            
            yield return new WaitForSeconds(poisonDurationMs/1000f);
            
            Remove();
        }
        
        protected override void Hit()
        {
            if (Target == null) return;
            
            SoundManager.I.PlayOneShot(SoundKey.bullet_hit_base);
            Target.Hit(Damage, textType);
            TargetPos.y += 8f;
            SpawnManager.I.GetBulletEffect(BulletData.hitEffName, TargetPos);

        }
        
        private void CreateToxicStiky()
        {
            bulletImage.gameObject.SetActive(false);
            
            // 피격 지역에 독웅덩이를 생성
            GameObject go = toxicStiky.gameObject;
            
            // TODO: 모드별 크기다르게
            if (GameManager.I.gameType == GameType.Solo)
            {
                go.transform.localScale = new Vector3(1.333f, 1.333f, 1.333f);
            }
            
            TargetPos.y -= 7f;
            go.transform.position = TargetPos;
            go.SetActive(true);
            
            object[] paramsArr = { poisonDamage, tickTimeMs, attacker };
            toxicStiky.Init(player, paramsArr);
            toxicStiky.gameObject.SetActive(true);
        }
    }
}