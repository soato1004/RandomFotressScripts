
using UnityEngine;

namespace RandomFortress
{
    public class OneOnOneMode : GameMode
    {
        Vector3[] waypoints = new[]
        {
            new Vector3(-400f,30f,0f), new Vector3(-400f,-575f,0f), new Vector3(-200f,-575f,0f), 
            new Vector3(-200f,-375f,0f), new Vector3(0f,-375f,0f), new Vector3(0f,-575f,0f), 
            new Vector3(200f,-575f,0f), new Vector3(200f,-175f,0f), new Vector3(-200f,-175f,0f), 
            new Vector3(-200f,25f,0f), new Vector3(400f,25f,0f), new Vector3(400f,-600f,0f)
        };
        
        // public GameObject[] StageBG;
        // private int currentIndex = 0;
        
        public override void Init()     
        {
            boardUI.gameObject.SetActive(true);

            GameManager.Instance.myPlayer.gameMap = gameMap[0];
            GameManager.Instance.otherPlayer.gameMap = gameMap[1];
            GameManager.Instance.SetupPlayer();
        }
        
        // otherPlayer 에겐 y값을 +860
        public override int GetRoadWayLength() => waypoints.Length;

        public override Vector3 GetRoadPos(int index) => waypoints[index];
        

        // public override void ChangeMapStage(int playerIndex = 0)
        // {
        //     gameMap[playerIndex].ChangeMapStage();
        // }
    }
}