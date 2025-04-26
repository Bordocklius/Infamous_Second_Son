using UnityEngine;

public class PlayerCharacterControler : MonoBehaviour
{
    public PlayerControls _inputActions;
    public CharacterController _characterController;
    public Transform _ccTransform;
    public Transform _pcTransform;

    public Transform _orientation;
    
    public float _speed;

    private Vector2 _movementDirection;

    public Vector3 _minVerticalVelocity;
    private Vector3 _verticalVelocity;
    

    [Space(10)]
    [Header("Jump attributes")]
    // Jump related variables
    public Vector3 _jumpVelocity;
    private bool _isJumping = false;

    [Space(10)]
    [Header("Camera")]
    public Transform _cameraOrbitPointX;
    public Transform _cameraOrbitPointY;
    public float _rotationSpeed;
    public float _cameraRotSpeed;
    private Vector2 _cameraMoveDirection;
    public Camera _camera;
    private Transform _cameraPos;
    Quaternion _targetRotation;
    public float _slerpDuration;
    private float _slerpTimer;
    

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Awake()
    {
        _camera = Camera.main;
        _cameraPos = _camera.transform;

        _ccTransform = _characterController.transform;

        _verticalVelocity = _minVerticalVelocity;

        _inputActions = new PlayerControls();

        _inputActions.Gameplay.Move.performed += ctx => _movementDirection = ctx.ReadValue<Vector2>();
        _inputActions.Gameplay.Move.canceled += ctx => _movementDirection = Vector2.zero;

        _inputActions.Gameplay.Jump.performed += ctx => StartJump();

        _inputActions.Gameplay.MoveCamera.performed += ctx => _cameraMoveDirection = ctx.ReadValue<Vector2>();
        _inputActions.Gameplay.MoveCamera.canceled += ctx => _cameraMoveDirection = Vector2.zero;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    private void OnEnable()
    {
        _inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Gameplay.Disable();
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ UPDATE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Update()
    {               
        MoveCharacter();

        if(_cameraMoveDirection != Vector2.zero)
        {
            MoveCamera();
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void MoveCharacter()
    {
        //Vector3 viewingDirection = _ccTransform.position - new Vector3(_orientation.position.x, _ccTransform.position.y, _orientation.position.z);
        //_orientation.forward = viewingDirection.normalized;

        //Vector3 inputDirection = _movementDirection.y * _orientation.forward + _movementDirection.x * _orientation.right;
        //if (inputDirection != Vector3.zero)
        //{
        //    _ccTransform.forward = Vector3.Slerp(_ccTransform.forward, inputDirection, _rotationSpeed * Time.deltaTime);
        //}


        Vector3 cameraForward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(_camera.transform.right.x, 0, _camera.transform.right.z).normalized;

        _verticalVelocity.y = Mathf.Clamp(_verticalVelocity.y + _minVerticalVelocity.y * Time.deltaTime, _minVerticalVelocity.y, _jumpVelocity.y);
        Vector3 movement = (cameraRight * _movementDirection.x + cameraForward * _movementDirection.y).normalized;
        //Vector3 movement = new Vector3(_movementDirection.x, _verticalVelocity.y, _movementDirection.y);
        movement.y = _verticalVelocity.y;
        movement *= _speed * Time.deltaTime;
        _characterController.Move(movement);
        _pcTransform.position = _ccTransform.position;

        Vector3 lookvector = new Vector3(movement.x, 0, movement.z);
        Quaternion targetRotation = Quaternion.LookRotation(lookvector);
        //_characterController.transform.rotation = Quaternion.Slerp(_characterController.transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime);

        _slerpTimer += Time.deltaTime;
        if (_targetRotation != targetRotation)
        {
            _slerpTimer = 0f;
            _targetRotation = targetRotation;
        }

        if (_movementDirection.sqrMagnitude > 0.01f)
        {
            _characterController.transform.rotation = Quaternion.Slerp(_characterController.transform.rotation, _targetRotation, _speed * _slerpTimer / _slerpDuration);
        }
    }

    private void StartJump()
    {
        if(!_characterController.isGrounded)
        {
            Debug.Log("not grounded");
            return;
        }

        Debug.Log("Jump");

        _isJumping = !_isJumping;
        _verticalVelocity = _jumpVelocity;
    }

    private void MoveCamera()
    {
        float xRot = _cameraMoveDirection.x * _cameraRotSpeed * Time.deltaTime;
        float yRot = _cameraMoveDirection.y * _cameraRotSpeed * Time.deltaTime;
        _cameraOrbitPointX.rotation *= Quaternion.AngleAxis(xRot, Vector3.up);
        _cameraOrbitPointY.rotation *= Quaternion.AngleAxis(-yRot, Vector3.right);
    }

}
