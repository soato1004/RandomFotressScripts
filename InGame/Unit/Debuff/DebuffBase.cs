using UnityEngine;

namespace RandomFortress.Game
{
    public class DebuffBase : MonoBehaviour
    {
        protected MonsterBase monster;
        
        public virtual void Init(params object[] valus) {}
        
        public virtual void UpdateDebuff(){}
        
        public virtual void Remove(){}
    }
}