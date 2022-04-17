using UnityEngine;

namespace AntonsCameraController
{
    public class PinchZoomController : ComponentBase
    {
        private float _minZoom;
        private float _maxZoom;

        public void Init(float minZoom, float maxZoom)
        {
            _minZoom = minZoom;
            _maxZoom = maxZoom;
        }

        void Update()
        {
            if (Input.touchCount == 2)
            {
                var touchZero = Input.GetTouch(0);
                var touchOne = Input.GetTouch(1);

                var touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                var touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                var prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                var touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

                var deltaMagnitudedRatio = prevTouchDeltaMag/touchDeltaMag;

                var localPos = T.localPosition;
                
                localPos.z *= deltaMagnitudedRatio;
                localPos.z = Mathf.Clamp(localPos.z, -_maxZoom, -_minZoom);
                T.localPosition = localPos;
            }
        }
    }
}

