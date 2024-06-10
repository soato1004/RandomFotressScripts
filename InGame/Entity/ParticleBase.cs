using System.Collections;
using UnityEngine;

namespace RandomFortress
{
    public class ParticleBase : EntityBase
    {
        IEnumerator Start()
        {
            ParticleSystem pa = GetComponent<ParticleSystem>();
            if (pa == null)
            {
                Debug.Log("Not Found ParticleSystem");
                yield break;
            }

            float lifeTime = pa.main.duration;
            yield return new WaitForSeconds(lifeTime);
            Release();
        }
    }
}