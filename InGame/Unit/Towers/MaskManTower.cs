using DG.Tweening;
using RandomFortress.Data;
using RandomFortress.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomFortress.Game
{
    public class MaskManTower : TowerBase
    {
        [SerializeField] protected int poisonDuration; // 독 지속시간
        [SerializeField] protected int poisonDamage; // 독 총 데미지
        
        public override void Init(GamePlayer targetPlayer, int posIndex, int towerIndex)
        {
            // 초기설정
            player = targetPlayer;
            TowerPosIndex = posIndex;
            SetInfo(DataManager.Instance.TowerDataDic[towerIndex]);

            // 타워 내부정보 리셋
            Reset();
            
            // 타워 초기화
            SetBody();

            // 코루틴으로 업데이트 처리
            StartCoroutine(TowerUpdate());
        }
        
        protected override void SetInfo(TowerData data)
        {
            base.SetInfo(data);
            poisonDuration = (int)data.dynamicData[0];
            poisonDamage = (int)data.dynamicData[1];
        }

        private float particleAngel = 0;
        
        protected override void Shooting()
        {
            if (!isDragging)
            {
                Vector3 oriPos = transform.position;
                body.transform.DOLocalMoveX(-15f, 0.08f).SetEase(Ease.OutBack);
                body.transform.DOMove(oriPos, 0.2f).SetDelay(0.12f);
            }

            // 총알 발사시 타워마다 시작지점이 다르다
            Vector3 flashPos = originPosition;
            flashPos.x += 30.8f;
            flashPos.y += 20.3f;
            flashPos.z = -50f;
            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(flashPos, Info.bulletIndex);
            ToxicBullet bullet = bulletGo.GetOrAddComponent<ToxicBullet>();

            object[] paramsArr = { Info.attackPower, poisonDamage, poisonDuration };
            bullet.Init(player, Info.bulletIndex, Target, paramsArr);
            
            //
            TotalDamege += Info.attackPower;
            dps += Info.attackPower;
        }
    }
}