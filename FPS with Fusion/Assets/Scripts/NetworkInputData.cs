using Fusion;

// ���� ��Ʈ������ ���� enum����
enum Buttons
{
    forward = 0,
    
    back = 1,
    
    right = 2,
    
    left = 3,

    jump = 4,


}
// Fusion�� ȿ������ ��Ʈ��ŷ�� ���� ������������ �ִµ� ���߿� �ϳ��� ��Ʈ�����̴�
// ��Ʈ������ ���� ���ÿ� �Էµ� Input�� ���������� ó���ϴ°��̾ƴ϶� ��Ʈ�������� �ѹ��� ó����
// ex). 0000 0001, 0000 0010 �ΰ��� �Է��� -> 0000 0011 �ϳ��� ���ļ� ó����
// �ɼ��ϴٸ� ���� ��Ʈ�����ڸ� ����Ͽ� ó���Ҽ������� enum�� �ᵵ �����ϴ�

public struct NetworkInputData : INetworkInput
{
    public NetworkButtons buttons;

    // yaw : ������
    public Angle yaw;
    // pitch : ������
    public Angle pitch;
}
