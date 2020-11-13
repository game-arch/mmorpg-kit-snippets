using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ResizeWindow : BaseHandleBehavior
    {

        protected override void OnHandleChange(Vector2 delta, RectTransform rectTransform)
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rectTransform.rect.width + (delta.x / 1.42f));
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rectTransform.rect.height + (delta.y / 1.42f));
            
        }
    }
}