using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControler : MonoBehaviour
{
    public PlayerControls InputActions;

    [Header("Camera")]
    [SerializeField]
    private Camera _mainCamera;
    [SerializeField]
    private Camera _normalCamera;
    [SerializeField]
    private Camera _aimCamera;

    [Space(10)]
    [Header("Positions")]
    [SerializeField]
    private Transform _cameraOrbitPointX;
    [SerializeField]
    private Transform _cameraOrbitPointY;
    [SerializeField]
    private Transform _mainCameraTransform;
    [SerializeField]
    private Transform _normalCameraTransform;
    [SerializeField]
    private Transform _aimCameraTransform;
    [SerializeField]
    private Transform _playerCharacterTransform;

    [Space(10)]
    [Header("Settings")]
    [SerializeField]
    private float _cameraRotSpeed;
    [SerializeField]
    private Vector2 _yLimits;
    [SerializeField]
    private LayerMask _playerRayCastMask;
    [SerializeField]
    private LayerMask _attackRayCastMask;
    [SerializeField]
    private float _lerpDuration;
    private float _lerpTimer;
    private float _colidingLerpTimer;

    private Vector2 _cameraMoveDirection;

    private bool _changePos = false;
    private bool _isAiming = false;

    private float _normalDistanceFromPlayer;
    private float _aimDistanceFromPlayer;
    private bool _isColliding = false;

    [SerializeField]
    private AudioSource _audioSource;

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void Awake()
    {
        // Hook into input actions
        InputActions = new PlayerControls();

        InputActions.Gameplay.MoveCamera.performed += ctx => _cameraMoveDirection = ctx.ReadValue<Vector2>();
        InputActions.Gameplay.MoveCamera.canceled += ctx => _cameraMoveDirection = Vector2.zero;

        InputActions.Gameplay.Aim.performed += ctx => StartAim();
        InputActions.Gameplay.Aim.canceled += ctx => StopAim();

    }

    private void OnEnable()
    {
        InputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        InputActions.Gameplay.Disable();
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ START ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
    void Start()
    {
        if(_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        _mainCamera.fieldOfView = _normalCamera.fieldOfView;
        _normalDistanceFromPlayer = Vector3.Distance(_normalCameraTransform.position, _playerCharacterTransform.position);
        _aimDistanceFromPlayer = Vector3.Distance(_aimCameraTransform.position, _playerCharacterTransform.position);
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ UPDATE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
    void Update()
    {
        if (_cameraMoveDirection != Vector2.zero)
        {
            MoveCamera();
        }

        if(_changePos)
        {
            Aim();
        }

        // checks for collision with obstacles
        CheckForCollision();

        if (Input.GetKeyDown(KeyCode.M))
        {
            if(_audioSource.mute)
            {
                _audioSource.mute = false;
            }
            else
            {
                _audioSource.mute = true;
            }
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
    private void MoveCamera()
    {
        // Apply yaw (horizontal rotation)
        float yaw = _cameraMoveDirection.x * _cameraRotSpeed * Time.deltaTime;
        _cameraOrbitPointX.rotation *= Quaternion.AngleAxis(yaw, Vector3.up);

        // Apply pitch (vertical rotation)
        float pitch = _cameraMoveDirection.y * _cameraRotSpeed * Time.deltaTime;
        float currentPitch = _cameraOrbitPointY.localEulerAngles.x;
        if (currentPitch > 180f)
        {
            currentPitch -= 360f;
        }
        currentPitch = Mathf.Clamp(currentPitch - pitch, _yLimits.x, _yLimits.y);
        _cameraOrbitPointY.localRotation = Quaternion.AngleAxis(currentPitch, Vector3.right);
    }

    private void StartAim()
    {
        _changePos = true;
        _isAiming = true;
        _lerpTimer = 0f;
    }

    private void StopAim()
    {
        _changePos = true;
        _isAiming = false;
        _lerpTimer = 0f;
    }

    // Activate lerp when aiming
    private void Aim()
    {
        if(_changePos)
        {
            _lerpTimer += Time.deltaTime;
            ChangePos();

            if(_lerpTimer >= _lerpDuration)
            {
                _changePos = false;
                _lerpTimer = 0f;
                SetPosAfter();
            }
        }
    }

    // Lerp between positions and FOVs
    private void ChangePos()
    {
        if (_isAiming)
        {
            _mainCameraTransform.position = Vector3.Lerp(_mainCameraTransform.position, _aimCameraTransform.position, _lerpTimer / _lerpDuration);
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _aimCamera.fieldOfView, _lerpTimer / _lerpDuration);
        }
        else
        {
            _mainCameraTransform.position = Vector3.Lerp(_mainCameraTransform.position, _normalCameraTransform.position, _lerpTimer / _lerpDuration);
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _normalCamera.fieldOfView, _lerpTimer / _lerpDuration);
        }
    }

    private void SetPosAfter()
    {
        if (_isAiming)
        {
            _mainCameraTransform.position = _aimCameraTransform.position;
            _mainCamera.fieldOfView = _aimCamera.fieldOfView;
        }
        else
        {
            _mainCameraTransform.position = _normalCameraTransform.position;
            _mainCamera.fieldOfView = _normalCamera.fieldOfView;
        }
    }

    // Check if camera is colliding with an object
    private void CheckForCollision()
    {        
        LayerMask layermask = (1 << 6) | (1 << 7);
        Physics.Raycast(_playerCharacterTransform.position, (_mainCameraTransform.position - _playerCharacterTransform.position).normalized, out RaycastHit hit, _normalDistanceFromPlayer, ~layermask.value);
        if (hit.collider != null)
        {
            _isColliding = true;
            _mainCameraTransform.position = hit.point;
        }
        else
        {
            _isColliding = false;
        }

        if (!_isAiming && !_isColliding)
        {
            CollidingLerp();
        }
    }

    private void CollidingLerp()
    {
        _colidingLerpTimer += Time.deltaTime;
        ChangePos();
        if (_colidingLerpTimer >= _lerpDuration)
        {
            _changePos = false;
            _colidingLerpTimer = 0f;
            SetPosAfter();
        }
    }
}
