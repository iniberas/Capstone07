using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingAnimator : MonoBehaviour
{
    [Header("Hands")]
    [SerializeField] private RectTransform handLeft;
    [SerializeField] private RectTransform handRight;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Hand Animation")]
    private float bobFrequency = 2f;
    private float bobHorizontalAmplitude = 2f;

    [Header("Dots Animation")]
    [SerializeField] private float dotInterval = 0.4f;

    private Vector2 handLeftOrigin;
    private Vector2 handRightOrigin;

    private float dotTimer;
    private int dotCount;

    void OnEnable()
    {
        if (handLeft != null)  handLeftOrigin  = handLeft.anchoredPosition;
        if (handRight != null) handRightOrigin = handRight.anchoredPosition;

        dotTimer = 0f;
        dotCount = 0;
        UpdateDotText();
    }

    void OnDisable()
    {
        if (handLeft != null)  handLeft.anchoredPosition  = handLeftOrigin;
        if (handRight != null) handRight.anchoredPosition = handRightOrigin;
    }

    void Update()
    {
        AnimateHands();
        AnimateDots();
    }

    void AnimateHands()
    {
        float t = Time.time;
            float pulse = (Mathf.Abs(Mathf.Sin(t * bobFrequency * Mathf.PI * 2f * 0.7f)) - 1) * bobHorizontalAmplitude;
            handLeft.anchoredPosition = handLeftOrigin + new Vector2(pulse, 0f);
            handRight.anchoredPosition = handRightOrigin + new Vector2(pulse, 0f);
    }

    void AnimateDots()
    {
        dotTimer += Time.deltaTime;
        if (dotTimer >= dotInterval)
        {
            dotTimer -= dotInterval;
            dotCount = (dotCount + 1) % 4;
            UpdateDotText();
        }
    }

    void UpdateDotText()
    {
        if (loadingText == null) return;
        loadingText.text = "Loading" + new string('.', dotCount);
    }
}