using UnityEngine;

namespace BiniarosAR
{
    public class BiniarosRuntimeUI : MonoBehaviour
    {
        public ARPlacementController placement;
        public Transform pergola;
        Rect panel = new Rect(18, 18, 300, 210);
        bool showHelp = true;

        void OnGUI()
        {
            GUI.skin.button.fontSize = 24;
            GUI.skin.label.fontSize = 23;
            GUI.Box(panel, "BINIAROS ROMA AR");
            GUI.Label(new Rect(34, 62, 270, 55), "Σκάναρε το δάπεδο και πάτησε εκεί που θέλεις να μπει η πέργκολα.");
            if (GUI.Button(new Rect(34, 122, 125, 48), "90°")) placement?.Rotate90();
            if (GUI.Button(new Rect(170, 122, 125, 48), "RESET")) placement?.ResetPlacement();
            if (GUI.Button(new Rect(34, 176, 261, 44), showHelp ? "ΚΡΥΨΕ ΟΔΗΓΙΕΣ" : "ΟΔΗΓΙΕΣ")) showHelp = !showHelp;
            if (showHelp)
                GUI.Label(new Rect(18, Screen.height - 90, Screen.width - 36, 70), "1 δάχτυλο: τοποθέτηση • 2 δάχτυλα: περιστροφή • Κλίμακα 1:1");
        }
    }
}
