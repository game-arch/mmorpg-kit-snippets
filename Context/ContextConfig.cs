using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Context
{
    public class ContextConfig : MonoBehaviour
    {

        public ContextMenuItem contextMenuItemPrefab;
        public Transform contextMenuContainer;

        public static ContextConfig Singleton { get; protected set; }
        // Start is called before the first frame update

        void Start()
        {
            Singleton = this;
        }

        void LateUpdate()
        {
            if (HasClickedOnSomethingElse())
                DestroyMenu();
        }

        bool HasClickedOnSomethingElse()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit, 100f))
                return hit.transform.gameObject.GetComponentsInParent<ContextConfig>().Length == 0;
            return false;
        }

        public void CreateMenu(ContextMenuItemData[] data)
        {
            DestroyMenu();
            foreach (ContextMenuItemData datum in data)
            {
                ContextMenuItem item = Instantiate(contextMenuItemPrefab, contextMenuContainer);
                item.text.text = datum.text;
                item.data = datum;
            }
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
            contextMenuContainer.gameObject.SetActive(false);
            contextMenuContainer.RemoveChildren();
        }
    }
}