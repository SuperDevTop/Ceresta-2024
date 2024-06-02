using Facebook.MiniJSON;
using Facebook.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
//using static UnityEditor.Progress;
//using static UnityEditor.Progress;
//using Newtonsoft.Json;

public class ShopManager : MonoBehaviour
{
    static ShopManager instance;
    public static ShopManager Instance => instance;

    //  public List<Item> allShopItems;
    public List<ShopItem> allShopItems;
    public List<ShopItem> allInventoryItems;
    public ShopControl shopControlRef;

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.name = GetType().Name;
    }

    public void GetStoreList()
    {

        WebRequestHandler<GetStoreListResponse> request = new WebRequestHandler<GetStoreListResponse>(OnGotStoreListResponse);
        string url = "/shop/get_store_list/";
        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");
        StartCoroutine(request.SendRequestAsync(url, "GET", accesstoken, ""));
    }

    // Response to store list
    private void OnGotStoreListResponse(WebResponse<GetStoreListResponse> response)
    {
        /* if (response.status == 1)
         {
             Debug.Log("Got store list.");
           //  OnPurchaseResult.Invoke(response.data.items);
         }
         else
         {
             Debug.Log("Store list issue:" + response.message);
         }*/
        // allShopItems = response.data.items;
        // allShopItems = JsonConvert.DeserializeObject<List<Item>>(response.ToString());
        //allShopItems = JsonUtility.FromJson<Item[]>(response);
        // List<Item> itemList = JsonConvert.DeserializeObject<List<Item>>(json);

        //UIController.Instance.GiveNotification("Got Store List: " + response.message);
        Debug.Log(response.data.data);
        allShopItems = response.data.data;
        /*foreach(var item in response.data.data)
        {
            Debug.Log(item.name);
        }*/

        shopControlRef.LoadShopList();

    }

    public void GetInventoryList()
    {
        WebRequestHandler<GetInventoryListResponse> request = new WebRequestHandler<GetInventoryListResponse>(OnGotInventoryListResponse);
        string url = "/shop/get_inventory_list/";
        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");
        StartCoroutine(request.SendRequestAsync(url, "GET", accesstoken, ""));
    }

    // Response to inventory list
    private void OnGotInventoryListResponse(WebResponse<GetInventoryListResponse> response)
    {
        /* if (response.status == 1)
         {
             Debug.Log("Got Inventory list.");

         }
         else
         {
             Debug.Log("Inventory list issue:" + response.message);
         }*/

        //UIController.Instance.GiveNotification("Got Inventory List: " + response.message);
        Debug.Log("Should have got the inventory list. Response data- " + response.data.data);
        allInventoryItems.Clear();
        allInventoryItems = response.data.data;

        shopControlRef.LoadInventoryList();
    }

    public void SellItem(int itemID, int quantity)
    {
        WebRequestHandler<SellItemResponse> request = new WebRequestHandler<SellItemResponse>(OnSoldItemResponse);
        string url = "/shop/sell_item/";
        string data = JsonUtility.ToJson(new SellRequest(itemID, quantity));
        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");
        StartCoroutine(request.SendRequestAsync(url, "POST", accesstoken, data));
    }

    // Response to sold item
    private void OnSoldItemResponse(WebResponse<SellItemResponse> response)
    {
        if (response.status == 1)
        {
            Debug.Log("Sold item succ.");

        }
        else
        {
            Debug.Log("Selling item issue:" + response.message);
        }

        Debug.Log(response);
        LoginManager.Instance.GetGold();
        UIController.Instance.GiveNotification("Sell Item: " + response.message);
        UIController.Instance.inventoryQtyPanel.SetActive(false);

        UIController.Instance.loadingInventoryIcon.SetActive(false);
        GetInventoryList();

    }
    public void BuyItem(int itemID, int quantity)
    {
        WebRequestHandler<BuyItemResponse> request = new WebRequestHandler<BuyItemResponse>(OnBuyItemResponse);
        string url = "/shop/buy_item/";
        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");
        Debug.Log("access token from player prefs: " + accesstoken);
        string data = JsonUtility.ToJson(new BuyRequest(itemID, quantity));
        StartCoroutine(request.SendRequestAsync(url, "POST", accesstoken, data));
    }

    // Response to buy item
    private void OnBuyItemResponse(WebResponse<BuyItemResponse> response)
    {
        /* if (response.status == 1)
         {
             Debug.Log("Bought item succ.");

         }
         else
         {
             Debug.Log("Buying item issue:" + response.message);
         }*/

        Debug.Log(response);
        if (string.IsNullOrEmpty(response.detail))
        {
            UIController.Instance.GiveNotification("Buy Item: " + response.message);
            GetInventoryList();
            GetStoreList();

        }
        else
        {
            UIController.Instance.GiveNotification("Buy Item: " + response.detail);
        }
        UIController.Instance.shopQtyPanel.SetActive(false);
        LoginManager.Instance.GetGold();

        UIController.Instance.loadingShopIcon.SetActive(false);
    }

    public void GetItems()
    {
        WebRequestHandler<GetItemResponse> request = new WebRequestHandler<GetItemResponse>(OnGotItemsResponse);
        string url = "/shop/get_items/";

        StartCoroutine(request.SendRequestAsync(url, "GET", ""));
    }

    // Response to buy item
    private void OnGotItemsResponse(WebResponse<GetItemResponse> response)
    {
        if (response.status == 1)
        {
            Debug.Log("Got store list.");

        }
        else
        {
            Debug.Log("Store list issue:" + response.message);
        }

        Debug.Log(response);
    }
}

[Serializable]
public class GetStoreListResponse
{
    [SerializeField]
    public string message;
    [SerializeField]
    public List<ShopItem> data;
    /*[SerializeField]
    public Item items;*/
}
[Serializable]
public class ShopItem
{
    [SerializeField]
    public int id;
    [SerializeField]
    public int item_id;
    [SerializeField]
    public string name;
    [SerializeField]
    public int price;
    [SerializeField]
    public int quantity;
}

[Serializable]
public class GetInventoryListResponse
{
    [SerializeField]
    public string message;
    [SerializeField]
    public List<ShopItem> data;

}

[Serializable]
public class SellItemResponse
{
    [SerializeField]
    public string message;

}

[Serializable]
public class BuyItemResponse
{
    [SerializeField]
    public string message;

}

[Serializable]
public class GetItemResponse
{
    [SerializeField]
    public string message;

}
[Serializable]
public class BuyRequest
{
    [SerializeField]
    public int item_id;
    [SerializeField]
    public int quantity;

    public BuyRequest(int itemID, int quantity)
    {
        this.item_id = itemID;
        this.quantity = quantity;

    }
}

[Serializable]
public class SellRequest
{
    [SerializeField]
    public int item_id;
    [SerializeField]
    public int quantity;

    public SellRequest(int itemID, int quantity)
    {
        this.item_id = itemID;
        this.quantity = quantity;

    }
}
