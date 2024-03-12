using UnityEngine;
using UnityEngine.Serialization;

namespace RandomFortress.Data
{
    [CreateAssetMenu(fileName = "BulletData", menuName = "ScriptableObjects/BulletData", order = 1)]
    public class BulletData : ScriptableObject
    {
        public int index;
        public string bulletName;   //
        public string prefabName = "";     
        public string startEffName = "";    
        public string hitEffName = "";    
    }
}