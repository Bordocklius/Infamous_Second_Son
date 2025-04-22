using UnityEngine;

public class PlayerCharacterControler : MonoBehaviour
{
    public PlayerControls _inputActions;
    public CharacterController _characterController;
    
    public float _speed;

    private Vector2 _movementDirection;

    // Jump related variables
    public float _jumpDuration;
    private float _jumpTimer;
    private bool _isGrounded = true;
    private bool _isJumping = false;
    void Awake()
    {
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

    // Update is called once per frame
    void Update()
    {
        Debug.Log(_movementDirection);
        if(_movementDirection != Vector2.zero)
        {
            MoveCharacter();
        }
    }

    private void FixedUpdate()
    {
        if(_isJumping)
        {

        }
    }

    private void MoveCharacter()
    {
        Vector3 movement = new Vector3(_movementDirection.x, 0, _movementDirection.y);
        movement *= _speed * Time.deltaTime;
        _characterController.Move(movement);
    }

    private void StartJump()
    {
        if(!_isGrounded)
        {
            return;
        }

        _isJumping = !_isJumping;
        _jumpTimer = 0f;
    }

    private void HandleJump()
    {
        _jumpTimer += Time.fixedDeltaTime;

    }
}
