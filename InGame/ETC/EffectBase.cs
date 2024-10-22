using System;
using System.Collections;
using UnityEngine;

namespace RandomFortress
{
    public class EffectBase : EntityBase
    {
        [SerializeField] private float lifeTime = -1;
        
        private ParticleSystem pa;
        private EntityBase entity;
        private Vector3 scaleOri;
        
        protected override void Awake()
        {
            base.Awake();
            scaleOri = transform.localScale;
        }

        public override void Reset()
        {
            ParticleSystem[] array = GetComponentsInChildren<ParticleSystem>();
            foreach (var pa in array)
            {
                pa.Clear();
                pa.Stop();
                pa.Play();
            }
        }
        
        public void Play()
        {
            Reset();
            StartCoroutine(PlayCor());
        }
        
        public IEnumerator PlayCor()
        {
            yield return new WaitForSeconds(lifeTime);
            
            Release();
        }
        
        public override void Release()
        {
            transform.localScale = scaleOri;
            if (Pool != null)
                Pool.Release(gameObject);
            else
                Destroy(gameObject);
        }
    }
}