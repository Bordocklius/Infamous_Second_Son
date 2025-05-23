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
    private bool _canJump = false;
    private bool _isJumping = false;
    private bool _isHovering = false;
    private bool _isClimbing = false;

    [Space(10)]
    [Header("Camera")]
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private Camera _mainCamera;

    [Space(10)]
    [Header("Power related")]
    public PowerBase CurrentPower;
    [SerializeField]
    private RectTransform _crosshair;
    [SerializeField]
    private Transform _shootPoint;
    [SerializeField]
    private float _range;

    public static event Action<PlayerCharacterControler> OnPowerReservesChange;
    public static event Action<PlayerCharacterControler> OnHeavyPowerReservesChange;
    public static event Action<PlayerCharacterControler> OnPowerChange;

    private bool _canDrainPower = false;
    private bool _isDashing = false;

    [SerializeField]
    private GameObject _playerParticleSystem;
    [SerializeField]
    private AudioSource _audioSource;

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
    [SerializeField]
    private AudioClip _dashSound;

    [Space(10)]
    [Header("Neon Sprint")]
    [SerializeField]
    private float _neonSprintSpeed;
    [SerializeField]
    private float _neonSprintDuration;
    [SerializeField]
    private AudioClip _neonSprintSound;

    private float _dashTimer;

    private PowerSource _nearbyPowerSource;


    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ AWAKE ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    void Awake()
    {
        _mainCamera = Camera.main;

        _verticalVelocity = _minVerticalVelocity;

        // Hook into input action events
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

    // Lock cursor and set particle system material
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

        // Reset jump
        if (_characterController.isGrounded)
        {
            _canJump = true;
            _isJumping = false;
        }

        // Reenable collision after smoke dash
        if (!_isDashing && CurrentPower is SmokePower && _dashTimer >= _dashDuration)
        {
            Physics.IgnoreLayerCollision(6, 8, false);
        }

        // Disable particle system after movement ability
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

        Vector3 movement = (cameraRight * _movementDirection.x + cameraForward * _movementDirection.y).normalized;
        float movementspeed = _speed;

        // Handle movement when smokedashing
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
        // Handle movement when neon sprinting
        else if (_isDashing && CurrentPower is NeonPower)
        {
            _dashTimer += Time.deltaTime;
            movementspeed = _neonSprintSpeed;              
        }
        // Allow sprinting up walls when neon sprinting
        if(_isDashing && CurrentPower is NeonPower && CheckWallInFront())
        {
            _verticalVelocity.y = _characterModelTransform.forward.y + Vector3.up.y;
        }

        movement.y = _verticalVelocity.y;

        _isClimbing = false;
        if (CheckWallInFront() && !_isDashing)
        {
            _canJump = true;
            _isClimbing = true;
            _isJumping = false;
        }

        // Deal with gravity
        if (_isHovering)
        {
            _verticalVelocity.y = Mathf.Clamp(_verticalVelocity.y + _slowedVerticalVelocity.y * Time.deltaTime, _minVerticalVelocity.y, _jumpVelocity.y);
        }        
        else if(!_isClimbing || _isJumping)
        {
            _verticalVelocity.y = Mathf.Clamp(_verticalVelocity.y + _minVerticalVelocity.y * Time.deltaTime, _minVerticalVelocity.y, _jumpVelocity.y);
        }
        else if (_isClimbing)
        {
            _verticalVelocity.y = 0;
        }

        movement *= movementspeed * Time.deltaTime;
        _characterController.Move(movement);

        Vector3 lookvector = new Vector3(movement.x, 0, movement.z);
        RotateCharacter(lookvector);

        if (_isDashing && _dashTimer >= _dashDuration)
        {
            _isDashing = false;
        }
    }

    private void RotateCharacter(Vector3 lookvector)
    {
        if (lookvector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookvector);
            _characterModelTransform.rotation = Quaternion.Slerp(_characterModelTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        }
    }

    // Check if wall is in front of player to allow them to grip onto it
    private bool CheckWallInFront()
    {
        RaycastHit hit;    
        Physics.Raycast(_characterController.transform.position, _characterModelTransform.forward, out hit, 1f, ~_playerMask);
        if (hit.collider != null && hit.collider.gameObject.tag.ToLower() == "climable")
        {
            return true;
        }
        return false;
    }

    // Start player jump
    private void StartJump()
    {
        if (!_canJump) return;

        _canJump = false;
        _isJumping = !_isJumping;
        _isClimbing = false;
        _verticalVelocity = _jumpVelocity;
    }

    // Activate hover ability
    private void Hover()
    {
        if (_characterController.isGrounded && _isHovering) return;

        _slowedVerticalVelocity = _minVerticalVelocity + new Vector3(0, CurrentPower.GlideSlow, 0);
        _isHovering = true;
        _playerParticleSystem.SetActive(true);
    }

    // Activate movement ability
    private void MovementAbility()
    {
        if (_isDashing) return;

        _isDashing = true;
        _dashTimer = 0;

        // Use smokedash if using smokepower
        if (CurrentPower is SmokePower)
        {
            Physics.IgnoreLayerCollision(6, 8, true);
            _audioSource.PlayOneShot(_dashSound);
        }

        // Use neon sprint if using neonpower
        if(CurrentPower is NeonPower)
        {
            _audioSource.PlayOneShot(_neonSprintSound);
        }

        _playerParticleSystem.SetActive(true);
    }

    // Drain power from nearby powersource
    private void PowerDrain()
    {
        if (!_canDrainPower || !_nearbyPowerSource.Drainable) return;

        // Check power of source
        _nearbyPowerSource.DrainSource();
        if (_nearbyPowerSource.PowerName.ToLower() == "smoke")
        {
            CurrentPower = this.GetComponent<SmokePower>();
        }
        if (_nearbyPowerSource.PowerName.ToLower() == "neon")
        {
            CurrentPower = this.GetComponent<NeonPower>();
            
        }

        // Set power to drained power and reset power reserves
        CurrentPower.PowerReserves = CurrentPower.MaxPowerReserves;
        CurrentPower.HeavyPowerReserves = CurrentPower.MaxHeavyPowerReserves;
        _playerParticleSystem.GetComponent<ParticleSystemRenderer>().material = CurrentPower.PowerMaterial;

        // Reset power reserves in UI
        OnPowerReservesChange?.Invoke(this);
        OnHeavyPowerReservesChange?.Invoke(this);
        OnPowerChange?.Invoke(this);
    }

    // Fire light attack
    private void LightRangedAttack()
    {
        if (!CurrentPower.CheckPowerReserves()) return;

        Vector3 direction = GetAimDirection();
        CurrentPower.FireLightAttack(_shootPoint.position, direction);
        _audioSource.PlayOneShot(CurrentPower.ShootEffect);
        OnPowerReservesChange?.Invoke(this);
    }

    // Fire heavy attack
    private void HeavyRangedAttack()
    {
        if (!CurrentPower.CheckHeavyPowerReserves()) return;

        Vector3 direction = GetAimDirection();
        CurrentPower.FireHeavyAttack(_shootPoint.position, direction);
        _audioSource.PlayOneShot(CurrentPower.ShootEffect);
        OnHeavyPowerReservesChange?.Invoke(this);
    }

    // Get aim direction for ranged attacks according to crosshair
    private Vector3 GetAimDirection()
    {
        // Shoot ray through crosshair
        Ray ray = _mainCamera.ScreenPointToRay(_crosshair.position);
        RaycastHit hit;
        Debug.DrawRay(ray.origin, ray.direction * _range, Color.red, 2f, true);
        Physics.Raycast(ray, out hit, _range);

        if (hit.collider == null)
        {
            hit.point = ray.origin + ray.direction * _range;
        }

        Debug.DrawLine(ray.origin, hit.point, Color.blue, 2f, true);

        // Get direction from shootpoint to hitpoint
        Vector3 direction = (hit.point - _shootPoint.position).normalized;
        _characterModelTransform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        return direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject obj = other.gameObject;
        
        // Check if obj is powersource
        if (obj.layer == 9)
        {
            _canDrainPower = !_canDrainPower;
            _nearbyPowerSource = obj.GetComponent<PowerSource>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When player leaves powersource range, disable draining
        if (_canDrainPower)
        {
            _canDrainPower = !_canDrainPower;
            _nearbyPowerSource = null;
        }
    }

}
