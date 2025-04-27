using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControler : MonoBehaviour
{
    public PlayerControls _inputActions;
    [Header("Camera")]
    public Camera _mainCamera;

    [Space(10)]
    [Header("Positions")]
    public Transform _cameraOrbitPointX;
    public Transform _cameraOrbitPointY;    

    [Space(10)]
    [Header("FOVs")]
    public float _normalFOV;
    public float _aimFOV;

    [Space(10)]
    [Header("Settings")]
    public float _cameraRotSpeed;
    public Vector2 _yLimits;
    public float _lerpDuration;
    private float _lerpTimer;

    private Vector2 _cameraMoveDirection;

    private bool _changeFOV = false;
    private bool _isAiming = false;

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void Awake()
    {
        _inputActions = new PlayerControls();

        _inputActions.Gameplay.MoveCamera.performed += ctx => _cameraMoveDirection = ctx.ReadValue<Vector2>();
        _inputActions.Gameplay.MoveCamera.canceled += ctx => _cameraMoveDirection = Vector2.zero;

        _inputActions.Gameplay.Aim.performed += ctx => StartAim();
        _inputActions.Gameplay.Aim.canceled += ctx => StartAim();

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
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ UPDATE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 

    // Update is called once per frame
    void Update()
    {
        if (_cameraMoveDirection != Vector2.zero)
        {
            MoveCamera();
        }

        if(_changeFOV && _mainCamera.fieldOfView != _aimFOV)
        {
            Aim();
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
        _changeFOV = !_changeFOV;
        _isAiming = !_isAiming;
        _lerpTimer = 0f;
        Debug.Log("Started Zooming");
    }

    private void Aim()
    {
        if(_changeFOV)
        {
            Debug.Log("Zooming");
            _lerpTimer += Time.deltaTime;
            ChangeFOV();

            if(_lerpTimer >= _lerpDuration)
            {
                _changeFOV = false;
                _lerpTimer = 0f;
                SetFOVAfter();
            }
        }
    }

    private void ChangeFOV()
    {
        if (_isAiming)
        {
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _aimFOV, _lerpTimer / _lerpDuration);
        }
        else
        {
            _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, _normalFOV, _lerpTimer / _lerpDuration);
        }
    }

    private void SetFOVAfter()
    {
        if (_isAiming)
        {
            _mainCamera.fieldOfView = _normalFOV;
        }
        else
        {
            _mainCamera.fieldOfView = _aimFOV;
        }
    }
}
