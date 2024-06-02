using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
//using UnityEditor.Timeline;

public class UIController : MonoBehaviour
{
    [Header("UI PANELS")]
    public GameObject LoginUI;
    public GameObject SignUpUI;
    public GameObject MainGameUI;
    public GameObject NotificationPanel;
    public GameObject loadingSpinner;

    [Header("UI PANELS Text")]
    public TMP_Text NotifyText;

    [Header("Profile Panel")]
    public TMP_Text profileUsername;
    public GameObject profilePanel;
    public Image profileImage;
    public TMP_Text profileGoldText;

    [Header("Main Game Panel")]
    public TMP_Text timeText;
    public TMP_Text playerUsernameText;
    public TMP_Text playerGoldText;

    [Header("Login in Setting Panel")]
    public GameObject LoginBtnSettings;
    public GameObject myProfileBtnSettings;


    [Header("Shop and Inventory Panel")]
    public GameObject shopPanel;
    public GameObject inventoryPanel;
    public TMP_Text shopPlayerGoldText;

    public GameObject shopQtyPanel;
    public TMP_InputField shopQtyInput;
    public Image shopItemImage;

    public GameObject inventoryQtyPanel;
    public TMP_InputField inventoryQtyInput;
    public Image inventoryItemImage;

    public GameObject loadingShopIcon;
    public GameObject loadingInventoryIcon;

    [Header("Job Req. Ref")]
    // public ItemRequire reqItem;
    public GameObject redSetJobImage;
    public Button confirmBtn;

    static UIController instance;
    public static UIController Instance => instance;

    public int selectedJobID;

    public int selectedShopItemID;
    public int selectedShopItemQuantity;

    public int selectedInventoryItemID;
    public int selectedInventoryItemQuantity;

    public Image selectedJobImage;
    public List<AsyncOperationHandle<Sprite>> shopInventoryHandleList = new List<AsyncOperationHandle<Sprite>>(); //List of addressable images loaded in shop & inventory

    public List<AsyncOperationHandle<Sprite>> jobHandeList = new List<AsyncOperationHandle<Sprite>>();

    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.name = GetType().Name;

    }
    private void Start()
    {
        shopQtyInput.onValueChanged.AddListener(ShopItemValueChanged);
        inventoryQtyInput.onValueChanged.AddListener(InventoryItemValueChanged);
    }
    private void ShopItemValueChanged(string itemQty)
    {
        int qty = int.Parse(itemQty);

        if (qty < 1)
        {
            qty = 1;
            shopQtyInput.text = qty + "";

        }
        selectedShopItemQuantity = qty;

    }
    public void PlayerHaveReqItemsJob()
    {
        redSetJobImage.SetActive(false);
        confirmBtn.interactable = true;
        confirmBtn.gameObject.SetActive(true);
    }
    public void PlayerDontHaveReqItemsJob()
    {
        redSetJobImage.SetActive(true);
        confirmBtn.interactable = false;
        confirmBtn.gameObject.SetActive(true);
    }
    public void PlayerCurrentJob()
    {
        redSetJobImage.SetActive(false);
        confirmBtn.interactable = true;
        confirmBtn.gameObject.SetActive(false);
    }
    private void InventoryItemValueChanged(string itemQty)
    {
        int qty = int.Parse(itemQty);

        if (qty < 1)
        {
            qty = 1;
            inventoryQtyInput.text = qty + "";

        }

        selectedInventoryItemQuantity = qty;

    }
    public void ChangeInShopItemValue(int addOrSubtract)
    {
        int qty = int.Parse(shopQtyInput.text);
        qty += addOrSubtract;
        if (qty < 1)
        {
            qty = 1;

        }
        if (qty > 999)
        {
            qty = 999;
        }
        shopQtyInput.text = qty + "";
        selectedShopItemQuantity = qty;
    }
    public void ChangeInInventoryValue(int addOrSubtract)
    {
        int qty = int.Parse(inventoryQtyInput.text);
        qty += addOrSubtract;
        if (qty < 1)
        {
            qty = 1;

        }
        if (qty > 999)
        {
            qty = 999;
        }
        inventoryQtyInput.text = qty + "";
        selectedInventoryItemQuantity = qty;
    }
    public void UpdatePlayerCoin(int Coins)
    {
        int currentGold = PlayerPrefs.GetInt("UserGold");

        currentGold += Coins;
        PlayerPrefs.SetInt("UserGold", currentGold);
        UpdateGoldText();
    }
    /*  public void CheckIfUserLoggedIn()
      {
          if (!String.IsNullOrEmpty(PlayerPrefs.GetString("Email").ToString()))
          {
              UserLoggedInUI();
          }
      }*/

    public void ReleaseShopInvAddressableImg()
    {
        int count = 0;
        foreach (AsyncOperationHandle<Sprite> handle in shopInventoryHandleList)
        {
            count++;
            Debug.Log("Releasing handle" + count);
            Addressables.Release(handle);

        }
    }
    public void UserRegistered()
    {
        SignUpUI.SetActive(false);
        LoginUI.SetActive(true);
    }

    public void UserLoggedInUI()
    {
        SignUpUI.SetActive(false);
        LoginUI.SetActive(false);

        MainGameUI.SetActive(true);

        string[] strings = PlayerPrefs.GetString("Email").Split('@');
        profileUsername.text = strings[0];
        playerUsernameText.text = strings[0];

        // playerUsernameText.transform.parent.gameObject.SetActive(true);
        LoginBtnSettings.SetActive(false);
        myProfileBtnSettings.SetActive(true);
        UpdateGoldText();
        //TimeShower.Instance.CountdownShow(serverTimeUtc.ToString("HH:mm:ss"));

    }
    public void UpdateGoldText()
    {
        playerGoldText.text = PlayerPrefs.GetInt("UserGold").ToString();
        shopPlayerGoldText.text = PlayerPrefs.GetInt("UserGold").ToString();
        profileGoldText.text = PlayerPrefs.GetInt("UserGold").ToString();
    }
    public void GiveNotification(string notification)
    {
        NotifyText.text = notification;
        NotificationPanel.SetActive(true);


    }


    public void UserLoggedOutUI()
    {
        SignUpUI.SetActive(false);
        MainGameUI.SetActive(false);
        profilePanel.SetActive(false);

        LoginUI.SetActive(true);
        LoginBtnSettings.SetActive(true);
    }

    public void UpdateQtyShopImage(Image refImage)
    {
        shopItemImage.sprite = refImage.sprite;
    }
    public void UpdateQtyInventoryImage(Image refImage)
    {
        inventoryItemImage.sprite = refImage.sprite;
    }
    public void UpdateProfileImageJobChange()
    {
        profileImage.sprite = selectedJobImage.sprite;
    }
    public void UpdateProfileImageLogin()
    {
        int jobId = PlayerPrefs.GetInt("UserJobId");
        string addressableGroup = "Assets/AddressableSprites/Jobs";

        foreach (JobItem item in LoginManager.Instance.allJobItems)
        {

            if (item.id == jobId)
            {

                LoadImageAsync(profileImage, addressableGroup + "/" + item.name.ToLower() + ".png");

            }
        }
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

    }


}
