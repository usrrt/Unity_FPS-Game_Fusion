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


    // ��� ����ȭ �Ǵ� ����
    // ������ ���ؼ��� ����ɼ��ִ�
        // RPC�� ����� �ش�
    // ��Ģ�� ĸ��ȭ�� �ʼ��̺�� get, set ������Ƽ���¸� ���
    // ��Ʈ��ũ ��ȭ�� ������ �ݹ��� ������ټ�����
    [Networked(OnChanged = nameof(OnNicknameChanged))] // -> OnChanged = "string" �� �־ ������ nameof�� ����ϸ� �Լ��� �״�� ����Ҽ��־� ����
    public NetworkString<_16> Nickname { get; set; }

    

    // NetworkButtons�� ������ ������ ���� ������ ȿ������
    [Networked]
    public NetworkButtons PrevButtons { get; set; }

    private NetworkButtons buttons;
    private NetworkButtons pressed;
    private NetworkButtons released;

    private Vector2 inputDir;
    private Vector3 moveDir;

    // ������Ʈ�� �����Ǿ����� 
    public override void Spawned()
    {
        // ���� ����ó��
        if (!Object.HasStateAuthority)
        {
            // ������ ������ ���� ĳ������ ī�޶� �μ�
            // �Է��� �� �޴µ� ī�޶� �ٸ� ĳ���͸� ���߰��ִ°�� ����
            //Destroy(_cam.gameObject);
            return;
        }

        _cam.gameObject.SetActive(true);
        RPC_SedNickname("Name : " + Random.Range(0, 100).ToString());
    }

    public override void Render()
    {
        // ������ �ƴ� ������Ʈ�� �����ϱ����� �����ʿ�
        // input������ �����̾��ٸ�
        if (!Object.HasInputAuthority)
        {
            return;
        }

        // ī�޶� �۾��� FixedUpdate�� �ƴ� Render�������ش� -> ī�޶� ���� ����
        _cam.transform.rotation = Quaternion.Euler(0, NetworkCallback.NC.Yaw, 0);

        // ���μ��� ȸ���� localrotation
        _cam.transform.localEulerAngles = new Vector3(NetworkCallback.NC.Pitch, _cam.transform.localRotation.y, _cam.transform.localRotation.z);

    }


    public override void FixedUpdateNetwork()
    {
        // �ݹ鿡�� set�� input�� �����ü�����
        buttons = default;

        // GetInput �� ���� on input���� �Z�� set�� input�� �����ü��ִ� out ���
        // �� ���� ��Ʈ��ũ������ ���� �������� ��õ�ϴ� ���
        if (GetInput<NetworkInputData>(out var input))
        {
            buttons = input.buttons;
        }

        // NetworkButtons�� �پ��� ����� �����Ѵ�
        pressed = buttons.GetPressed(PrevButtons);      // ��������
        released = buttons.GetReleased(PrevButtons);    // �� ����

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

        // ������ �����Է� ������ ���� pressed���
        if (pressed.IsSet(Buttons.jump))
        {
            _cc.Jump();
        }

        // ĳ���ͱ�����(local space)
        // �����ΰ��� ���⿡ ������ ����
        // �����ʰ��� ���⿡ ������ ����
        moveDir = transform.forward * inputDir.y + transform.right * inputDir.x;

        _cc.Move(moveDir);

        // ĳ���� �¿�� ȸ�� yaw ���
        transform.rotation = Quaternion.Euler(0, (float)input.yaw, 0);

    }

    // Networked �ݹ� �Լ��� static�� �⺻ ��Ģ
    public static void OnNicknameChanged(Changed<ExampleCharacter> changed)
    {
        // ��ȭ�� ������ behabiour�� �����ؼ� SetNickname�Լ��� ������
        changed.Behaviour.SetNickname();
    }

    public void SetNickname()
    {
        nicknameText.text = Nickname.ToString();
    }

    // InputAuthority������ ���� ���(Ŭ���̾�Ʈ)�� StateAuthority������ ���� ���(����)���� RPC�� ����
    // �����͸� �������ִ� �Լ����� ����
    // Rpc�Ķ���ͷ� �ѱ���ִ°� ����������
    [Rpc(sources: RpcSources.InputAuthority, targets: RpcTargets.StateAuthority)]
    public void RPC_SedNickname(NetworkString<_16> message)
    {
        Nickname = message;
    }


}
