using RandomFortress.Game;
using RandomFortress.Manager;
using RandomFortress.Scene;
using UnityEngine;

namespace RandomFortress.Game
{
    public class OneOnOneMode : GameMode
    {
        Vector3Int[] waypoints = new[]
        {
            new Vector3Int(-9, 0),
            new Vector3Int(-9, -12),
            new Vector3Int(-5, -12),
            new Vector3Int(-5, -8),
            new Vector3Int(-1, -8),
            new Vector3Int(-1, -12),
            new Vector3Int(3, -12),
            new Vector3Int(3, -4),
            new Vector3Int(-5, -4),
            new Vector3Int(-5, 0),      
            new Vector3Int(7, 0),
            new Vector3Int(7, -12)
        };
        
        public GameObject[] StageBG;
        private int currentIndex = 0;
        
        public override void Init()     
        {
            // 화면크기에 맞춰서 배경을 조정한다
            // for(int i=0; i<waypoints.Length; ++i)
                // waypoints[i] = Vector3.Scale(waypoints[i], GameManager.Instance.mainScale);
            
            GameManager.Instance.myPlayer.gameMap = gameMap[0];
            GameManager.Instance.otherPlayer.gameMap = gameMap[1];
            GameManager.Instance.SetupPlayer();
        }
        
        public override int GetRoadWayLength() => waypoints.Length;
        
        public override Vector3 GetRoadPos(int index) => waypoints[index];
        
        // public override void ChangeMapStage(int playerIndex = 0)
        // {
        //     gameMap[playerIndex].ChangeMapStage();
        // }
    }
}