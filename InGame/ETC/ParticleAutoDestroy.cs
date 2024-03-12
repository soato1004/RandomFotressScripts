using UnityEngine;

namespace RandomFortress.Game
{
    // ParticleAutoDestroy 클래스는 MonoBehaviour를 상속받아 Unity 오브젝트에 부착될 수 있습니다.
    public class ParticleAutoDestroy : MonoBehaviour
    {
        private ParticleSystem particleSystem;

        void Start()
        {
            // 현재 게임 오브젝트에 부착된 ParticleSystem 컴포넌트를 가져옵니다.
            particleSystem = GetComponent<ParticleSystem>();

            // ParticleSystem이 없으면 경고를 출력하고 스크립트 실행을 중지합니다.
            if (particleSystem == null)
            {
                Debug.LogWarning("ParticleSystem component not found on the GameObject.");
                return;
            }

            // ParticleSystem의 duration 값을 가져와서, 이 시간이 지난 후 게임 오브젝트를 자동으로 파괴합니다.
            // 파티클 시스템의 duration과 startLifetime을 비교하여 더 긴 시간을 기준으로 파괴 시간을 결정합니다.
            // 이는 파티클이 모두 사라진 후 오브젝트가 파괴되도록 보장합니다.
            float destroyTime = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
            Destroy(gameObject, destroyTime);
        }
    }

}