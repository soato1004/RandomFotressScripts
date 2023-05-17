using Common;
using RandomFortress.Common;
using RandomFortress.Common.Util;
using RandomFortress.Game;
using UnityEngine;

namespace RandomFortress.Manager
{
    public class EffectManager : Singleton<EffectManager>
    {
        public override void Reset()
        {
            JTDebug.LogColor("EffectManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("EffectManager Terminate");
        }
        
        public void ShowHitEffect(int index, Vector3 targetPos, float scale = 5f)
        {
            GameObject go = ObjectPoolManager.Instance.GetEffect("Hit ",index);
            go.transform.position = targetPos;
            go.transform.localScale = new Vector3(scale,scale,scale);
            ParticleSystem particle = go.GetComponent<ParticleSystem>();
            Destroy(go, particle.main.duration);
        }
        
        public void ShootingEffect(int index, Vector3 targetPos, float scale = 5f)
        {
            GameObject go = ObjectPoolManager.Instance.GetEffect("Flash ", index);
            go.transform.position = targetPos;
            go.transform.localScale = new Vector3(scale,scale,scale);
            ParticleSystem particle = go.GetComponent<ParticleSystem>();
            Destroy(go, particle.main.duration);
        }
    }
}