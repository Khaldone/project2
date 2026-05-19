using Billiards.Bootstrapper.Scopes;
using UnityEngine;
using VContainer;
using VContainer.Unity;
namespace Billiards.Presentation
{
    public class Scope : MonoBehaviour
    {
        [SerializeField] HomeLifetimeScope homeLifetimeScope;
        
        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Y))
            {
                //Debug.Log(homeLifetimeScope.GetParent());
            }
        
        }
    }
}
