using Fusion;

// 쉬운 비트연산을 위해 enum생성
enum Buttons
{
    forward = 0,
    
    back = 1,
    
    right = 2,
    
    left = 3,

    jump = 4,


}
// Fusion은 효율적인 네트워킹을 위해 규정된형식이 있는데 그중에 하나가 비트연산이다
// 비트연산을 통해 동시에 입력된 Input을 순차적으로 처리하는것이아니라 비트연산으로 한번에 처리함
// ex). 0000 0001, 0000 0010 두개의 입력을 -> 0000 0011 하나로 합쳐서 처리함
// 능숙하다면 직접 비트연산자를 사용하여 처리할수있지만 enum을 써도 무방하다

public struct NetworkInputData : INetworkInput
{
    public NetworkButtons buttons;

    // yaw : 가로축
    public Angle yaw;
    // pitch : 세로축
    public Angle pitch;
}
