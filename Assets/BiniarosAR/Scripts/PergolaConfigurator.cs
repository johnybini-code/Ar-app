using System.Collections.Generic;
using UnityEngine;

namespace BiniarosAR
{
    /// Runtime controls for visible AR presentation.
    /// Assign columns/louvers/ZIP/LED groups after the ROMA hierarchy is mapped.
    public class PergolaConfigurator : MonoBehaviour
    {
        public List<GameObject> columns = new();
        public List<Transform> louverPivots = new();
        public List<GameObject> zipScreens = new();
        public List<Light> ledLights = new();
        public Renderer[] ralRenderers;
        public Vector3 louverAxis = Vector3.right;

        [Range(0, 135)] public float louverAngle;

        public void SetColumnCount(int count)
        {
            count = Mathf.Clamp(count, 0, columns.Count);
            for (int i = 0; i < columns.Count; i++)
                if (columns[i] != null) columns[i].SetActive(i < count);
        }

        public void SetLouverAngle(float degrees)
        {
            louverAngle = Mathf.Clamp(degrees, 0f, 135f);
            foreach (Transform pivot in louverPivots)
            {
                if (pivot == null) continue;
                pivot.localRotation = Quaternion.AngleAxis(louverAngle, louverAxis.normalized);
            }
        }

        public void SetZip(int index, bool visible)
        {
            if (index < 0 || index >= zipScreens.Count || zipScreens[index] == null) return;
            zipScreens[index].SetActive(visible);
        }

        public void SetLED(bool on)
        {
            foreach (Light light in ledLights)
                if (light != null) light.enabled = on;
        }

        public void SetRalColor(Color color)
        {
            foreach (Renderer r in ralRenderers)
            {
                if (r == null) continue;
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
                    else if (m.HasProperty("_Color")) m.SetColor("_Color", color);
                }
            }
        }
    }
}
