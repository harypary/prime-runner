using UnityEngine;

public enum SwipeDirection { None, Up, Down, Left, Right }

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    // タッチ
    private Vector2 fingerDownPos;
    private Vector2 fingerUpPos;
    public float minSwipeDistance = 50f;

    // マウスドラッグ（PC デスクトップ用）
    private Vector2 mouseDownPos;
    private bool    mouseDragging;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// フレームごとに呼ぶ。方向が確定したらその方向を返し、次フレームは None に戻る。
    /// PC: WASD / 矢印 / マウスドラッグ
    /// スマホ: タッチスワイプ
    /// </summary>
    public SwipeDirection GetSwipe()
    {
        // ── キーボード ────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.LeftArrow)  || Input.GetKeyDown(KeyCode.A)) return SwipeDirection.Left;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return SwipeDirection.Right;

        // ── マウスドラッグ（PC でスワイプ相当） ──────────────────
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPos  = Input.mousePosition;
            mouseDragging = true;
        }
        if (mouseDragging && Input.GetMouseButtonUp(0))
        {
            mouseDragging = false;
            Vector2 delta = (Vector2)Input.mousePosition - mouseDownPos;
            if (delta.magnitude >= minSwipeDistance)
                return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }

        // ── タッチ（モバイル） ─────────────────────────────────────
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                fingerDownPos = touch.position;
                fingerUpPos   = touch.position;
            }
            if (touch.phase == TouchPhase.Ended)
            {
                fingerUpPos = touch.position;
                return DetectTouchSwipe();
            }
        }

        return SwipeDirection.None;
    }

    private SwipeDirection DetectTouchSwipe()
    {
        float hDist = fingerDownPos.x - fingerUpPos.x;
        float hAbs  = Mathf.Abs(hDist);

        if (hAbs < minSwipeDistance) return SwipeDirection.None;
        return hDist > 0 ? SwipeDirection.Left : SwipeDirection.Right;
    }
}
