using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class DragWindow : BaseHandleBehavior
    {

        protected override void OnHandleChange(Vector2 delta, RectTransform rectTransform)
        {
            target.transform.position = new Vector2(target.transform.position.x + delta.x, target.transform.position.y + delta.y);
        }
    }
}