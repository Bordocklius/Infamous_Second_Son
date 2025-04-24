using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterControler : MonoBehaviour
{
    public PlayerControls _inputActions;
    public CharacterController _characterController;
    
    public float _speed;

    private Vector2 _movementDirection;

    public Vector3 _minVerticalVelocity;
    private Vector3 _verticalVelocity;
    

    [Space(10)]
    [Header("Jump attributes")]
    // Jump related variables
    public Vector3 _jumpVelocity;
    private bool _isJumping = false;

    // AWAKE
    void Awake()
    {
        _verticalVelocity = _minVerticalVelocity;

        _inputActions = new PlayerControls();

        _inputActions.Gameplay.Move.performed += ctx => _movementDirection = ctx.ReadValue<Vector2>();
        _inputActions.Gameplay.Move.canceled += ctx => _movementDirection = Vector2.zero;

        _inputActions.Gameplay.Jump.performed += ctx => StartJump();
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
        _characterController.Move(_verticalVelocity * Time.deltaTime);
        Debug.Log(_characterController.isGrounded);

        if(_movementDirection != Vector2.zero)
        {
            MoveCharacter();
        }
    }

    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ METHODS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    private void MoveCharacter()
    {
        Vector3 movement = new Vector3(_movementDirection.x, 0, _movementDirection.y);
        movement *= _speed * Time.deltaTime;
        _characterController.Move(movement);
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
}
