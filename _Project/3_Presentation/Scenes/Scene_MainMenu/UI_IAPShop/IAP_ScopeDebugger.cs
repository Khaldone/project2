using Billiards.Bootstrapper.Scopes;
using UnityEngine;

namespace Billiards.Presentation
{
    public class IAP_ScopeDebugger : MonoBehaviour
    {
        [SerializeField] IAPShopLifetimeScope iapShopLifetimeScope;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.D))
            {
                //Debug.Log(iapShopLifetimeScope.GetParent());
            }

        }
    }
}
