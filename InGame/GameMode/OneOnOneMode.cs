using RandomFortress.Game;
using RandomFortress.Manager;
using RandomFortress.Scene;
using UnityEngine;

namespace GamePlay.GameMode
{
    public class OneOnOneMode : GameMode
    {
        Vector3Int[] roadWayPoints = new[]
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
        
        public override void Init()
        {
            GameManager.Instance.SetupPlayer();
        }
        
        public override Vector3Int GetRoadWayPoint(int index) => roadWayPoints[index];
        
        public override int GetRoadWayLength() => roadWayPoints.Length;
    }
}