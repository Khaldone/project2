// Attached to: 3D_Inspection_Stage
using System.Threading.Tasks;
using UnityEngine;
using Billiards.CoreDomain.Services;

public class Cue3DViewerWidget : MonoBehaviour
{
    [SerializeField] private Transform _spawnAnchor;
    [SerializeField] private float _rotationSpeed = 45f; // Degrees per second

    // Injected or referenced Asset Delivery Service
    private IAssetDeliveryService _assetDelivery;
    private GameObject _currentCueMesh;


    private void Update()
    {
        // Slowly spin the cue so the player can admire the 3D details
        if (_currentCueMesh != null)
        {
            _currentCueMesh.transform.Rotate(0, _rotationSpeed * Time.deltaTime, 0);
        }
    }


    public async Task LoadAndRenderCueAsync(string addressableKey)
    {
        // 1. CRITICAL MEMORY MANAGEMENT: Release the previous 4K model from RAM immediately
        if (_currentCueMesh != null)
        {
            //_assetDelivery.ReleaseAsset(_currentCueMesh);
            _currentCueMesh = null;
        }


        // 2. Fetch the new model over the air (or from local cache)
        _currentCueMesh = await _assetDelivery.LoadAssetAsync<GameObject>(addressableKey);

        // 3. Parent it to the anchor and reset its transform
        if (_currentCueMesh != null)
        {
            _currentCueMesh.transform.SetParent(_spawnAnchor, false);
            _currentCueMesh.transform.localPosition = Vector3.zero;
        }
    }
}