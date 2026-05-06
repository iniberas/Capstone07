using TMPro;
using UnityEngine;

public class ButtonHover : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public void EnableUnderline()
    {
        text.fontStyle |= FontStyles.Underline;
    }

    public void DisableUnderline()
    {
        text.fontStyle &= ~FontStyles.Underline;
    }
}