using DG.Tweening;
using RandomFortress.Data;
using RandomFortress.Manager;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomFortress.Game
{
    public class ShinoTower : TowerBase
    {
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

        protected override void Shooting()
        {
            // TODO: 트윈을 사용한 발사모션. Spine 사용시엔 공격모션으로 대체
            if (!isDragging)
            {
                Vector3 oriPos = transform.position;
                body.transform.DOLocalMoveX(-15f, 0.08f).SetEase(Ease.OutBack);
                body.transform.DOMove(oriPos, 0.2f).SetDelay(0.12f);
            }

            // 총알 발사시 타워마다 시작지점이 다르다
            Vector3 flashPos = originPosition;
            flashPos.x += 10.6f;
            flashPos.y += 2.5f;
            flashPos.z = -50f;

            
            GameObject bulletGo = SpawnManager.Instance.GetBullet(flashPos, Info.bulletIndex);
            ShurikenBullet bullet = bulletGo.GetOrAddComponent<ShurikenBullet>();
            bullet.Init(player, Info.bulletIndex, Target, Info.attackPower);
            
            //
            TotalDamege += Info.attackPower;
            dps += Info.attackPower;
        }
    }
}