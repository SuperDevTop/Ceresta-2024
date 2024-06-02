using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
public class ShopTemplate : MonoBehaviour
{
    public ShopItem refShopItem;
    public TMP_Text amountText;
    public TMP_Text unitsText;

    public TMP_Text itemNameText;
    public Image itemImage;
    public string addressableGroup = "Assets/AddressableSprites/Shop&Inventory";

  //  public List<AsyncOperationHandle<Sprite>> handleList;
    void Start()
    {
        // Invoke(nameof(PopulateDetails), 1f);       
    }
    public void PopulateDetails()
    {
        amountText.text = refShopItem.price.ToString();
        unitsText.text = refShopItem.quantity.ToString();
        itemNameText.text = refShopItem.name.ToString();
        LoadImageAsync(itemImage, addressableGroup + "/" + refShopItem.item_id + ".png");
    }

    private async void LoadImageAsync(Image image, string addressableKey)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(addressableKey);

        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite sprite = handle.Result;
            image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Failed to load image: " + handle.DebugName);
        }

       // Addressables.Release(handle);
       // handleList.Add(handle);
        UIController.Instance.shopInventoryHandleList.Add(handle);
    }
    public void OnClickShopItem()
    {

        UIController.Instance.selectedShopItemID = refShopItem.item_id;

       // UIController.Instance.selectedShopItemQuantity = refShopItem.quantity;
        UIController.Instance.selectedShopItemQuantity = 1;

        //Open Shop Qty Panel
        UIController.Instance.shopQtyInput.text = 1.ToString();

        UIController.Instance.shopQtyPanel.SetActive(true);
        UIController.Instance.UpdateQtyShopImage(itemImage);
    }

    public void OnClickInventoryItem()
    {

        UIController.Instance.selectedInventoryItemID = refShopItem.item_id;

        // UIController.Instance.selectedInventoryItemQuantity = refShopItem.quantity;
        UIController.Instance.selectedInventoryItemQuantity = 1;
        //Open Inventory Qty Panel
        UIController.Instance.inventoryQtyInput.text = 1.ToString();
        UIController.Instance.inventoryQtyPanel.SetActive(true);
        UIController.Instance.UpdateQtyInventoryImage(itemImage);
    }
}
