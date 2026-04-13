using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    private Vector3 offset;
    private Vector3 shakeOffset;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void LateUpdate()
    {
        transform.position = target.position + offset + shakeOffset;
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float strength = magnitude * (1f - elapsed / duration);
            var r = Random.insideUnitCircle * strength;
            shakeOffset = new Vector3(r.x, r.y, 0f);
            yield return null;
        }
        shakeOffset = Vector3.zero;
    }
}
