using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using System;

public class Player
{
    public PlayerRef playerRef;
    public NetworkObject playerObject;

    public Player(PlayerRef player, NetworkObject obj)
    {
        playerRef = player;
        playerObject = obj;
    }
}

public class NetworkCallback : MonoBehaviour, INetworkRunnerCallbacks
{
    // static�����Ͽ� ���ٽ�������
    public static NetworkCallback NC;

    // PlayerRef - �÷��̾� �� ��ü�� ���� ��
    public List<Player> runningPlayers = new List<Player>();

    // ���ʿ� DontDestroy����� �־ ���� ���� ���������൵ �ı���������
    private NetworkRunner runner;


    public NetworkPrefabRef PlayerPrefab;

    // ȸ��
    private float yaw;
    public float Yaw
    {
        get
        {
            return yaw;
        }
        set
        {
            yaw = value;

            if (yaw < 0)
            {
                yaw = 360f;
            }

            if (yaw > 360)
            {
                yaw = 0f;
            }
        }
    }
    // ���Ʒ�
    private float pitch;
    public float Pitch
    {
        get
        {
            return pitch;
        }
        set
        {
            pitch = value;

            pitch = Mathf.Clamp(pitch, -80, 80);
        }
    }

    // ���������� ������ ���Ե� �Լ�
    private async void RunGame(GameMode mode)
    {
        var gameArgs = new StartGameArgs();
        gameArgs.GameMode = mode;
        gameArgs.SessionName = "Test";
        gameArgs.PlayerCount = 10;
        

        await runner.StartGame(gameArgs);

        runner.SetActiveScene("GameScene");
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 400, 200), "Host"))
        {
            RunGame(GameMode.Host);
        }

        if (GUI.Button(new Rect(0, 200, 400, 200), "Client"))
        {
            RunGame(GameMode.Client);
        }
    }

    private void Awake()
    {
        // ���ӻ� �����ϴ� NC�� �ϳ��� �α�����
        if (NC == null)
        {
            NC = this;
            runner = gameObject.AddComponent<NetworkRunner>();

            // Oninput���� �Է°��� �����鼭 �ٸ�������� ����� �ϰ��Ϸ��� true
            runner.ProvideInput = true;
        }
        else if (NC != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        yaw += Input.GetAxis("Mouse X");
        // ���콺 ���Ʒ���  -= �ؾ� �츮�� ��������� �ƴ� �����ӱ���
        Pitch -= Input.GetAxis("Mouse Y");
    }


    // ������ ����������
    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    // �������� ����������
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    // ������ ��û�޾�����
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    // �缳�������� �ٸ� �ܺ��÷������� ����ؼ� �����ؾ���
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    // ������ ������ ���������� Ŭ���̾�Ʈ�� �ش��
    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    // ȣ��Ʈ�� �ٲ����� -> ������ ȣ��Ʈ�� ƨ�ܵ� �ٸ������ ȣ��Ʈ ������ �̾
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    // �Է��� ������
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var myInput = new NetworkInputData();

        myInput.buttons.Set(Buttons.forward, Input.GetKey(KeyCode.W));
        myInput.buttons.Set(Buttons.right, Input.GetKey(KeyCode.D));
        myInput.buttons.Set(Buttons.left, Input.GetKey(KeyCode.A));
        myInput.buttons.Set(Buttons.back, Input.GetKey(KeyCode.S));
        myInput.buttons.Set(Buttons.jump, Input.GetKey(KeyCode.Space));

        myInput.pitch = Pitch;
        myInput.yaw = yaw;

        input.Set(myInput);
    }

    // �Է��� ���޾�����
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    // �÷��̾ ���Դ��� �������� Ȯ���ϴ� �۾��� ���� ���� �ʿ䰡 �������� ������
    // �������ϸ� Joined, Left�� �� �ν��� ��
    // �Ʒ� �Լ��� �Ἥ ���� �÷��̾ ������� ī��Ʈ�ϱ� ����
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!this.runner.IsServer)
        {
            return;
        }

        runningPlayers.Add(new Player(player, null));

        foreach (var players in runningPlayers)
        {
            if (players.playerObject != null)
            {
                continue;
            }

            var obj = this.runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, players.playerRef);

            players.playerObject = obj;

            var cc = obj.GetComponent<CharacterController>();
            cc.enabled = false;
            obj.transform.position = new Vector3(0, 10, 0);
            cc.enabled = true;

        }

    } 

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer)
        {
            return;
        }

        foreach (var players in runningPlayers)
        {
            if (players.playerRef.Equals(player))
            {
                this.runner.Despawn(players.playerObject);
                runningPlayers.Remove(players);
            }
            break;
        }
    }

    // �����͸� �޾����� �����͸� ����������(����Ʈ�� �ɰ��� ����)
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    // ���� �ε� ������
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer)
        {
            return;
        }


        foreach (var player in runningPlayers)
        {
            // ������ ���� ���� -> ������ ������ ����
            // ������ �÷��̾����� input���� �� -> inputAuthority 4��° �Ķ���Ͱ� ������ ����Ʈ���ִ� �÷��̾����� �ǳ���
            var obj = runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, player.playerRef);

            player.playerObject = obj;

            // ĳ���� ��Ʈ�ѷ��� �ȵ��� ��츦 ���
            var cc = obj.GetComponent<CharacterController>();
            cc.enabled = false;
            // ������ġ ����
            obj.transform.position = new Vector3(0, 10, 0);
            cc.enabled = true;
        }
    }

    // ���ε尡 ���� ������
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        if (!this.runner.IsServer)
        {
            return;
        }
    }

    // ������ ������Ʈ ������ �� ����� �̿��� ���뼼�� ����Ʈ�� �ް� ���������� ���� ����
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    // �˴ٿ� ������
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    // ???
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
