// Assets/_Project/3_Presentation/TrophyRoad/Pools/TrophyNodePool.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Billiards.Presentation.TrophyRoad.Widgets;

namespace Billiards.Presentation.TrophyRoad.Pools
{
    /// <summary>
    /// Specialized zero-allocation recycling container for structural milestone UI nodes.
    /// </summary>
    public sealed class TrophyNodePool
    {
        private readonly GameObject _prefabAsset;
        private readonly Transform _spawnParentContainer;
        private readonly Stack<TrophyNodeWidget> _inactivePoolStack = new();
        private readonly List<TrophyNodeWidget> _activePooledNodes = new();

        public IReadOnlyList<TrophyNodeWidget> ActiveNodes => _activePooledNodes;

        public TrophyNodePool(GameObject prefab, Transform parent, int initialPrewarmCapacity = 10)
        {
            _prefabAsset = prefab;
            _spawnParentContainer = parent;

            // Pre-warm the pool allocation limits to prevent runtime hitching
            for (int i = 0; i < initialPrewarmCapacity; i++)
            {
                var go = UnityEngine.Object.Instantiate(_prefabAsset, _spawnParentContainer);
                go.SetActive(false);
                var widget = go.GetComponent<TrophyNodeWidget>();
                if (widget != null) _inactivePoolStack.Push(widget);
            }
        }

        public TrophyNodeWidget SpawnNode()
        {
            TrophyNodeWidget widget;
            if (_inactivePoolStack.Count > 0)
            {
                widget = _inactivePoolStack.Pop();
            }
            else
            {
                // Fallback allocation if pool capacity bounds are broken
                var go = UnityEngine.Object.Instantiate(_prefabAsset, _spawnParentContainer);
                widget = go.GetComponent<TrophyNodeWidget>();
            }

            widget.gameObject.SetActive(true);
            // Push layout execution order behind the slider track
            widget.transform.SetAsLastSibling();
            _activePooledNodes.Add(widget);
            return widget;
        }

        public void RecycleAllActiveNodes()
        {
            for (int i = _activePooledNodes.Count - 1; i >= 0; i--)
            {
                var widget = _activePooledNodes[i];
                widget.gameObject.SetActive(false);
                _inactivePoolStack.Push(widget);
            }
            _activePooledNodes.Clear();
        }
    }
}