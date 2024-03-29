using UnityEngine;

namespace RandomFortress.Common
{
    public class AutoPaticleDurationDestroy : MonoBehaviour
    {
        private ParticleSystem pa;

        void Awake()
        {
            pa = GetComponent<ParticleSystem>();
            if (pa != null)
            {
                Destroy(gameObject, pa.main.duration);
            }
            else
                Debug.Log("Not Found ParticleSystem");
        }
    }
}