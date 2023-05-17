using UnityEngine;

namespace RandomFortress.Game
{
    public class TileInfo : MonoBehaviour
    {
        public enum TileType { Block, Build, Road }
        public TileType type = TileType.Block;
    }
}