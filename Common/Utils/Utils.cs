using System;
using System.Collections;

using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Animation = Spine.Animation;

namespace RandomFortress
{
    public static class Utils
    {
        /// <summary> 딜레이 코루틴 </summary>
        public static Coroutine DelayCallCoroutine(MonoBehaviour mono, float seconds, Action action)
        {
            if (mono == null || !mono.gameObject.activeInHierarchy)
                return null;
            
            return mono.StartCoroutine(DelayCallCoroutine(seconds, action));
        }
        
        /// <summary> 코루틴 지정 시간 딜레이 후 콜백 </summary>
        private static IEnumerator DelayCallCoroutine(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }
        
        /// <summary> 타임스케일값에 영향을 받는 대기 </summary>
        public static IEnumerator WaitForSeconds(float seconds)
        {
            float timer = 0;
            while (timer < seconds)
            {
                timer += Time.deltaTime * GameManager.Instance.TimeScale;
                yield return null;
            }
        }
        
        /// <summary> 딥카피 </summary>
        public static T DeepCopy<T>(T other) where T : ScriptableObject
        {
            // // 새로운 인스턴스를 만듭니다.
            // T clone = ScriptableObject.CreateInstance<T>();
            //
            // // 원본 ScriptableObject의 필드를 복사합니다.
            // System.Reflection.FieldInfo[] fields = typeof(T).GetFields();
            // foreach (var field in fields)
            // {
            //     field.SetValue(clone, field.GetValue(other));
            // }
            
            // 원본 ScriptableObject를 JSON으로 직렬화합니다.
            string json = JsonUtility.ToJson(other);

            // 새로운 인스턴스를 만듭니다.
            T clone = ScriptableObject.CreateInstance<T>();

            // JSON 데이터를 새로운 인스턴스에 역직렬화합니다.
            JsonUtility.FromJsonOverwrite(json, clone);

            return clone;
        }
        
        // 배열중 랜덤으로 선택하여 반환
        public static int ChooseWithProbabilities(params float[] probs)
        {
            var total = 0f;
            foreach (var prob in probs)
            {
                total += prob;
            }
            
            var randomPoint = UnityEngine.Random.value * total; // 0 ~ total 값.

            for (var i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                    return i;
                randomPoint -= probs[i];
            }

            return probs.Length - 1;
        }
        
        public static int GetRandomIndexWithWeight(float[] weights)
        {
            float totalWeight = 0;
        
            // 모든 가중치의 합을 계산합니다.
            foreach (float weight in weights)
            {
                totalWeight += weight;
            }

            // 랜덤한 값(0과 가중치 합 사이)을 선택합니다.
            float randomPoint = UnityEngine.Random.value * totalWeight;

            // 선택된 랜덤 값이 어떤 가중치 범위에 속하는지 찾아 인덱스를 반환합니다.
            for (int i = 0; i < weights.Length; i++)
            {
                if (randomPoint < weights[i])
                {
                    return i;
                }
                randomPoint -= weights[i];
            }

            return weights.Length - 1; // 만약 모든 가중치를 넘어선 경우, 마지막 인덱스 반환
        }
        
        // 지정된 크기안에 최대크기의 Image 조정
        public static void ImageSizeToFit(int width, int height, ref Image image)
        {
            float imageWidth = image.sprite.rect.width;
            float imageHeight = image.sprite.rect.height;

            // Sprite의 가로 세로 비율 계산
            float widthRatio = width / imageWidth;
            float heightRatio = height / imageHeight;

            // 비율에 맞게 크기 조정
            float scaleRatio = Mathf.Min(widthRatio, heightRatio);
            Vector3 newScale = new Vector3(scaleRatio, scaleRatio, 1f);
            image.rectTransform.sizeDelta = new Vector2(imageWidth * scaleRatio, imageHeight * scaleRatio);
        }
        
        /// <summary>
        /// 스크린 좌표를 월드 좌표로 전환합니다.
        /// PointerEventData 객체가 PressEvent를 받은 상태에서 호출되어야 합니다.
        /// </summary>
        public static Vector3 ScreenToWorldPoint(PointerEventData data)
        {
            var cam = data.pressEventCamera;
            if (cam == null)
                cam = Camera.main;

            // 2D orthographic camera
            if (cam.orthographic)
                return cam.ScreenToWorldPoint(data.position).ExNewZ(0f);

            // 3D perspective camera
            var pointerDrag = data.pointerDrag;
            if (pointerDrag == null)
                return Vector3.zero;

            return ScreenToWorldPointOnPlane(data.position, cam, pointerDrag.transform.position.z);
        }
	    
        /// <summary>
        /// 3D perspective camera 에서 스크린 좌표를 월드 좌표로 변환합니다.
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <param name="cam"></param>
        /// <param name="z">변환할 월드 좌표에서의 z 값</param>
        public static Vector3 ScreenToWorldPointOnPlane(Vector3 screenPosition, Camera cam, float z) 
        {
            var ray = cam.ScreenPointToRay(screenPosition);
            var plane = new Plane(Vector3.forward, new Vector3(0, 0, z));

            if (plane.Raycast(ray, out var distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }
        
        // 확률의 덧셈을 계산하는 함수
        public static double CalculateCombinedProbability(double probA, double probB)
        {
            // 두 확률이 독립적이라고 가정
            double combinedProb = 1 - (1 - probA) * (1 - probB);
            return combinedProb;
        }

        // 기대 데미지를 역산하여 새로운 데미지 배율을 계산하는 함수
        public static double CalculateExpectedDamageMultiplier(double probA, double multiplierA, double probB, double multiplierB)
        {
            // 각 버프가 적용되지 않을 확률
            double nonCritProb = (1 - probA) * (1 - probB);

            // 기대 데미지 계산
            double expectedDamage = nonCritProb * 1 + probA * (1 - probB) * multiplierA + probB * (1 - probA) * multiplierB + probA * probB * multiplierA * multiplierB;

            // 기대 데미지를 기반으로 새로운 데미지 배율을 역산
            double newMultiplier = expectedDamage / (1 - CalculateCombinedProbability(probA, probB)) - 1;

            return newMultiplier;
        }
    }

    public static class JustDebug
    {
        private static bool _isDebug = false;
        
        public static void LogColor(object message, string color = "red")
        {
            if (!_isDebug) return;
                
#if UNITY_EDITOR
            Debug.Log($"<color={color}>{message}</color>");
#else
            Debug.Log($"<color={color}>{message}</color>");
#endif
        }

        public static void Log(object message, bool isForce = false)
        {
            if (!_isDebug && !isForce) return;
            
            Debug.Log(message);
        }

        public static void LogError(object message)
        {
            Debug.LogError(message);
        }
    }
    
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
    
    public static class DelayCallUtils
    {
        public static Coroutine DelayCallCoroutine(MonoBehaviour mono, float seconds, Action action)
        {
            if (mono == null || !mono.gameObject.activeInHierarchy)
                return null;
            
            return mono.StartCoroutine(DelayCallCoroutine(seconds, action));
        }
        private static IEnumerator DelayCallCoroutine(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action?.Invoke();
        }
        
        // 배열중 랜덤으로 선택하여 반환
        public static int ChooseWithProbabilities(params float[] probs)
        {
            var total = 0f;
            foreach (var prob in probs)
            {
                total += prob;
            }
            
            var randomPoint = UnityEngine.Random.value * total; // 0 ~ total 값.

            for (var i = 0; i < probs.Length; i++)
            {
                if (randomPoint < probs[i])
                    return i;
                randomPoint -= probs[i];
            }

            return probs.Length - 1;
        }
        
        public static int GetRandomIndexWithWeight(float[] weights)
        {
            float totalWeight = 0;
        
            // 모든 가중치의 합을 계산합니다.
            foreach (float weight in weights)
            {
                totalWeight += weight;
            }

            // 랜덤한 값(0과 가중치 합 사이)을 선택합니다.
            float randomPoint = UnityEngine.Random.value * totalWeight;

            // 선택된 랜덤 값이 어떤 가중치 범위에 속하는지 찾아 인덱스를 반환합니다.
            for (int i = 0; i < weights.Length; i++)
            {
                if (randomPoint < weights[i])
                {
                    return i;
                }
                randomPoint -= weights[i];
            }

            return weights.Length - 1; // 만약 모든 가중치를 넘어선 경우, 마지막 인덱스 반환
        }
    }
}