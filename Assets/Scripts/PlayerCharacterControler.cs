using Mono.Cecil.Cil;
using System;
using UnityEngine;

public class PlayerCharacterControler : MonoBehaviour
{

    public PlayerControls InputActions;

    [SerializeField]
    private CharacterController _characterController;
    [SerializeField]
    private Transform _characterModelTransform;

    [SerializeField]
    private float _speed;

    private Vector2 _movementDirection;

    [SerializeField]
    private Vector3 _minVerticalVelocity;
    private Vector3 _verticalVelocity;


    [Space(10)]
    [Header("Jump attributes")]
    // Jump related variables
    [SerializeField]
    private Vector3 _jumpVelocity;
    private bool _isJumping = false;

    [Space(10)]
    [Header("Camera")]
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private Camera _camera;

    [Space(10)]
    [Header("Power related")]
    [SerializeField]
    private PowerBase _selectedPower;
    [SerializeField]
    private RectTransform _crosshair;
    [SerializeField]
    private Transform _shootPoint;
    private bool _canDrainPower = false;

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Awake()
    {
        _camera = Camera.main;

        _verticalVelocity = _minVerticalVelocity;

        InputActions = new PlayerControls();

        InputActions.Gameplay.Move.performed += ctx => _movementDirection = ctx.ReadValue<Vector2>();
        InputActions.Gameplay.Move.canceled += ctx => _movementDirection = Vector2.zero;

        InputActions.Gameplay.Jump.performed += ctx => StartJump();

        InputActions.Gameplay.Movementability.performed += ctx => MovementAbility();
        InputActions.Gameplay.Powerdrain.performed += ctx => PowerDrain();

        InputActions.Gameplay.Lightrangedattack.performed += ctx => LightRangedAttack();
        InputActions.Gameplay.Heavyrangedattack.performed += ctx => HeavyRangedAttack();
    }

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        InputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        InputActions.Gameplay.Disable();
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
            _characterModelTransform.rotation = Quaternion.Slerp(_characterModelTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            //_characterModelTransform.rotation = targetRotation;
        }
    }

    private void StartJump()
    {
        if (!_characterController.isGrounded)
        {
            return;
        }

        _isJumping = !_isJumping;
        _verticalVelocity = _jumpVelocity;
    }

    private void MovementAbility()
    {
        _selectedPower.MovementAbility();
    }

    private void PowerDrain()
    {
        if(_canDrainPower == false)
        {
            return;
        }
        Debug.Log("PowerDrain");
    }

    private void LightRangedAttack()
    {
        Debug.Log("Fire");
        Vector3 crosshairPoint = _camera.ScreenToWorldPoint(_crosshair.position);
        Vector3 targetDirection = -(crosshairPoint - (_shootPoint.position + new Vector3(0, 0, 0.5f))).normalized;
        _selectedPower.FireLightAttack(_characterModelTransform.position, targetDirection);
    }

    private void HeavyRangedAttack()
    {
        //_selectedPower.FireHeavyAttack();
    }

}
