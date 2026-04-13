using UnityEngine;

/// <summary>
/// キャラクターの走行アニメーション。
/// ゲームオーバー時は前のめり（穴に落ちる）アニメーションに切り替える。
/// </summary>
public class ProceduralRunAnimation : MonoBehaviour
{
    [HideInInspector] public Transform leftArmPivot;
    [HideInInspector] public Transform rightArmPivot;
    [HideInInspector] public Transform leftLegPivot;
    [HideInInspector] public Transform rightLegPivot;

    private const float SwingAngle   = 52f;
    private const float RunFrequency = 2.6f;
    private const float BobAmount    = 0.04f;

    /// <summary>StartGame() から呼ぶ：前のめり姿勢を即座にリセット</summary>
    public void ResetPose()
    {
        transform.localRotation = Quaternion.identity;
        transform.localPosition = Vector3.zero;
        transform.localScale    = Vector3.one;
        if (leftArmPivot)  leftArmPivot.localRotation  = Quaternion.identity;
        if (rightArmPivot) rightArmPivot.localRotation = Quaternion.identity;
        if (leftLegPivot)  leftLegPivot.localRotation  = Quaternion.identity;
        if (rightLegPivot) rightLegPivot.localRotation = Quaternion.identity;
    }

    void Update()
    {
        bool gameOver = GameManager.Instance != null &&
                        GameManager.Instance.phase == GamePhase.GameOver;

        if (gameOver)
        {
            // 前のめり（穴に落ちるアニメーション）
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation,
                Quaternion.Euler(70, 0, 0),
                6f * Time.deltaTime);
            return;
        }

        bool playing = GameManager.Instance != null &&
                       GameManager.Instance.phase == GamePhase.Game;
        if (!playing) return;

        transform.localScale    = Vector3.Lerp(transform.localScale,    Vector3.one,        10f * Time.deltaTime);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, 10f * Time.deltaTime);

        float t     = Time.time * RunFrequency;
        float swing = Mathf.Sin(t) * SwingAngle;

        if (leftArmPivot)  leftArmPivot.localRotation  = Quaternion.Euler( swing,        0, 0);
        if (rightArmPivot) rightArmPivot.localRotation = Quaternion.Euler(-swing,        0, 0);
        if (leftLegPivot)  leftLegPivot.localRotation  = Quaternion.Euler(-swing * 0.8f, 0, 0);
        if (rightLegPivot) rightLegPivot.localRotation = Quaternion.Euler( swing * 0.8f, 0, 0);

        transform.localPosition = new Vector3(0,
            Mathf.Abs(Mathf.Sin(t * 2f)) * BobAmount, 0);
    }
}
