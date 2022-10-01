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
    // static선언하여 접근쉽게해줌
    public static NetworkCallback NC;

    // PlayerRef - 플레이어 그 자체에 대한 값
    public List<Player> runningPlayers = new List<Player>();

    // 러너에 DontDestroy기능이 있어서 굳이 따로 설정안해줘도 파괴되지않음
    private NetworkRunner runner;


    public NetworkPrefabRef PlayerPrefab;

    // 회전
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
    // 위아래
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

    // 직접적으로 서버를 열게될 함수
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
        // 게임상 존재하는 NC를 하나만 두기위해
        if (NC == null)
        {
            NC = this;
            runner = gameObject.AddComponent<NetworkRunner>();

            // Oninput에서 입력값을 받으면서 다른서버들과 통신을 하게하려면 true
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
        // 마우스 위아래는  -= 해야 우리가 통상적으로 아는 움직임구현
        Pitch -= Input.GetAxis("Mouse Y");
    }


    // 서버에 연결했을때
    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    // 서버연결 실패했을때
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    // 연결을 요청받았을때
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    // 사설인증같은 다른 외부플러그인을 사용해서 인증해야함
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    // 서버와 연결이 끊어졌을때 클라이언트가 해당됨
    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
    }

    // 호스트가 바꼈을때 -> 실행중 호스트가 튕겨도 다른사람이 호스트 역할을 이어감
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    // 입력을 받을때
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

    // 입력을 못받았을때
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    // 플레이어가 들어왔는지 나갔는지 확인하는 작업을 따로 해줄 필요가 있을수도 있지만
    // 어지간하면 Joined, Left로 다 인식이 됨
    // 아래 함수를 써서 현재 플레이어가 몇명인지 카운트하기 쉽다
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

    // 데이터를 받았을때 데이터를 보낼수있음(바이트로 쪼개서 보냄)
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    // 씬이 로드 됬을때
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer)
        {
            return;
        }


        foreach (var player in runningPlayers)
        {
            // 프리팹 스폰 가능 -> 스폰은 서버만 가능
            // 스폰된 플레이어한테 input값을 줌 -> inputAuthority 4번째 파라미터가 권한을 리스트에있는 플레이어한테 건네줌
            var obj = runner.Spawn(PlayerPrefab, Vector3.zero, Quaternion.identity, player.playerRef);

            player.playerObject = obj;

            // 캐릭터 컨트롤러가 안들어올 경우를 대비
            var cc = obj.GetComponent<CharacterController>();
            cc.enabled = false;
            // 스폰위치 설정
            obj.transform.position = new Vector3(0, 10, 0);
            cc.enabled = true;
        }
    }

    // 씬로드가 시작 됬을때
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        if (!this.runner.IsServer)
        {
            return;
        }
    }

    // 세션이 업데이트 됬을때 이 기능을 이용해 공용세션 리스트를 받고 공개적으로 참가 가능
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    // 셧다운 됬을때
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    // ???
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }
}
