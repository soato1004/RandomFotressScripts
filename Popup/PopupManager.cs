


using UnityEngine;

namespace RandomFortress
{
    public class PopupManager : Singleton<PopupManager>
    {
        [SerializeField] private PopupBase[] PopupPrefab;
        
        public override void Reset()
        {
            JustDebug.LogColor("PopupManager Reset");
        }
    }
}