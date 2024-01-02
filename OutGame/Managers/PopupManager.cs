using RandomFortress.Common;
using RandomFortress.Common.Utils;

namespace RandomFortress.Manager
{
    public class PopupManager : Singleton<PopupManager>
    {
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