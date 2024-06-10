
using UnityEngine;

namespace RandomFortress
{
    public class SoloMode : GameMode
    {
        Vector3[] waypoints = new[] {
            new Vector3(-380f, 870f, 0f), new Vector3(-380f, 420f, 0f), new Vector3(410f, 420f, 0f),
            new Vector3(410f, 120f, 0f), new Vector3(-380f, 120f, 0f), new Vector3(-380f, -180f, 0f),
            new Vector3(410f, -180f, 0f), new Vector3(410f, -600f, 0f)
        };
        
        public override void Init()
        {
            boardUI.gameObject.SetActive(true);
            
            GameManager.Instance.myPlayer.gameMap = gameMap[0];
            GameManager.Instance.SetupPlayer();
        }
        
        public override int GetRoadWayLength() => waypoints.Length;
        
        public override Vector3 GetRoadPos(int index) => waypoints[index];
        
        // 솔로모드에서는 본인배경만 바뀐다
        // public override void ChangeMapStage(int playerIndex = 0)
        // {
        //     gameMap[0].ChangeMapStage();
        // }
    }
}