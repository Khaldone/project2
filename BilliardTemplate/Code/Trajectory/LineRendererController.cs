/*
 *  Created by Dragutin Sredojevic.
 *  https://www.nitugard.com
 */
using Unity.Collections;
using UnityEngine;

namespace ibc.trajectory
{
    public class LineRendererController : MonoBehaviour
    {
        private LineRenderer _line;

        private NativeArray<Vector3> _points;
        private int _color;

        private void Awake()
        {
            _line = GetComponentInChildren<LineRenderer>();
            _points = new NativeArray<Vector3>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            _line.positionCount = 2;

            Hide();
        }


        public void Hide()
        {
            _line.enabled = false;
        }

        public void Show()
        {
            _line.enabled = true;
        }

        public void SetPoints(Vector3 p1, Vector3 p2)
        {
            _points[0] = p1;
            _points[1] = p2;

            _line.SetPositions(_points);
        }

        public void SetColorNoAlphaChange(Color color)
        {
            color.a = _line.startColor.a;
            _line.startColor = color;

            color.a = _line.endColor.a;
            _line.endColor = color;

        }

        private void OnDestroy()
        {
            _points.Dispose();
        }
    }
}