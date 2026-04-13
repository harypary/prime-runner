using UnityEngine;

/// <summary>
/// 素数ランナー用 PlayerController。
/// 2レーン固定：レーン0=左(X=-1.5), レーン1=右(X=+1.5)
/// 道路は常にX=0中心なのでroadCenterRight=0固定。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float forwardSpeed    = 16f;
    public float laneDistance    = 3f;     // レーン間隔: lane0=-1.5, lane1=+1.5
    public float laneChangeSpeed = 56f;
    public float gravity         = -28f;

    // 現在のレーン（0=左, 1=右）2レーンのみ
    [HideInInspector] public int currentLane = 0;

    private CharacterController controller;
    private float verticalVel    = 0f;

    // 道路は常にX=0中心（直線コースのため固定）
    // lane0 target = 0 - laneDistance*0.5 = -1.5
    // lane1 target = 0 + laneDistance*0.5 = +1.5
    private const float roadCenterRight = 0f;

    // ゲーム10と同じ地面判定（Raycast併用）
    bool IsGrounded()
    {
        if (controller.isGrounded) return true;
        return Physics.Raycast(
            transform.position + Vector3.up * 0.1f,
            Vector3.down, 0.2f);
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        float dt = Time.deltaTime;

        // ── 重力（常時適用：タイトル中も地面に立つ）─────────────────
        if (IsGrounded())
            verticalVel = -0.5f;
        else
            verticalVel += gravity * dt;

        if (GameManager.Instance.phase != GamePhase.Game)
        {
            if (GameManager.Instance.phase == GamePhase.Title)
            {
                // タイトル中はデモ走行（街並みが流れてゲームっぽく見える）
                controller.Move(
                    transform.forward * 14f * dt
                  + Vector3.up        * verticalVel * dt);
            }
            else
            {
                // ゲームオーバー：重力のみ
                controller.Move(Vector3.up * verticalVel * dt);
            }
            return;
        }

        // ── Game phase: 入力受付 ──────────────────────────────────
        var swipe = InputManager.Instance != null
                    ? InputManager.Instance.GetSwipe()
                    : SwipeDirection.None;

        if (swipe == SwipeDirection.Left  && currentLane > 0) currentLane--;
        if (swipe == SwipeDirection.Right && currentLane < 1) currentLane++;

        // ── 横移動 ────────────────────────────────────────────────
        // lane0=-1.5m, lane1=+1.5m（道路中心X=0から±1.5m）
        float cur  = Vector3.Dot(transform.position, transform.right);
        float want = roadCenterRight + (currentLane == 0 ? -laneDistance * 0.5f : laneDistance * 0.5f);
        float diff = want - cur;
        float laneStep = Mathf.Clamp(diff, -laneChangeSpeed * dt, laneChangeSpeed * dt);

        // ── 移動適用 ─────────────────────────────────────────────
        controller.Move(
            transform.forward * forwardSpeed * dt
          + transform.right   * laneStep
          + Vector3.up        * verticalVel  * dt
        );
    }

    /// <summary>StartGame()から呼ぶ：レーンと速度をリセット</summary>
    public void ResetOnSpawn()
    {
        // CharacterController が有効なまま transform.position を書き換えると
        // CCが位置を拒否することがある。一旦無効化して確実に移動させる。
        controller.enabled = false;
        transform.position = new Vector3(-1.5f, 0f, 5f);
        transform.rotation = Quaternion.identity;
        controller.enabled = true;

        verticalVel = -0.5f;
        currentLane = 0;
    }
}
