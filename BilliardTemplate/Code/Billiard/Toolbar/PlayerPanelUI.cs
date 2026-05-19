using System.Collections.Generic;
using ibc.trajectory;
using UnityEngine;
using UnityEngine.UI;

namespace ibc.game
{
    public class PlayerPanelUI : MonoBehaviour
    {
        [Header("Refs")] [SerializeField] private Image _avatar;
        [SerializeField] private Image _avatarRing;
        [SerializeField] private Transform _ballsStripRoot;
        [SerializeField] private GameObject _ballIconPrefab;
            
        [Header("Options")] [SerializeField] private bool _isRightSide;

        private readonly List<GameObject> _spawnedIcons = new();
        private BallColorList _colorList;

        public void Setup(BallColorList colorList, Sprite avatar)
        {
            _colorList = colorList;
            if (avatar) _avatar.sprite = avatar;
            SetActiveHighlight(false);
            SetTimerFill(1f);
        }

        public void SetActiveHighlight(bool active)
        {
            var c = _avatar.color;
            c.a = active ? 1f : 0.7f;
            _avatar.color = c;
        }

        public void SetTimerFill(float t) 
        {
            if (_avatarRing)
            {
                _avatarRing.type = Image.Type.Filled;
                _avatarRing.fillMethod = Image.FillMethod.Radial360;
                _avatarRing.fillClockwise = !_isRightSide;
                _avatarRing.fillAmount = Mathf.Clamp01(t);
            }
        }

        public void RenderPocketedBalls(IReadOnlyList<(int, BallType)> pocketedForThisPlayer)
        {
            foreach (var go in _spawnedIcons) Destroy(go);
            _spawnedIcons.Clear();

            foreach (var (id, ballType) in pocketedForThisPlayer)
            {
                var go = Instantiate(_ballIconPrefab, _ballsStripRoot);
                var icon = go.GetComponent<BallIconUI>();
                if (icon != null && _colorList != null)
                {
                    var color = _colorList.GetColor(id);
                    icon.Setup(color, ballType, id);
                }
                _spawnedIcons.Add(go);
            }

            if (_isRightSide)
            {
                for (int i = 0; i < _ballsStripRoot.childCount; i++)
                    _ballsStripRoot.GetChild(i).SetSiblingIndex(_ballsStripRoot.childCount - 1 - i);
            }
        }
    }
}