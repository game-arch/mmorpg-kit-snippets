using System.Collections;
using System.Collections.Generic;
using MultiplayerARPG;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tooltips
{
    public class TooltipArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        static Canvas canvas;
        protected UICharacterItem item;
        protected UICharacterSkill skill;
        protected UICharacterItem uiCharacterItem
        {
            get
            {
                if (item == null)
                    item = GetComponent<UICharacterItem>();
                return item;
            }
        }
        protected UICharacterSkill uiCharacterSkill
        {
            get
            {
                if (skill == null)
                    skill = GetComponent<UICharacterSkill>();
                return skill;
            }
        }

        protected UIBase instance;
        protected Renderer uiRenderer;
        // Start is called before the first frame update
        void OnEnable()
        {
            if (instance == null)
            {
                if (TooltipConfig.Singleton?.uiItemDialog != null && uiCharacterItem != null)
                    instance = TooltipConfig.Singleton.uiItemDialog;
                if (TooltipConfig.Singleton?.uiSkillDialog != null && uiCharacterSkill != null)
                    instance = TooltipConfig.Singleton.uiSkillDialog;
                if (instance != null)
                {
                    uiRenderer = instance.GetComponent<Renderer>();
                }
            }
        }
        void OnDisable()
        {

        }


        void CalculateTooltipPosition()
        {
            if (canvas == null)
                canvas = Object.FindObjectOfType<UISceneGameplay>()?.GetComponent<Canvas>();
            float scale = canvas.scaleFactor;
            RectTransform parentRect = GetComponent<RectTransform>();
            RectTransform rect = instance.GetComponent<RectTransform>();
            float halfWidth = rect.rect.width / 2 * scale;
            float halfHeight = rect.rect.height / 2 * scale;
            float parentHalfWidth = parentRect.rect.width / 2 * scale;
            float parentHalfHeight = parentRect.rect.height / 2 * scale;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            float xPos = transform.position.x + parentHalfWidth + halfWidth;
            float yPos = transform.position.y;
            if (xPos + halfWidth > screenWidth)
                xPos = transform.position.x - parentHalfWidth - halfWidth;
            if (transform.position.y + halfHeight > screenHeight)
                yPos = transform.position.y + halfHeight + (screenHeight - (transform.position.y + halfHeight));
            if (transform.position.y - halfHeight < 0)
                yPos = transform.position.y - (transform.position.y - halfHeight);
            instance.transform.position = new Vector3(xPos, yPos, 0);
            if (!instance.gameObject.activeSelf)
                instance.gameObject.SetActive(true);

        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (instance != null && !instance.gameObject.activeSelf)
            {
                if (instance is UICharacterItem && uiCharacterItem.Data.characterItem?.GetItem() != null)
                {
                    UICharacterItem ui = instance as UICharacterItem;
                    ui.Setup(uiCharacterItem.Data, uiCharacterItem.Character, uiCharacterItem.IndexOfData);
                    CalculateTooltipPosition();
                }
                if (instance is UICharacterSkill && uiCharacterSkill.Data.characterSkill?.GetSkill() != null)
                {
                    UICharacterSkill ui = instance as UICharacterSkill;
                    ui.Setup(uiCharacterSkill.Data, uiCharacterSkill.Character, uiCharacterSkill.IndexOfData);
                    CalculateTooltipPosition();
                }
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (instance != null && instance.gameObject.activeSelf)
            {
                instance.gameObject.SetActive(false);
            }
        }
    }
}