using RandomFortress.Common;
using RandomFortress.Common.Util;

namespace Photon
{
    public class AdsManager : Singleton<AdsManager>
    {
        public override void Reset()
        {
            JTDebug.LogColor("AdsManager Reset");
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("AdsManager Terminate");
        }

        public void ShowBanner()
        {
            
        }
    }
}