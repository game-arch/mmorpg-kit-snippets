using UnityEngine;
using UnityEngine.UI;
using MultiplayerARPG;

namespace Context
{
    public class ItemContextMenuButtonActivator : MonoBehaviour
    {
        public Button buttonEquip;
        public Button buttonUnEquip;
        public Button buttonUse;
        public Button buttonRefine;
        public Button buttonDismantle;
        public Button buttonRepair;
        public Button buttonSocketEnhance;
        public Button buttonSell;
        public Button buttonOffer;
        public Button buttonMoveToStorage;
        public Button buttonMoveFromStorage;
        public Button buttonDrop;
        private UICharacterItem ui;
        private void Start()
        {
            ui = GetComponent<ItemContextMenu>().item;
            ui.onSetEquippedData.AddListener(OnSetEquippedData);
            ui.onSetUnEquippedData.AddListener(OnSetUnEquippedData);
            ui.onSetUnEquippableData.AddListener(OnSetUnEquippableData);
            ui.onSetUsableData.AddListener(OnSetUsableData);
            ui.onSetStorageItemData.AddListener(OnSetStorageItemData);
            ui.onRefineItemDialogAppear.AddListener(OnRefineItemDialogAppear);
            ui.onRefineItemDialogDisappear.AddListener(OnRefineItemDialogDisappear);
            ui.onDismantleItemDialogAppear.AddListener(OnDismantleItemDialogAppear);
            ui.onDismantleItemDialogDisappear.AddListener(OnDismantleItemDialogDisappear);
            ui.onRepairItemDialogAppear.AddListener(OnRepairItemDialogAppear);
            ui.onRepairItemDialogDisappear.AddListener(OnRepairItemDialogDisappear);
            ui.onNpcSellItemDialogAppear.AddListener(OnNpcSellItemDialogAppear);
            ui.onNpcSellItemDialogDisappear.AddListener(OnNpcSellItemDialogDisappear);
            ui.onStorageDialogAppear.AddListener(OnStorageDialogAppear);
            ui.onStorageDialogDisappear.AddListener(OnStorageDialogDisappear);
            ui.onEnterDealingState.AddListener(OnEnterDealingState);
            ui.onExitDealingState.AddListener(OnExitDealingState);
            // Refresh UI data to applies events
            ui.ForceUpdate();
        }

        public void DeactivateAllButtons()
        {
            if (buttonEquip)
                buttonEquip.gameObject.SetActive(false);
            if (buttonUnEquip)
                buttonUnEquip.gameObject.SetActive(false);
            if (buttonUse)
                buttonUse.gameObject.SetActive(false);
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(false);
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(false);
            if (buttonRepair)
                buttonRepair.gameObject.SetActive(false);
            if (buttonSocketEnhance)
                buttonSocketEnhance.gameObject.SetActive(false);
            if (buttonSell)
                buttonSell.gameObject.SetActive(false);
            if (buttonOffer)
                buttonOffer.gameObject.SetActive(false);
            if (buttonMoveToStorage)
                buttonMoveToStorage.gameObject.SetActive(false);
            if (buttonMoveFromStorage)
                buttonMoveFromStorage.gameObject.SetActive(false);
            if (buttonDrop)
                buttonDrop.gameObject.SetActive(false);
        }

        public void OnSetEquippedData()
        {
            DeactivateAllButtons();
            if (buttonUnEquip)
                buttonUnEquip.gameObject.SetActive(true);
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(GameInstance.Singleton.canRefineItemByPlayer);
            if (buttonRepair)
                buttonRepair.gameObject.SetActive(GameInstance.Singleton.canRepairItemByPlayer);
            if (buttonSocketEnhance)
                buttonSocketEnhance.gameObject.SetActive(true);
        }

        public void OnSetUnEquippedData()
        {
            DeactivateAllButtons();
            if (buttonEquip)
                buttonEquip.gameObject.SetActive(true);
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(GameInstance.Singleton.canRefineItemByPlayer);
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(GameInstance.Singleton.canDismantleItemByPlayer && GameInstance.Singleton.dismantleFilter.Filter(ui.CharacterItem));
            if (buttonRepair)
                buttonRepair.gameObject.SetActive(GameInstance.Singleton.canRepairItemByPlayer);
            if (buttonSocketEnhance)
                buttonSocketEnhance.gameObject.SetActive(true);
            if (buttonDrop)
                buttonDrop.gameObject.SetActive(true);
        }

        public void OnSetUnEquippableData()
        {
            DeactivateAllButtons();
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(GameInstance.Singleton.canDismantleItemByPlayer && GameInstance.Singleton.dismantleFilter.Filter(ui.CharacterItem));
            if (buttonDrop)
                buttonDrop.gameObject.SetActive(true);
        }

        public void OnSetUsableData()
        {
            DeactivateAllButtons();
            if (buttonUse)
                buttonUse.gameObject.SetActive(true);
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(GameInstance.Singleton.canDismantleItemByPlayer && GameInstance.Singleton.dismantleFilter.Filter(ui.CharacterItem));
            if (buttonDrop)
                buttonDrop.gameObject.SetActive(true);
        }

        public void OnSetStorageItemData()
        {
            DeactivateAllButtons();
            if (buttonMoveFromStorage)
                buttonMoveFromStorage.gameObject.SetActive(true);
        }

        public void OnRefineItemDialogAppear()
        {
            if (GameInstance.Singleton.canRefineItemByPlayer)
                return;

            if (buttonRefine)
                buttonRefine.gameObject.SetActive(true);
        }

        public void OnRefineItemDialogDisappear()
        {
            if (GameInstance.Singleton.canRefineItemByPlayer)
                return;

            if (buttonRefine)
                buttonRefine.gameObject.SetActive(false);
        }

        public void OnDismantleItemDialogAppear()
        {
            if (GameInstance.Singleton.canDismantleItemByPlayer)
                return;

            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(GameInstance.Singleton.dismantleFilter.Filter(ui.CharacterItem));
        }

        public void OnDismantleItemDialogDisappear()
        {
            if (GameInstance.Singleton.canDismantleItemByPlayer)
                return;

            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(false);
        }
        public void OnRepairItemDialogAppear()
        {
            if (GameInstance.Singleton.canRepairItemByPlayer)
                return;

            if (buttonRepair)
                buttonRepair.gameObject.SetActive(true);
        }

        public void OnRepairItemDialogDisappear()
        {
            if (GameInstance.Singleton.canRepairItemByPlayer)
                return;

            if (buttonRepair)
                buttonRepair.gameObject.SetActive(false);
        }

        public void OnNpcSellItemDialogAppear()
        {
            if (buttonSell)
                buttonSell.gameObject.SetActive(true);
        }

        public void OnNpcSellItemDialogDisappear()
        {
            if (buttonSell)
                buttonSell.gameObject.SetActive(false);
        }

        public void OnStorageDialogAppear()
        {
            if (buttonMoveToStorage)
                buttonMoveToStorage.gameObject.SetActive(true);
        }

        public void OnStorageDialogDisappear()
        {
            if (buttonMoveToStorage)
                buttonMoveToStorage.gameObject.SetActive(false);
        }

        public void OnEnterDealingState()
        {
            if (buttonOffer)
                buttonOffer.gameObject.SetActive(true);
        }

        public void OnExitDealingState()
        {
            if (buttonOffer)
                buttonOffer.gameObject.SetActive(false);
        }
    }
}
