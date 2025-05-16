using Mono.Cecil.Cil;
using System;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

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
    private Vector3 _slowedVerticalVelocity;
    private Vector3 _verticalVelocity;


    [Space(10)]
    [Header("Jump attributes")]
    // Jump related variables
    [SerializeField]
    private Vector3 _jumpVelocity;
    private bool _isJumping = false;
    private bool _isHovering = false;

    [Space(10)]
    [Header("Camera")]
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private Camera _mainCamera;

    [Space(10)]
    [Header("Power related")]
    [SerializeField]
    public PowerBase CurrentPower;
    [SerializeField]
    private RectTransform _crosshair;
    [SerializeField]
    private Transform _shootPoint;
    [SerializeField]
    private float _range;
    [SerializeField]
    public static event Action<PlayerCharacterControler> OnPowerReservesChange;
    [SerializeField]
    public static event Action<PlayerCharacterControler> OnHeavyPowerReservesChange;

    private bool _canDrainPower = false;
    private bool _isDashing = false;
    [SerializeField]
    private GameObject _playerParticleSystem;

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

    [Space(10)]
    [Header("Neon Sprint")]
    [SerializeField]
    private float _neonSprintSpeed;
    [SerializeField]
    private float _neonSprintDuration;

    private float _dashTimer;

    private PowerSource _nearbyPowerSource;


    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Awake()
    {
        _mainCamera = Camera.main;

        _verticalVelocity = _minVerticalVelocity;

        InputActions = new PlayerControls();

        InputActions.Gameplay.Move.performed += ctx => _movementDirection = ctx.ReadValue<Vector2>();
        InputActions.Gameplay.Move.canceled += ctx => _movementDirection = Vector2.zero;

        InputActions.Gameplay.Jump.performed += ctx => StartJump();
        InputActions.Gameplay.Hover.performed += ctx => Hover();
        InputActions.Gameplay.Hover.canceled += ctx =>
        {
            _isHovering = false;
            _playerParticleSystem.SetActive(false);
        };

        InputActions.Gameplay.Movementability.performed += ctx => MovementAbility();
        InputActions.Gameplay.Powerdrain.performed += ctx => PowerDrain();

        InputActions.Gameplay.Lightrangedattack.performed += ctx => LightRangedAttack();
        InputActions.Gameplay.Heavyrangedattack.performed += ctx => HeavyRangedAttack();
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _playerParticleSystem.GetComponent<ParticleSystemRenderer>().material = CurrentPower.PowerMaterial;
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

        if (!_isDashing && CurrentPower is SmokePower && _dashTimer >= _dashDuration)
        {
            Physics.IgnoreLayerCollision(6, 8, false);
        }
        if(!_isDashing && _dashTimer >= _dashDuration)
        {
            _dashTimer = 0;
            _playerParticleSystem.SetActive(false);
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void MoveCharacter()
    {
        //Make charactermovement follow camera orientation
        Vector3 cameraForward = new Vector3(_mainCamera.transform.forward.x, 0, _mainCamera.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(_mainCamera.transform.right.x, 0, _mainCamera.transform.right.z).normalized;

        // Apply movement & jump
        if (_isHovering)
        {
            _verticalVelocity.y = Mathf.Clamp(_verticalVelocity.y + _slowedVerticalVelocity.y * Time.deltaTime, _minVerticalVelocity.y, _jumpVelocity.y);
        }
        else
        {
            _verticalVelocity.y = Mathf.Clamp(_verticalVelocity.y + _minVerticalVelocity.y * Time.deltaTime, _minVerticalVelocity.y, _jumpVelocity.y);
        }

        Vector3 movement = (cameraRight * _movementDirection.x + cameraForward * _movementDirection.y).normalized;

        float movementspeed = _speed;
        if (_isDashing && CurrentPower is SmokePower && _dashTimer < _dashDuration)
        {
            _dashTimer += Time.deltaTime;
            _verticalVelocity.y = 0;
            movementspeed = _smokeDashSpeed;
            if (movement == Vector3.zero)
            {
                movement = new Vector3(_mainCamera.transform.forward.x, _verticalVelocity.y, _mainCamera.transform.forward.z).normalized;
            }
        }
        else if(_isDashing && CurrentPower is NeonPower)
        {
            _dashTimer += Time.deltaTime;
            movementspeed = _neonSprintSpeed;
            if(CheckWallInFront())
            {
                _verticalVelocity.y = _characterModelTransform.forward.y + Vector3.up.y;
            }
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

    private bool CheckWallInFront()
    {
        RaycastHit hit;
        //Physics.Raycast(_characterController.transform.position, _characterModelTransform.forward, out hit, 1f, ~_playerMask);        
        return Physics.Raycast(_characterController.transform.position, _characterModelTransform.forward, out hit, 1f, ~_playerMask);
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

    private void Hover()
    {
        if (_characterController.isGrounded && _isHovering)
        {
            return;
        }
        _slowedVerticalVelocity = _minVerticalVelocity + new Vector3(0, CurrentPower.GlideSlow, 0);
        _isHovering = true;
        _playerParticleSystem.SetActive(true);
        Debug.Log("Hover");
    }

    private void MovementAbility()
    {
        if (_isDashing)
        {
            return;
        }

        _isDashing = true;
        _dashTimer = 0;

        if (CurrentPower is SmokePower)
        {
            Debug.Log("Smoke dash");
            Physics.IgnoreLayerCollision(6, 8, true);            
        }
        if(CurrentPower is NeonPower)
        {
            Debug.Log("Neon sprint");
        }

        _playerParticleSystem.SetActive(true);
    }

    private void PowerDrain()
    {
        if (!_canDrainPower || !_nearbyPowerSource.Drainable)
        {
            Debug.Log("Can't drain power");
            return;
        }

        Debug.Log("PowerDrain");
        _nearbyPowerSource.DrainSource();
        if (_nearbyPowerSource.PowerName.ToLower() == "smoke")
        {
            CurrentPower = this.GetComponent<SmokePower>();
        }
        if (_nearbyPowerSource.PowerName.ToLower() == "neon")
        {
            CurrentPower = this.GetComponent<NeonPower>();
            
        }
        CurrentPower.PowerReserves = CurrentPower.MaxPowerReserves;
        CurrentPower.HeavyPowerReserves = CurrentPower.MaxHeavyPowerReserves;
        _playerParticleSystem.GetComponent<ParticleSystemRenderer>().material = CurrentPower.PowerMaterial;
        OnPowerReservesChange?.Invoke(this);
    }

    private void LightRangedAttack()
    {
        if (!CurrentPower.CheckPowerReserves())
        {
            return;
        }

        Vector3 direction = GetAimDirection();
        CurrentPower.FireLightAttack(_shootPoint.position, direction);
        OnPowerReservesChange?.Invoke(this);
    }

    private void HeavyRangedAttack()
    {
        if (!CurrentPower.CheckHeavyPowerReserves())
        {
            return;
        }

        Vector3 direction = GetAimDirection();
        CurrentPower.FireHeavyAttack(_shootPoint.position, direction);
        OnHeavyPowerReservesChange?.Invoke(this);
    }

    private Vector3 GetAimDirection()
    {
        Ray ray = _mainCamera.ScreenPointToRay(_crosshair.position);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * _range, Color.red, 2f, true);
        Physics.Raycast(ray, out hit, _range);

        if (hit.collider == null)
        {
            hit.point = ray.origin + ray.direction * _range;
        }

        Debug.DrawLine(ray.origin, hit.point, Color.blue, 2f, true);

        return (hit.point - _shootPoint.position).normalized;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        if (obj.layer == 9)
        {
            _canDrainPower = !_canDrainPower;
            _nearbyPowerSource = obj.GetComponent<PowerSource>();
            Debug.Log("InRange of power source");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_canDrainPower)
        {
            _canDrainPower = !_canDrainPower;
            _nearbyPowerSource = null;
            Debug.Log("Left powersource range");
        }
    }

}
