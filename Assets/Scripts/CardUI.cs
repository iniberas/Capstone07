using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour{
    [SerializeField] private GameObject imageCard;
    [SerializeField] private GameObject modalText;

    public void SetCardInfo(Sprite sprite, string desc) {
        imageCard.GetComponent<Image>().sprite = sprite;
        modalText.GetComponent<TMPro.TextMeshProUGUI>().text = desc;
    }
}
