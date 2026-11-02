using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace BiniarosAR
{
    /// AR night preview: dims virtual ambient contribution and enables pergola LEDs.
    /// It cannot physically darken the real world in optical see-through glasses.
    public class DayNightController : MonoBehaviour
    {
        public PergolaConfigurator pergola;
        public ARCameraManager cameraManager;
        public Light virtualSun;
        public float dayIntensity = 1f;
        public float nightIntensity = 0.08f;

        public void SetNight(bool night)
        {
            if (virtualSun != null) virtualSun.intensity = night ? nightIntensity : dayIntensity;
            if (pergola != null) pergola.SetLED(night);
        }
    }
}
