using UnityEngine;

/// <summary>
/// 左右2つのゲートを1組として管理するコンポーネント。
/// Prefab の子オブジェクト LeftGate（X=-2）、RightGate（X=+2）を配置すること。
/// </summary>
public class GatePair : MonoBehaviour
{
    [Header("子オブジェクト参照")]
    public Gate leftGate;
    public Gate rightGate;

    [HideInInspector] public int  leftNumber;
    [HideInInspector] public int  rightNumber;
    [HideInInspector] public bool leftIsPrime;
    [HideInInspector] public bool judged    = false;
    [HideInInspector] public bool previewed = false;

    public void Setup(int left, int right, bool lIsPrime)
    {
        leftNumber  = left;
        rightNumber = right;
        leftIsPrime = lIsPrime;

        leftGate.number      = left;
        leftGate.isPrimeGate = lIsPrime;

        rightGate.number      = right;
        rightGate.isPrimeGate = !lIsPrime;

        // Awake/Start 前に呼ばれる場合も初期化できるよう手動で実行
        leftGate.Init();
        rightGate.Init();
    }
}
