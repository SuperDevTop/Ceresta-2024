using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using TMPro;

public class ShopControl : MonoBehaviour
{
    public GameObject contentParent;

    [Header("Shop Ref")]
    public Transform templateShopParent;
    public GameObject buttonShopTemplate;
    public TMP_InputField searchShop;
    public FilterContent shopFilterContentRef;

    [Header("Inventory Ref")]
    public Transform templateInventoryParent;
    public GameObject buttonInventoryTemplate;
    public TMP_InputField searchInv;
    public FilterContent invFilterContentRef;

    public static ShopControl instance;
    public int lastSelectedItemID;


    private void Awake()
    {
        //if (instance != null)
        //{
        //    Destroy(instance);
        //}
        instance = this;
    }
    private void OnEnable()
    {
        GetShopList();
        GetInventoryList();
    }
    public void GetInventoryList()
    {
        ShopManager.Instance.GetInventoryList();
    }
    public void GetShopList()
    {
        ShopManager.Instance.GetStoreList();
    }

    public void BuyFromShop()
    {
        ShopManager.Instance.BuyItem(UIController.Instance.selectedShopItemID, UIController.Instance.selectedShopItemQuantity); //Buys from Shop

      //  searchShop.text = "";

    }

    public void SellFromInventory()
    {
        ShopManager.Instance.SellItem(UIController.Instance.selectedInventoryItemID, UIController.Instance.selectedInventoryItemQuantity); //Sells from inventory
       // searchInv.text = "";

    }

    public void LoadShopList()
    {
        templateShopParent = buttonShopTemplate.transform.parent;
        foreach (Transform obj in buttonShopTemplate.transform.parent)
        {
            if (obj.gameObject.activeInHierarchy)
            {
                Destroy(obj.gameObject);
            }
            Debug.Log(":Des");
        }
        if (buttonShopTemplate != null)
        {
            templateShopParent = buttonShopTemplate.transform.parent;

            Debug.Log("::");

            foreach (var item in ShopManager.Instance.allShopItems)
            {
                GameObject obj = Instantiate(buttonShopTemplate, buttonShopTemplate.transform.parent);
                obj.GetComponent<ShopTemplate>().refShopItem = item;
                obj.GetComponent<ShopTemplate>().PopulateDetails();
                obj.SetActive(true);


                Debug.Log(":::");
            }


        }

        //FILTER CONTENT OF SHOP HERE-
        shopFilterContentRef.FilterScrollViewContent(searchShop.text);

    }
    /*  private async void LoadImageAsync(Image image, string addressableKey)
      {
          // Load the image asynchronously using its addressable key
          AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(addressableKey);

          // Wait until the operation is completed
          await handle.Task;

          // Check if the operation is valid and successful
          if (handle.Status == AsyncOperationStatus.Succeeded)
          {
              // Get the loaded sprite
              Sprite sprite = handle.Result;

              // Set the sprite to the Image component
              image.sprite = sprite;
          }
          else
          {
              // Handle the case where loading the image failed
              Debug.LogError("Failed to load image: " + handle.DebugName);
          }

          // Release the operation handle to free up resources
          Addressables.Release(handle);
      }*/
    public void LoadInventoryList()
    {
       

        if (UIController.Instance.inventoryPanel.activeInHierarchy)
        {
            templateInventoryParent = buttonInventoryTemplate.transform.parent;
            foreach (Transform obj in buttonInventoryTemplate.transform.parent)
            {
                if (obj.gameObject.activeInHierarchy)
                {
                    Destroy(obj.gameObject);
                }
                Debug.Log(":Des");
            }
            if (buttonInventoryTemplate != null)
            {
                templateInventoryParent = buttonInventoryTemplate.transform.parent;

                Debug.Log("::");

                if (ShopManager.Instance != null)
                {
                    foreach (var item in ShopManager.Instance.allInventoryItems)
                    {
                        GameObject obj = Instantiate(buttonInventoryTemplate, templateInventoryParent);
                        obj.GetComponent<ShopTemplate>().refShopItem = item;
                        obj.GetComponent<ShopTemplate>().PopulateDetails();
                        obj.SetActive(true);


                    }
                }
                else
                {
                    Debug.LogError("No shop manager is found");

                }


            }
        }
        else
        {
            templateInventoryParent = buttonInventoryTemplate.transform.parent;
            foreach (Transform obj in buttonInventoryTemplate.transform.parent)
            {
                if (obj.gameObject != buttonInventoryTemplate)
                {
                    Destroy(obj.gameObject);
                }
                Debug.Log(":Des");
            }
            if (buttonInventoryTemplate != null)
            {
                templateInventoryParent = buttonInventoryTemplate.transform.parent;

                Debug.Log("::");

                if (ShopManager.Instance != null)
                {
                    foreach (var item in ShopManager.Instance.allInventoryItems)
                    {
                        GameObject obj = Instantiate(buttonInventoryTemplate, templateInventoryParent);
                        obj.GetComponent<ShopTemplate>().refShopItem = item;
                        obj.GetComponent<ShopTemplate>().PopulateDetails();
                        obj.SetActive(true);


                    }
                }
                else
                {
                    Debug.LogError("No shop manager is found");

                }


            }
        }

        //FILTER CONTENT OF INVENTORY CALL HERE-

        invFilterContentRef.FilterScrollViewContent(searchInv.text);
        
    }
}

