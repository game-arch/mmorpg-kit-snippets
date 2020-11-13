using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{

    [RequireComponent(typeof(HandleTarget))]
    public abstract class BaseHandleBehavior : MonoBehaviour
    {

        protected GameObject target;
        protected RectTransform rectTransform;

        protected Vector2 lastPosition;
        protected bool hasLastPosition;

        protected bool dragging;
        public void Start()
        {
            HandleTarget dt = GetComponent<HandleTarget>();
            if (dt)
            {
                target = dt.target;
                rectTransform = target.GetComponent<RectTransform>();
            }
        }
        public void OnMouseDown() {
            dragging = true;
            hasLastPosition = false;
        }
        public void OnMouseUp() {
            dragging = false;
            hasLastPosition = false;
        }
        void FixedUpdate() {
            if (dragging) {
                OnDrag();
                lastPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                hasLastPosition = true;
            }
        }

        protected void OnDrag()
        {
            if (hasLastPosition) {
                Vector2 delta = new Vector2(
                    Input.mousePosition.x - lastPosition.x,
                    Input.mousePosition.y - lastPosition.y
                );
                OnHandleChange(delta, rectTransform);
            }
        }

        protected abstract void OnHandleChange(Vector2 delta, RectTransform rectTransform);
    }
}