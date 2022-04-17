using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace AntonsCameraController
{
    public class FocusedCameraController : ComponentBase
    {
        private const float HorRigMaxAngleDeltaToTarget = 90f;
        private const float VerRigMaxAngleDeltaToTarget = 90f;

        [Header("Target")]
        [SerializeField] private Transform _target;
        [Header("Target rig speed settings")]
        [SerializeField] private float _verticalTargetRotationSpeed = 10f;
        [SerializeField] private float _horizontalTargetRotationSpeed = 10f;
        [SerializeField] private float _verticalRotationDamping = 0.1f;
        [SerializeField] private float _horizontalRotationDamping = 0.1f;
        [Header("Target rig constraints settings")]
        [SerializeField] private float _leftHorizintalRotationConstraints = -90f;
        [SerializeField] private float _rightHorizontalRotationConstraints = 90f;
        [SerializeField] private float _bottomVerticalRotationConstaints = -90f;
        [SerializeField] private float _topVerticalRotationConstraints = 90f;
        [Header("Constraints damping settings")]
        [SerializeField] private float _horizontalConstraintsDamping = 0.1f;
        [SerializeField] private float _verticalConstraintsDamping = 0.1f;
        [SerializeField] private float _horizontalDampingDistance = 30f;
        [SerializeField] private float _verticalDampingDistance = 30f;
        [Header("Zoom settings")]
        [SerializeField] private float _zoomSpeed = 100f;
        [SerializeField] private float _minZoom = 30f;
        [SerializeField] private float _maxZoom = 100f;
        [Header("Axis inverting settings")]
        [SerializeField] private bool _yAxisInversed = false;
        [SerializeField] private bool _xAxisInversed = false;
        

        [Header("Internal references")]
        [SerializeField] private Transform _horizontalTargetRigTransform;
        [SerializeField] private Transform _verticalTargetRigTransform;
        [SerializeField] private Transform _horizontalRigTransform;
        [SerializeField] private Transform _verticalRigTransform;
        [SerializeField] private Transform _cameraTransform;

        private Quaternion _lastValidVerTargetRotation;

        protected override void OnAwake()
        {

#if MOBILE_INPUT
              var zoomCamera = _cameraTransform.GetComponent<UnityEngine.Camera>();
              var pinchZoomController = zoomCamera.gameObject.AddComponent<PinchZoomController>();
              pinchZoomController.Init(_minZoom, _maxZoom);
#endif
        }

        private void Update()
        {
            T.position = _target.position;
            
            UpdateRotation();
#if !MOBILE_INPUT
            UpdateZoom();
#endif
        }

        private void UpdateRotation()
        {

            var deltaTime = Time.deltaTime;

            var horSpeed = _horizontalTargetRotationSpeed;
            var verSpeed = _verticalTargetRotationSpeed;

            var horDelta = Vector3.Angle(_horizontalRigTransform.forward, _horizontalTargetRigTransform.forward);
            var verDelta = Vector3.Angle(_verticalRigTransform.up, _verticalTargetRigTransform.up);

            if (horDelta > HorRigMaxAngleDeltaToTarget)
                horSpeed = 0f;

            if (verDelta > VerRigMaxAngleDeltaToTarget)
                verSpeed = 0f;

            var horAngle = -Vector3.Angle(_horizontalTargetRigTransform.forward, T.forward);
            if (_horizontalTargetRigTransform.forward.x < 0f) horAngle *= -1f;

            var verAngle = Vector3.Angle(_verticalTargetRigTransform.forward, _horizontalTargetRigTransform.forward);
            if (_verticalTargetRigTransform.forward.y < 0f) verAngle *= -1f;

            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                if (horSpeed > 0f)
                {
                    var horizontalAxis = _xAxisInversed ? -GetHorizontal() : GetHorizontal();

                    if (_leftHorizintalRotationConstraints != 0f && _rightHorizontalRotationConstraints != 0f)
                    {
                        var horOverAngle = MathfExt.OutOfRangeDistance(_leftHorizintalRotationConstraints, _rightHorizontalRotationConstraints, horAngle);
                        if (Mathf.Abs(horOverAngle) > 0f && horOverAngle * horizontalAxis <= 0f)
                            horSpeed = 0f;
                    }

                    var horTargetDelta = horSpeed * horizontalAxis * deltaTime;
                    _horizontalTargetRigTransform.Rotate(T.up, horTargetDelta, Space.World);
                }

                if (verSpeed > 0f)
                {
                    var verticalAxis = _yAxisInversed ? GetVertical() : -GetVertical();

                    if (_bottomVerticalRotationConstaints != 0f && _topVerticalRotationConstraints != 0f)
                    {
                        var verOverAngle = MathfExt.OutOfRangeDistance(_bottomVerticalRotationConstaints, _topVerticalRotationConstraints, verAngle);
                        if (Mathf.Abs(verOverAngle) > 0f && verOverAngle * verticalAxis <= 0f)
                            verSpeed = 0f;
                    }

                    var verTargetDelta = verSpeed * verticalAxis * deltaTime;
                    _verticalTargetRigTransform.Rotate(_verticalTargetRigTransform.right, verTargetDelta, Space.World);
                }
            }
            else
            {
                if (_horizontalDampingDistance != 0f)
                {
                    var dampHorOverAngle = MathfExt.OutOfRangeDistance(_leftHorizintalRotationConstraints + _horizontalDampingDistance, _rightHorizontalRotationConstraints - _horizontalDampingDistance, horAngle);
                    _horizontalTargetRigTransform.Rotate(T.up, _horizontalConstraintsDamping * dampHorOverAngle, Space.World);
                }

                if (_verticalDampingDistance != 0f)
                {
                    var dampVerOverAngle = MathfExt.OutOfRangeDistance(_bottomVerticalRotationConstaints + _verticalDampingDistance, _topVerticalRotationConstraints - _verticalDampingDistance, verAngle);
                    _verticalTargetRigTransform.Rotate(_verticalTargetRigTransform.right, _verticalConstraintsDamping * dampVerOverAngle, Space.World);
                }
            }

            var horRotation = _horizontalRigTransform.localRotation;
            var verRotation = _verticalRigTransform.localRotation;

            var targetHorRotation = _horizontalTargetRigTransform.localRotation;
            var targetVerRotation = _verticalTargetRigTransform.localRotation;

            _horizontalRigTransform.localRotation = Quaternion.Lerp(horRotation, targetHorRotation, _horizontalRotationDamping);
            _verticalRigTransform.localRotation = Quaternion.Lerp(verRotation, targetVerRotation, _verticalRotationDamping);

        }
        
        private void UpdateZoom()
        {
            var zoom = GetZoom();

            if (zoom != 0f)
            {
                var localPos = _cameraTransform.localPosition;
                localPos.z *= (1f - Time.deltaTime * _zoomSpeed * zoom);
                localPos.z = Mathf.Clamp(localPos.z, -_maxZoom, -_minZoom);
                _cameraTransform.localPosition = localPos;
            }
        }

        private float GetHorizontal()
        {
            return CrossPlatformInputManager.GetAxis("Horizontal");
        }

        private float GetVertical()
        {
            return CrossPlatformInputManager.GetAxis("Vertical");
        }

        private float GetZoom()
        {
            return Input.GetAxis("Mouse ScrollWheel");
        }
    }
}
