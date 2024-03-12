using UnityEngine;
using UnityEngine.Tilemaps;

namespace RandomFortress.Game
{
    public class GameMap : MonoBehaviour
    {
        public GameObject[] StageBG;
        public Tilemap roadTilemap;
        public Transform TowerSeatPos;
        
        private int currentIndex = 0;

        public void ChangeMapStage(int playerIndex = 0)
        {
            StageBG[currentIndex].SetActive(false);

            ++currentIndex;
            StageBG[currentIndex].SetActive(true);
        }
    }
}