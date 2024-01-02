using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using RandomFortress.Game;
using RandomFortress.Manager;
using RandomFortress.Scene;
using UnityEngine;

namespace GamePlay.GameMode
{
    public class SoloMode : GameMode
    {
        // 크기를 2배로 키운버전
        Vector3Int[] roadWayPoints = new[]
        {
            new Vector3Int(-5, 8),
            new Vector3Int(-5, 4),
            new Vector3Int(3, 4),
            new Vector3Int(3, 1),
            new Vector3Int(-5, 1),
            new Vector3Int(-5, -2),
            new Vector3Int(3, -2),
            new Vector3Int(3, -6)
        };
        
        // 맵이 크고 타워와 몬스터가 작은버전 
        // Vector3Int[] roadWayPoints = new[]
        // {
        //     new Vector3Int(-10, 13),
        //     new Vector3Int(-10, 9),
        //     new Vector3Int(7, 9),
        //     new Vector3Int(7, 13),
        //     new Vector3Int(-3, 13),
        //     new Vector3Int(-3, 5),
        //     new Vector3Int(-6, 5),
        //     new Vector3Int(-6, -2),
        //     new Vector3Int(8, -2),
        //     new Vector3Int(8, 5),
        //     new Vector3Int(1, 5),
        //     new Vector3Int(1, -10),
        //     new Vector3Int(-9, -10),
        //     new Vector3Int(-9, -6),
        //     new Vector3Int(8, -6),
        //     new Vector3Int(8, -10),
        // };

        
        public override void Init()
        {
            GameManager.Instance.myPlayer.Map = Map;
            GameManager.Instance.SetupPlayer();
        }
        
        public override Vector3Int GetRoadWayPoint(int index) => roadWayPoints[index];
        
        public override int GetRoadWayLength() => roadWayPoints.Length;
    }
}