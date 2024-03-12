using RandomFortress.Constants;
using UnityEngine;

namespace RandomFortress.Game
{
    public class DebuffBase : MonoBehaviour
    {
        protected MonsterBase monster;
        public DebuffIndex debuffIndex { get; protected set; }
        
        public DebuffType debuffType { get; protected set; }
        
        public virtual void Init(params object[] values) {}
        
        public virtual void UpdateDebuff(){}
        
        public virtual void Remove(){}
    }
}