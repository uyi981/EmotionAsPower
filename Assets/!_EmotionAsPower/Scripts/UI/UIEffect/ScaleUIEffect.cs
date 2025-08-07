
using System.Collections;
using UnityEngine;

public class ScaleUIEffect : MonoBehaviour, IUIEffect
{
    [SerializeField] private Vector3 targetScale = Vector3.one * 1.1f;
    [SerializeField] private float duration = 0.2f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine effectCoroutine;

    private Vector3 originalScale;

    void Awake()
    {
        originalScale = transform.localScale;
    }
    public void PlayEffect()
    {
        //Stop any existing coroutine to prevent overlapping animations
        if (effectCoroutine != null)
        {
            StopAllCoroutines();
        }

        effectCoroutine = StartCoroutine(ScaleUpAnimation());
    }

    public void StopEffect()
    {
        //Stop any existing coroutine to prevent overlapping animations
        if (effectCoroutine != null)
        {
            StopAllCoroutines();
        }

        effectCoroutine = StartCoroutine(ScaleDownAnimation());


    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        effectCoroutine = null;
    }

    private IEnumerator ScaleUpAnimation()
    {
        float timer = 0;
        Vector3 startScale = transform.localScale;

        //Scale up
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = curve.Evaluate(timer / duration);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        //Ensure reaching the target scale
        transform.localScale = targetScale;
        effectCoroutine = null;
    }

    private IEnumerator ScaleDownAnimation()
    {
        float timer = 0;
        Vector3 startScale = transform.localScale;

        //Scale up
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = curve.Evaluate(timer / duration);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            yield return null;
        }

        //Ensure reaching the original scale
        transform.localScale = originalScale;
        effectCoroutine = null;
    }
}