using UnityEngine;
using Fusion;
using TMPro;

public class ExampleCharacter : NetworkBehaviour
{
    [SerializeField]
    private NetworkCharacterControllerPrototype _cc;

    [SerializeField]
    private Camera _cam;

    [SerializeField]
    private TMP_Text nicknameText;


    // 계속 동기화 되는 변수
    // 서버에 의해서만 변경될수있다
        // RPC를 만들어 준다
    // 규칙상 캡슐화가 필수이브로 get, set 프로퍼티형태를 띈다
    // 네트워크 변화가 있을때 콜백을 만들어줄수있음
    [Networked(OnChanged = nameof(OnNicknameChanged))] // -> OnChanged = "string" 을 넣어도 되지만 nameof를 사용하면 함수를 그대로 사용할수있어 편함
    public NetworkString<_16> Nickname { get; set; }

    

    // NetworkButtons의 장점은 변경점 만을 전송해 효율적임
    [Networked]
    public NetworkButtons PrevButtons { get; set; }

    private NetworkButtons buttons;
    private NetworkButtons pressed;
    private NetworkButtons released;

    private Vector2 inputDir;
    private Vector3 moveDir;

    // 오브젝트가 스폰되었을때 
    public override void Spawned()
    {
        // 권한 예외처리
        if (!Object.HasStateAuthority)
        {
            // 스폰시 권한이 없는 캐릭터의 카메라 부숨
            // 입력은 잘 받는데 카메라가 다른 캐릭터를 비추고있는경우 방지
            //Destroy(_cam.gameObject);
            return;
        }

        _cam.gameObject.SetActive(true);
        RPC_SedNickname("Name : " + Random.Range(0, 100).ToString());
    }

    public override void Render()
    {
        // 내것이 아닌 오브젝트는 예외하기위한 조건필요
        // input에대한 권한이없다면
        if (!Object.HasInputAuthority)
        {
            return;
        }

        // 카메라 작업은 FixedUpdate가 아닌 Render에서해준다 -> 카메라 끊김 방지
        _cam.transform.rotation = Quaternion.Euler(0, NetworkCallback.NC.Yaw, 0);

        // 가로세로 회전은 localrotation
        _cam.transform.localEulerAngles = new Vector3(NetworkCallback.NC.Pitch, _cam.transform.localRotation.y, _cam.transform.localRotation.z);

    }


    public override void FixedUpdateNetwork()
    {
        // 콜백에서 set한 input을 가져올수있음
        buttons = default;

        // GetInput 을 쓰면 on input으로 줫던 set한 input을 빼내올수있다 out 사용
        // 더 나은 네트워크경험을 위해 포럼에서 추천하는 방식
        if (GetInput<NetworkInputData>(out var input))
        {
            buttons = input.buttons;
        }

        // NetworkButtons는 다양한 기능을 제공한다
        pressed = buttons.GetPressed(PrevButtons);      // 누름감지
        released = buttons.GetReleased(PrevButtons);    // 뗌 감지

        PrevButtons = buttons;

        inputDir = Vector2.zero;

        if (buttons.IsSet(Buttons.forward))
        {
            inputDir += Vector2.up;
        }
        if (buttons.IsSet(Buttons.back))
        {
            inputDir -= Vector2.up;
        }
        if (buttons.IsSet(Buttons.right))
        {
            inputDir += Vector2.right;
        }
        if (buttons.IsSet(Buttons.left))
        {
            inputDir -= Vector2.right;
        }

        // 점프는 연속입력 방지를 위해 pressed사용
        if (pressed.IsSet(Buttons.jump))
        {
            _cc.Jump();
        }

        // 캐릭터기준임(local space)
        // 앞으로가는 방향에 세로축 곱함
        // 오른쪽가는 방향에 가로축 곱함
        moveDir = transform.forward * inputDir.y + transform.right * inputDir.x;

        _cc.Move(moveDir);

        // 캐릭터 좌우로 회전 yaw 사용
        transform.rotation = Quaternion.Euler(0, (float)input.yaw, 0);

    }

    // Networked 콜백 함수는 static이 기본 규칙
    public static void OnNicknameChanged(Changed<ExampleCharacter> changed)
    {
        // 변화된 값들의 behabiour를 실행해서 SetNickname함수를 실행함
        changed.Behaviour.SetNickname();
    }

    public void SetNickname()
    {
        nicknameText.text = Nickname.ToString();
    }

    // InputAuthority권한을 가진 사람(클라이언트)이 StateAuthority권한을 가진 사람(서버)에게 RPC를 보냄
    // 데이터를 보낼수있는 함수같은 역할
    // Rpc파라미터로 넘길수있는게 정해져잇음
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SedNickname(NetworkString<_16> message)
    {
        Nickname = message;
    }


}
