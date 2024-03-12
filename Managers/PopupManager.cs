using RandomFortress.Common;
using RandomFortress.Common.Utils;
using RandomFortress.Game;
using UnityEngine;

namespace RandomFortress.Manager
{
    public class PopupManager : Singleton<PopupManager>
    {
        [SerializeField] private PopupBase[] PopupPrefab;
        
        public override void Reset()
        {
            JTDebug.LogColor("PopupManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("PopupManager Terminate");
        }
    }
}