using UnityEngine;

namespace RandomFortress
{
    public class AutoPaticleDurationDestroy : MonoBehaviour
    {
        private ParticleSystem pa;
        private EntityBase entity;
        [SerializeField] private float lifeTime = -1;

        void Awake()
        {
            pa = GetComponent<ParticleSystem>();
            entity = GetComponent<EntityBase>();
            if (pa != null)
            {
                // 파티클 시스템이 루프인지 확인하고, 최대 수명을 고려하여 파괴 시점 계산
                if (lifeTime == -1)
                {
                    var mainModule = pa.main;
                    lifeTime = mainModule.duration + mainModule.startLifetime.constantMax;
                }

                if (entity != null)
                    entity.Release();
                else
                    Destroy(gameObject, lifeTime);
            }
            else
            {
                Debug.LogError("AutoParticleDurationDestroy: Not Found ParticleSystem", this);
                // 필요에 따라 여기서 추가적인 처리를 수행할 수 있습니다. 예를 들어, 자동으로 컴포넌트를 삭제할 수 있습니다.
                Destroy(this); // ParticleSystem이 없는 경우 이 컴포넌트만 파괴
            }
        }
    }
}