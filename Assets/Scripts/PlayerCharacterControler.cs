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
    private Camera _mainCamera;

    [Space(10)]
    [Header("Power related")]
    [SerializeField]
    private PowerBase _currentPower;
    [SerializeField]
    private RectTransform _crosshair;
    [SerializeField]
    private Transform _shootPoint;
    [SerializeField]
    private float _range;
    private bool _canDrainPower = false;
    private bool _isDashing = false;

    [Space(10)]
    [Header("Smoke Dash")]
    [SerializeField]
    private float _smokeDashSpeed;
    [SerializeField]
    private float _dashDuration;
    [SerializeField]
    private LayerMask _playerMask;
    [SerializeField]
    private LayerMask _passableTerrainMask;
    private float _dashTimer;


    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Awake()
    {
        _mainCamera = Camera.main;

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

        if(!_isDashing && _currentPower is SmokePower && _dashTimer >= _dashDuration)
        {
            Physics.IgnoreLayerCollision(6, 8, false);
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void MoveCharacter()
    {
        //Make charactermovement follow camera orientation
        Vector3 cameraForward = new Vector3(_mainCamera.transform.forward.x, 0, _mainCamera.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(_mainCamera.transform.right.x, 0, _mainCamera.transform.right.z).normalized;

        // Apply movement & jump
        _verticalVelocity.y = Mathf.Clamp(_verticalVelocity.y + _minVerticalVelocity.y * Time.deltaTime, _minVerticalVelocity.y, _jumpVelocity.y);
        Vector3 movement = (cameraRight * _movementDirection.x + cameraForward * _movementDirection.y).normalized;

        float movementspeed = _speed;
        if (_isDashing && _currentPower is SmokePower && _dashTimer < _dashDuration)
        {
            _dashTimer += Time.deltaTime;
            Debug.Log("Smokedashing");
            _verticalVelocity.y = 0;
            movementspeed = _smokeDashSpeed;
        }

        movement.y = _verticalVelocity.y;
        movement *= movementspeed * Time.deltaTime;
        _characterController.Move(movement);

        Vector3 lookvector = new Vector3(movement.x, 0, movement.z);
        if (lookvector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookvector);
            _characterModelTransform.rotation = Quaternion.Slerp(_characterModelTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            //_characterModelTransform.rotation = targetRotation;
        }

        if (_isDashing && _dashTimer >= _dashDuration)
        {
            _isDashing = false;
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
        if(_isDashing)
        {
            return;
        }
        Debug.Log("MovementAbility");
        _isDashing = true;
        _dashTimer = 0;

        if(_currentPower is SmokePower)
        {
            Physics.IgnoreLayerCollision(6, 8, true);
        }
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
        Ray ray = _mainCamera.ScreenPointToRay(_crosshair.position);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * _range, Color.red, 2f, true);
        Physics.Raycast(ray, out hit, _range);

        if (hit.collider == null)
        {
            hit.point = ray.origin + ray.direction * _range;
        }

        Debug.DrawLine(ray.origin, hit.point, Color.blue, 2f, true);
        //Debug.DrawRay(_mainCamera.ScreenToWorldPoint(_crosshair.position), _shootPoint.position.normalized * _range, Color.green, 2f, true);

        _currentPower.FireLightAttack(_shootPoint.position, (hit.point - _shootPoint.position).normalized);
    }

    private void HeavyRangedAttack()
    {
        //_selectedPower.FireHeavyAttack();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isDashing && _currentPower is SmokePower)
        {
            Physics.IgnoreCollision(collision.collider, _characterController);
        }
    }

}
