using System;
using UnityEngine;

public class PlayerCharacterControler : MonoBehaviour
{
    public PlayerControls _inputActions;
    public CharacterController _characterController;
    public Transform _characterModelTransform;

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
    public Vector2 _yLimits;

    [Space(10)]
    [Header("Power related")]
    private bool _canDrainPower = false;

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Awake()
    {
        _camera = Camera.main;

        _verticalVelocity = _minVerticalVelocity;

        _inputActions = new PlayerControls();

        _inputActions.Gameplay.Move.performed += ctx => _movementDirection = ctx.ReadValue<Vector2>();
        _inputActions.Gameplay.Move.canceled += ctx => _movementDirection = Vector2.zero;

        _inputActions.Gameplay.Jump.performed += ctx => StartJump();

        _inputActions.Gameplay.Movementability.performed += ctx => MovementAbility();
        _inputActions.Gameplay.Powerdrain.performed += ctx => PowerDrain();

        _inputActions.Gameplay.Lightrangedattack.performed += ctx => LightRangedAttack();
    }

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void MoveCharacter()
    {
        //Make charactermovement follow camera orientation
        Vector3 cameraForward = new Vector3(_camera.transform.forward.x, 0, _camera.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(_camera.transform.right.x, 0, _camera.transform.right.z).normalized;

        // Apply movement & jump
        _verticalVelocity.y = Mathf.Clamp(_verticalVelocity.y + _minVerticalVelocity.y * Time.deltaTime, _minVerticalVelocity.y, _jumpVelocity.y);
        Vector3 movement = (cameraRight * _movementDirection.x + cameraForward * _movementDirection.y).normalized;

        //Vector3 movement = new Vector3(_movementDirection.x, _verticalVelocity.y, _movementDirection.y);
        movement.y = _verticalVelocity.y;
        movement *= _speed * Time.deltaTime;
        _characterController.Move(movement);

        Vector3 lookvector = new Vector3(movement.x, 0, movement.z);
        if (lookvector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookvector);
            _characterModelTransform.rotation = targetRotation;
        }

        //_slerpTimer += Time.deltaTime;
        //if (_targetRotation != targetRotation)
        //{
        //    _slerpTimer = 0f;
        //    _targetRotation = targetRotation;
        //}

        //if (_movementDirection.sqrMagnitude > 0.01f)
        //{
        //    _characterModelTransform.rotation = Quaternion.Slerp(_characterController.transform.rotation, _targetRotation, _speed * _slerpTimer / _slerpDuration);
        //}
    }

    private void StartJump()
    {
        if (!_characterController.isGrounded)
        {
            Debug.Log("not grounded");
            return;
        }

        Debug.Log("Jump");

        _isJumping = !_isJumping;
        _verticalVelocity = _jumpVelocity;
    }

    private void MovementAbility()
    {
        Debug.Log("Movement ability");
    }

    private void PowerDrain()
    {
        Debug.Log("PowerDrain");
    }

    private void LightRangedAttack()
    {
        Debug.Log("Fire");
    }

}
