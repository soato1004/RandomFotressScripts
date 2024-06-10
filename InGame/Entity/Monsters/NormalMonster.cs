using DG.Tweening;


using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress
{
    public class NormalMonster : MonsterBase
    {
        protected override void Awake()
        {
            base.Awake();
            hpBar = SpawnManager.Instance.GetHpBar(transform.position);
        }
        
        public override void Init(GamePlayer targetPlayer, int index, int hp, int unitID, MonsterType type)
        {
            // Anima2d를 사용하는 몬스터는 SortingGroup을 사용
            anim = GetComponent<Animator>();
            if (anim != null)
                GetComponent<SortingGroup>().sortingOrder = (int)GameLayer.Monster;
            
            // Spine을 사용하는 몬스터는 SkeletonAnimation을 사용
            spineBody = GetComponent<SkeletonAnimation>();
            if (spineBody != null) 
                spineBody.GetComponent<MeshRenderer>().sortingOrder = (int)GameLayer.Monster;
            
            base.Init(targetPlayer, index, hp, unitID, type);
        }
        
        public override void SetState(MonsterState state)
        {
            if (anim != null)
            {
                anim.SetBool("isHit", false);
                anim.SetBool("isIdle", true);
                anim.SetBool("isAttack", false);
                anim.SetBool("isDie", false);
                anim.SetBool("isWalk", false);
                
                monsterState = state;
                switch (monsterState)
                {
                    case MonsterState.idle: anim.SetBool("isIdle", true); break;
                    case MonsterState.walk: anim.SetBool("isWalk", true); break;
                    case MonsterState.attack: anim.SetBool("isAttack", true); break;
                    case MonsterState.hit: anim.SetBool("isHit", true); break;
                    case MonsterState.die: anim.SetBool("isDie", true); break;
                    default: anim.SetBool("isIdle", true); break;
                }
            }
            else
            {
                monsterState = state;
                switch (monsterState)
                {
                    case MonsterState.idle:
                        spineBody.AnimationState.SetAnimation(0, "Idle", true);
                        break;
                    case MonsterState.walk:
                        spineBody.AnimationState.SetAnimation(0, "Run", true);
                        break;

                    case MonsterState.die:
                        spineBody.AnimationState.SetAnimation(0, "Death", false);
                        break;

                    case MonsterState.stun:
                        spineBody.AnimationState.SetAnimation(0, "Stun", true);
                        break;
                    
                    default: 
                        spineBody.AnimationState.SetAnimation(0, "Idle", true);
                        break;
                }
            }
        }
        
        protected override void SetNextWay()
        {
            targetPos = player.GetNext(wayPoint);
            dir = (targetPos - transform.position).normalized;
            totalDistance = Vector3.Distance(targetPos, transform.position);
            
            float yRotate = (dir.x > 0) ^ (spineBody != null) ? 180 : 0;
            
            transform.DORotate(new Vector3(0, yRotate, 0), 0);
        }
    }
}