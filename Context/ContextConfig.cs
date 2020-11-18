using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Context
{
    public class ContextConfig : MonoBehaviour
    {

        public Transform contextMenuContainer;

        public static ContextConfig Singleton { get; protected set; }
        // Start is called before the first frame update

        void Start()
        {
            Singleton = this;
        }

        void LateUpdate()
        {
            if (Input.GetMouseButtonUp(0))
                DestroyMenu();

        }

        bool HasClickedOnSomethingElse()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 100f))
                return hit.transform.gameObject.GetComponentInParent<ContextConfig>() != null;
            return false;
        }

        public void CreateMenu(GameObject item)
        {
            contextMenuContainer.RemoveChildren();
            item.transform.SetParent(contextMenuContainer);
            RectTransform rect = contextMenuContainer.GetComponent<RectTransform>();
            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;
            float halfWidth = rect.sizeDelta.x / 2;
            float halfHeight = rect.sizeDelta.y / 2;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float xPos = x + halfWidth;
            float yPos = y - halfHeight;
            if (xPos + halfWidth > screenWidth)
                xPos = x - halfWidth;
            if (y - halfHeight < 0)
                yPos = y - (y - halfHeight);
            contextMenuContainer.transform.position = new Vector3(xPos, yPos, 0);
            contextMenuContainer.gameObject.SetActive(true);
        }


        public void DestroyMenu()
        {
            contextMenuContainer.RemoveChildren();
            contextMenuContainer.gameObject.SetActive(false);
        }
    }
}