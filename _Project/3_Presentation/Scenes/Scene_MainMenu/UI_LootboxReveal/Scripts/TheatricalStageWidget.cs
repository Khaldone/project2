// Attached to: 3D_Theatrical_Stage
using System.Threading.Tasks;
using UnityEngine;


public class TheatricalStageWidget : MonoBehaviour
{
    [SerializeField] private Transform _boxAnchor;
    [SerializeField] private Transform _itemAnchor;
    [SerializeField] private Animator _boxAnimator;

    [Header("VFX Pools")]
    [SerializeField] private ParticleSystem _vfxCommon;
    [SerializeField] private ParticleSystem _vfxEpic;


    private GameObject _currentFloatingItem;


    public void SpawnBox()
    {
        _boxAnimator.gameObject.SetActive(true);
        _boxAnimator.SetTrigger("SlamDown");
    }


    public void PlayEruption(string rarity)
    {
        _boxAnimator.SetTrigger("ShakeAndBurst");

        if (rarity == "Epic") _vfxEpic.Play();
        else _vfxCommon.Play();
    }


    public async Task AnimateItemPopAsync(GameObject itemPrefab, int quantity)
    {
        // 1. Clean up the previous item if there was one
        if (_currentFloatingItem != null) Destroy(_currentFloatingItem);


        // 2. Spawn the new 3D item (e.g., the Dragon Cue piece)
        _currentFloatingItem = Instantiate(itemPrefab, _boxAnchor.position, Quaternion.identity, _itemAnchor);


        // 3. Animate it flying up from the box to the floating anchor
        float duration = 0.8f;
        float elapsed = 0f;
        Vector3 startPos = _boxAnchor.position;
        Vector3 endPos = _itemAnchor.position;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Add a curve here for a juicy pop!
            _currentFloatingItem.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            _currentFloatingItem.transform.Rotate(0, 360 * Time.deltaTime, 0); // Spin
            await Task.Yield();
        }


        _currentFloatingItem.transform.position = endPos;
    }


    public void HideBox()
    {
        _boxAnimator.SetTrigger("FadeOut");
        if (_currentFloatingItem != null) Destroy(_currentFloatingItem);
    }
}