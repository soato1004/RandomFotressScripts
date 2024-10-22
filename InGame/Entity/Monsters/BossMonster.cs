using DG.Tweening;


using Spine.Unity;
using UnityEngine;

namespace RandomFortress
{
    public class BossMonster : MonsterBase
    {
        protected override void Awake()
        {
            base.Awake();
        }
        
        public override void Init(GamePlayer targetPlayer, int index, int hp, MonsterType type)
        {
            spineBody = GetComponent<SkeletonAnimation>();
            spineBody.GetComponent<MeshRenderer>().sortingOrder = (int)GameLayer.Boss;
            
            base.Init(targetPlayer, index, hp, type);
            
            hpBar = SpawnManager.I.GetHpBar(transform.position,(int)CanvasLayer.Boss);
            hpBar?.Init(this); // 체력바 설정
        }
        
        // Attack, Attack2, Damage taken, Death, Idle, Idle2, Run, Run2, Skill
        public override void SetState(MonsterState state)
        {
            monsterState = state;
            switch (monsterState)
            {
                case MonsterState.idle:
                    spineBody.AnimationState.SetAnimation(0, "Idle", true);
                    break;
                case MonsterState.walk:
                    spineBody.AnimationState.SetAnimation(0, "Run2", true);
                    break;
                case MonsterState.die:
                    spineBody.AnimationState.SetAnimation(0, "Death", false);
                    break;
                case MonsterState.stun:
                    spineBody.AnimationState.SetAnimation(0, "Idle", false);
                    break;
            }
        }
        
        protected override void SetNextWay()
        {
            targetPos = player.GetMonsterNextTargetPoint(wayPoint);
            dir = (targetPos - transform.position).normalized;
            totalDistance = Vector3.Distance(targetPos, transform.position);
            transform.DORotate(new Vector3(0, (dir.x > 0) ? 0 : 180, 0), 0);
        }
    }
}