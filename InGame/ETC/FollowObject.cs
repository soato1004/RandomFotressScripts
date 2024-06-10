using UnityEngine;

namespace RandomFortress
{
    public class FollowObject : MonoBehaviour
    {
        public Transform target; // 따라갈 대상 게임 오브젝트의 Transform
        private bool isFollowing = true; // 따라가는 상태를 확인하는 플래그

        void Update()
        {
            // 타겟이 존재하면, 타겟의 위치로 이동
            if (target != null && isFollowing)
            {
                transform.position = target.position;
            }
            // 타겟이 삭제되었으면, 자신도 삭제
            else if (target == null)
            {
                Destroy(gameObject);
            }
        }

        // 타겟을 설정하는 메소드
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        // 따라가는 기능을 중지하거나 재개하는 메소드
        public void ToggleFollowing(bool follow)
        {
            isFollowing = follow;
        }
    }
}