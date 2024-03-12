using RandomFortress.Constants;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress.Game
{
    public class NormalMonster : MonsterBase
    {
        [SerializeField] protected SkeletonAnimation spineBody = null;
        [SerializeField] private Animator anim = null; // 몬스터 모션

        public override void Init(GamePlayer targetPlayer, int index, int hp, MonsterType monsterType)
        {
            // Anima2d를 사용하는 몬스터는 SortingGroup을 사용
            anim = GetComponent<Animator>();
            if (anim != null)
                GetComponent<SortingGroup>().sortingOrder = (int)GameLayer.Monster;
            
            // Spine을 사용하는 몬스터는 SkeletonAnimation을 사용
            spineBody = GetComponent<SkeletonAnimation>();
            if (spineBody != null) 
                spineBody.GetComponent<MeshRenderer>().sortingOrder = (int)GameLayer.Monster;
            
            base.Init(targetPlayer, index, hp, monsterType);
        }
        
        protected override void SetState(MonsterState state)
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
                    case MonsterState.walk: spineBody.AnimationState.SetAnimation(0, "Run2", true); break;
                    // case GameConstants.MonsterState.attack: spineBody.AnimationState.SetAnimation(0, "Idle", true); break;
                    // case GameConstants.MonsterState.hit: spineBody.AnimationState.SetAnimation(0, "Idle", true);    break;
                    case MonsterState.die: spineBody.AnimationState.SetAnimation(0, "Death", false);    break;
                }
            }
            
        }
    }
}