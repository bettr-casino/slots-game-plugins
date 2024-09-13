using UnityEditor;
using UnityEngine;

namespace Bettr.Editor
{
    public class BettrTimeScaleController : EditorWindow
    {
        float _timeScale = 1f;

        [MenuItem("Bettr/Window/Time Scale Controller")]
        public static void ShowWindow()
        {
            GetWindow<BettrTimeScaleController>("Time Scale Controller");
        }

        void OnGUI()
        {
            GUILayout.Label("Control Time Scale", EditorStyles.boldLabel);

            _timeScale = EditorGUILayout.Slider("Time Scale", _timeScale, 0f, 2f);

            if (GUILayout.Button("Apply Time Scale"))
            {
                Time.timeScale = _timeScale;
            }

            if (GUILayout.Button("Reset to Normal"))
            {
                Time.timeScale = 1.0f;
            }
        }
    }
}