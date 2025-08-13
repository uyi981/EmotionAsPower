using UnityEngine;
using System.Collections;

public class UIPanelSlider : MonoBehaviour
{
    [Header("Assign the panel RectTransform here")]
    public RectTransform panel;

    [Header("Slide settings")]
    public float slideDuration = 0.3f;

    [Header("Hidden position")]
    public float hiddenX = 0f;
    public float hiddenY = -400f;

    [Header("Shown position")]
    public float shownX = 0f;
    public float shownY = 0f;

    [Header("Animation curve for easing")]
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private bool isShown = false;
    private Coroutine currentAnimation;
    private bool isInitialized = false;

    void Awake()
    {
        InitializePanel();
    }

    void Start()
    {
        if (!isInitialized)
        {
            InitializePanel();
        }
    }

    void OnEnable()
    {
        InitializePanel();
    }

    void OnDisable()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    private void InitializePanel()
    {
        if (panel != null)
        {
            Vector2 pos = new Vector2(hiddenX, hiddenY);
            panel.anchoredPosition = pos;
            isShown = false;
            isInitialized = true;
        }
    }

    public void ShowPanel()
    {
        if (!isShown)
        {
            TogglePanel();
        }
    }

    public void HidePanel()
    {
        if (isShown)
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        if (panel == null)
        {
            Debug.LogWarning("Panel is not assigned to UIPanelSlider!");
            return;
        }

        isShown = !isShown;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        Vector2 targetPos = isShown ? new Vector2(shownX, shownY) : new Vector2(hiddenX, hiddenY);
        currentAnimation = StartCoroutine(SlidePanel(targetPos));
    }

    private IEnumerator SlidePanel(Vector2 targetPos)
    {
        if (panel == null)
        {
            Debug.LogWarning("Panel became null during animation!");
            yield break;
        }

        Vector2 startPos = panel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            if (panel == null)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / slideDuration);
            float easedTime = easeCurve != null ? easeCurve.Evaluate(normalizedTime) :
                             normalizedTime * normalizedTime * (3f - 2f * normalizedTime);

            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, easedTime);
            yield return null;
        }

        if (panel != null)
        {
            panel.anchoredPosition = targetPos;
        }

        currentAnimation = null;
    }

    // Properties
    public bool IsShown => isShown;
    public bool IsAnimating => currentAnimation != null;

    public void SetStateImmediate(bool show)
    {
        if (panel == null) return;

        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        isShown = show;
        Vector2 targetPos = isShown ? new Vector2(shownX, shownY) : new Vector2(hiddenX, hiddenY);
        panel.anchoredPosition = targetPos;
    }

    public void SetHiddenPosition(float x, float y)
    {
        hiddenX = x;
        hiddenY = y;
    }

    public void SetShownPosition(float x, float y)
    {
        shownX = x;
        shownY = y;
    }

    public void SetHiddenPosition(Vector2 position)
    {
        hiddenX = position.x;
        hiddenY = position.y;
    }

    public void SetShownPosition(Vector2 position)
    {
        shownX = position.x;
        shownY = position.y;
    }
}