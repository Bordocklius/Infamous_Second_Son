using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControler : MonoBehaviour
{
    public PlayerControls _inputActions;
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
    private LayerMask _rayCastLayerMask;
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

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void Awake()
    {
        _inputActions = new PlayerControls();

        _inputActions.Gameplay.MoveCamera.performed += ctx => _cameraMoveDirection = ctx.ReadValue<Vector2>();
        _inputActions.Gameplay.MoveCamera.canceled += ctx => _cameraMoveDirection = Vector2.zero;

        _inputActions.Gameplay.Aim.performed += ctx => StartAim();
        _inputActions.Gameplay.Aim.canceled += ctx => StopAim();

    }

    private void OnEnable()
    {
        _inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Gameplay.Disable();
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
        Debug.Log(_normalDistanceFromPlayer);
        Debug.Log(_aimDistanceFromPlayer);
        Debug.Log(_normalCameraTransform.position - _playerCharacterTransform.position);
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ UPDATE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 

    // Update is called once per frame
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

        Physics.Raycast(_playerCharacterTransform.position, (_mainCameraTransform.position - _playerCharacterTransform.position).normalized, out RaycastHit hit, _normalDistanceFromPlayer, ~_rayCastLayerMask.value);
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.name);
            _isColliding = true;
            _mainCameraTransform.position = hit.point;
        }
        else
        {
            _isColliding = false;
        }
        
        if(!_isAiming && !_isColliding)
        {
            CollidingLerp();       
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
        Debug.Log("Aiming");
        _changePos = true;
        _isAiming = true;
        _lerpTimer = 0f;
    }

    private void StopAim()
    {
        Debug.Log("Stopped aiming");
        _changePos = true;
        _isAiming = false;
        _lerpTimer = 0f;
    }

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
