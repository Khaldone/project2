using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

namespace Billiards.Presentation.Login
{
    public class LoginUIHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nativeLoginButtonText;
        private LoginEntryPoint _entryPoint;
        private IEnumerable<INativeAuthService> _nativeAuthServices;

        [Inject]
        public void Construct(LoginEntryPoint entryPoint, IEnumerable<INativeAuthService> nativeAuthServices)
        {
            _entryPoint = entryPoint;
            _nativeAuthServices = nativeAuthServices;
        }

        private void Start()
        {
            if (_nativeLoginButtonText != null)
            {
                var authService = _nativeAuthServices?.FirstOrDefault();
                if (authService != null)
                {
                    _nativeLoginButtonText.text = $"Login with {authService.PlatformName}";
                }
                else
                {
                    _nativeLoginButtonText.text = "Native Login Unavailable";
                }
            }
        }

        public void OnLoginButtonPressed()
        {
            // .Forget() tells UniTask to fire this off without 
            // forcing the button click method to wait for it.
            _entryPoint.TryLoginSequenceAsync().Forget();
            Debug.Log("Pressed");
        }

        public void OnNativeLoginButtonPressed()
        {
            _entryPoint.TryNativeLoginSequenceAsync().Forget();
            Debug.Log("Native Login Pressed");
        }
    }
}