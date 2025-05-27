using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;   // 새 Input System 네임스페이스

public class PlayerMovement : NetworkBehaviour
{
    private Vector3 _velocity;
    private bool _jumpPressed;

    // 이벤트 기반 입력 저장용
    private Vector2 _moveInput;

    private InputAction _moveAction;
    private InputAction _jumpAction;

    private CharacterController _controller;

    public float PlayerSpeed = 2f;

    public float JumpForce = 5f;
    public float GravityValue = -9.81f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        // 이동 액션 세팅 (2DVector Composite + 게임패드 스틱 + 모바일 터치 드래그)
        _moveAction = new InputAction(name: "Move", type: InputActionType.Value, expectedControlType: "Vector2");

        // 키보드 WASD
        var composite = _moveAction.AddCompositeBinding("2DVector");
        composite.With("Up", "<Keyboard>/w");
        composite.With("Down", "<Keyboard>/s");
        composite.With("Left", "<Keyboard>/a");
        composite.With("Right", "<Keyboard>/d");

        // 화살표키
        var compositeArrows = _moveAction.AddCompositeBinding("2DVector");
        compositeArrows.With("Up", "<Keyboard>/upArrow");
        compositeArrows.With("Down", "<Keyboard>/downArrow");
        compositeArrows.With("Left", "<Keyboard>/leftArrow");
        compositeArrows.With("Right", "<Keyboard>/rightArrow");

        // 게임패드 스틱 & D-Pad
        _moveAction.AddBinding("<Gamepad>/leftStick");
        _moveAction.AddBinding("<Gamepad>/dpad");

        // 모바일 터치 드래그(스크린의 delta를 간단 조이스틱으로 사용)
        //_moveAction.AddBinding("<Touchscreen>/primaryTouch/delta");

        // Jump 액션 세팅 (Button)
        _jumpAction = new InputAction(name: "Jump", type: InputActionType.Button);
        _jumpAction.AddBinding("<Keyboard>/space");
        _jumpAction.AddBinding("<Gamepad>/buttonSouth");
        //_jumpAction.AddBinding("<Touchscreen>/primaryTouch/press");

        // 이벤트 연결
        _moveAction.performed += ctx =>
        {
            // 가상 스틱이 활성일 때 Touchscreen 이벤트는 건너뛴다
            if (ctx.control.device is Touchscreen) return;
            _moveInput = ctx.ReadValue<Vector2>();
        };
        _moveAction.canceled += ctx => _moveInput = Vector2.zero;

        _jumpAction.performed += ctx => _jumpPressed = true;

        _moveAction.Enable();
        _jumpAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction?.Disable();
        _jumpAction?.Disable();
    }
    
    public override void Spawned()
    {
        // 로컬 플레이어만 스폰시 권한 확인 후 Destroy
        if (HasStateAuthority == false)
        {
            Destroy(this);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (_controller.isGrounded)
        {
            _velocity = Vector3.down;
        }
        ///Quaternion cameraRotationY = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0);
        //Vector3 move = cameraRotationY * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * Runner.DeltaTime * PlayerSpeed;
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y) * Runner.DeltaTime * PlayerSpeed;

        _velocity.y += GravityValue * Runner.DeltaTime;
        if (_jumpPressed && _controller.isGrounded)
        {
            _velocity.y += JumpForce;
        }
        _controller.Move(move + _velocity * Runner.DeltaTime);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }

        _jumpPressed = false;
    }
}
