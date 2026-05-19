// Assets/_Project/3_Presentation/TrophyRoad/Debug/TrophyRoadDebugSimulator.cs
using UnityEngine;
using VContainer;
using Billiards.Core.Progression;

namespace Billiards.Presentation.TrophyRoad.Debug
{
    /// <summary>
    /// Isolated simulator script utilizing immediate GUI to alter cup progression live.
    /// </summary>
    public sealed class TrophyRoadDebugSimulator : MonoBehaviour
    {
        private ITrophyRoadOrchestrator _orchestrator;

        [Inject]
        public void Construct(ITrophyRoadOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        private void OnGUI()
        {
            if (_orchestrator == null) return;

            GUILayout.BeginArea(new Rect(20, 20, 250, 200), "Trophy Road Simulator", GUI.skin.window);
            GUILayout.Label($"Current Cups: {_orchestrator.CurrentCups}");

            if (GUILayout.Button("Earn Trophies (+20 CP)"))
            {
                _orchestrator.DebugSetCupBalance(_orchestrator.CurrentCups + 20);
            }

            if (GUILayout.Button("Lose Trophies (-20 CP)"))
            {
                int nextVal = Mathf.Max(0, _orchestrator.CurrentCups - 20);
                _orchestrator.DebugSetCupBalance(nextVal);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Reset Progress Track (0 CP)"))
            {
                _orchestrator.DebugSetCupBalance(0);
            }

            GUILayout.EndArea();
        }
    }
}