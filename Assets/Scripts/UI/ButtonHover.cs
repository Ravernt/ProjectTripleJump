using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject background;

    public void OnPointerEnter(PointerEventData eventData)
    {
        background.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        background.SetActive(false);
    }

    void OnEnable()
    {
        background.SetActive(false);
    }
}
