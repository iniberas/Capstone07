using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class MaxWidthFitter : MonoBehaviour
{
    public float maxWidth = 30f;
    public TMP_Text tmpText;

    void Update()
    {
        float preferred = tmpText.preferredWidth;
        float target = Mathf.Min(preferred, maxWidth);
        GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, target);
    }
}