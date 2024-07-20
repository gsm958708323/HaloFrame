using HaloFrame;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RedDotClick : MonoBehaviour, IPointerClickHandler
{
    RedDotItem redDotItem;

    private void Awake()
    {
        redDotItem = GetComponentInChildren<RedDotItem>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameManager.RedDot.AddValue(redDotItem.Key, 1);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            GameManager.RedDot.AddValue(redDotItem.Key, -1);
        }
    }
}
