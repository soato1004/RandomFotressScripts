using RandomFortress.Game;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace RandomFortress.Game
{
    public class GameMode : MonoBehaviour
    {
        public Transform commonMap;
        public GameMap[] gameMap;
        public Transform bulletParent;
        public Transform monsterParent;
        public Transform effectParent;
        public Transform skillParent;
        public Transform uiParent;

        public virtual void Init()
        {
            Debug.Log("재정의안됨");
        }

        public virtual Vector3 GetRoadPos(int index)
        {
            Debug.Log("재정의안됨");
            return Vector3.zero;
        }
        
        public virtual int GetRoadWayLength()
        {
            Debug.Log("재정의안됨");
            return -1;
        }
        
        public virtual void ChangeMapStage(int playerIndex = 0)
        {
            // Debug.Log("재정의안됨");
        }
    }
}