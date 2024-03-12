using Spine.Unity;
using UnityEngine;
using Animation = Spine.Animation;

namespace RandomFortress.Common.Utils
{
    public static class SpineUtils
    {
        public static float GetAnimationDuration(SkeletonAnimation skeletonAnimation, string animationName)
        {
            // SkeletonData에서 애니메이션을 찾습니다.
            Animation animation = skeletonAnimation.Skeleton.Data.FindAnimation(animationName);
            if (animation != null)
            {
                // 애니메이션의 지속 시간을 반환합니다.
                return animation.Duration;
            }
            else
            {
                Debug.LogWarning("Animation not found: " + animationName);
                return 0;
            }
        }
    }
}