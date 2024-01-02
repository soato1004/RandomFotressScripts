using UnityEngine;
using UnityEngine.Tilemaps;

namespace GamePlay.GameMode
{
    public class GameMode : MonoBehaviour
    {
        public Transform Map;
        public Transform bulletParent;
        public Transform monsterParent;
        public Transform effectParent;
        public Transform skillParent;

        public virtual void Init()
        {
            
        }
        
        public virtual Vector3Int GetRoadWayPoint(int index)
        {
            return Vector3Int.zero;
        }
        
        public virtual int GetRoadWayLength()
        {
            return -1;
        }
    }
}