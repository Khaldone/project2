using UnityEngine;

namespace ibc.ai
{
    public class AITest : MonoBehaviour
    {
        
        [SerializeField] private AIManager _aiManager;
        [SerializeField] private Billiard _billiard;
        [SerializeField] private int _settingsIndexTarget;

        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                if (_aiManager.IsThinking)
                {
                    _aiManager.StopThinking(StopThinkingReason.UserRequest);
                }
                else
                {
                    _aiManager.StartThinking(_billiard.State, _settingsIndexTarget);
                }
            }
        }
        
    }
}