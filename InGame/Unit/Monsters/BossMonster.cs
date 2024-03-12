using DG.Tweening;
using RandomFortress.Constants;
using Spine.Unity;
using UnityEngine;

namespace RandomFortress.Game
{
    public class BossMonster : MonsterBase
    {
        [SerializeField] protected SkeletonAnimation spineBody;
        
        public override void Init(GamePlayer targetPlayer, int index, int hp, MonsterType monsterType)
        {
            spineBody = GetComponent<SkeletonAnimation>();
            spineBody.GetComponent<MeshRenderer>().sortingOrder = (int)GameLayer.Boss;
            
            base.Init(targetPlayer, index, hp, monsterType);
        }
        
        // Attack, Attack2, Damage taken, Death, Idle, Idle2, Run, Run2, Skill
        protected override void SetState(MonsterState state)
        {
            monsterState = state;
            switch (monsterState)
            {
                case MonsterState.idle:
                    spineBody.AnimationState.SetAnimation(0, "Idle", true);
                    break;
                case MonsterState.walk: spineBody.AnimationState.SetAnimation(0, "Run2", true); break;
                // case GameConstants.MonsterState.attack: spineBody.AnimationState.SetAnimation(0, "Idle", true); break;
                // case GameConstants.MonsterState.hit: spineBody.AnimationState.SetAnimation(0, "Idle", true);    break;
                case MonsterState.die: spineBody.AnimationState.SetAnimation(0, "Death", false);    break;
            }
        }
        
        protected override void SetNextWay()
        {
            targetPos = player.GetNext(wayPoint);
            dir = (targetPos - transform.position).normalized;
            totalDistance = Vector3.Distance(targetPos, transform.position);
            transform.DORotate(new Vector3(0, (dir.x > 0) ? 0 : 180, 0), 0);
        }
    }
}