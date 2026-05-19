// Assets/_Project/Scenes/02_Spoke_GameArena/Scripts/VFX/ReturnToPoolAfterDelay.cs
using System.Collections;
using UnityEngine;


public class ReturnToPoolAfterDelay : MonoBehaviour
{
    private VFXPoolManager _owningPool;

    public void StartTimer(VFXPoolManager pool, float delay)
    {
        _owningPool = pool;
        StartCoroutine(TimerRoutine(delay));
    }

    private IEnumerator TimerRoutine(float delay)
    {
        // Wait for the particle system to finish flashing
        yield return new WaitForSeconds(delay);


        // Tell the manager we are done
        if (_owningPool != null)
        {
            _owningPool.ReturnSparkToPool(this.gameObject);
        }
    }
    private void OnDisable()
    {
        // Safety cleanup if the scene is unloaded forcefully
        StopAllCoroutines();
    }
}