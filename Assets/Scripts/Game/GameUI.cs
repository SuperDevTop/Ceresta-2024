using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GameUI : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [Serializable]
    public class ItemInfos
    {
        public List<ItemDetails> items;
    }

    [Serializable]
    public class ItemDetails
    {
        public string name;
        public int item_id;
        public int quantity;
        public int price;
    }

    public static GameUI instance;
    public GameObject gameUI;
    public GameObject loadingUI;
    public Text timeText;
    public float timeRemaining;
    //public GameObject playerprefab;
    string getTimeUrl = "https://backend-test-k12i.onrender.com/game/get_starttime/";
    public Text chatText;
    public Text doctorText;
    public GameObject chatTextDialog;
    public GameObject doctorDialog;
    public GameObject itemDialog;
    public GameObject stealBtn;
    public InputField chatInput;
    public ItemInfos itemInventory;
    public ItemDetails itemDetail;
    public string tempName;
    public static string playerName;
    public static string ownItems;
    public GameObject[] ownItemLists;
    public GameObject selectedObject;
    public static bool isAcceptedPolice;
    public Sprite thiefIcon;
    //public Text testText;

    void Start()
    {
        //StartCoroutine(GetServerTime(getTimeUrl));
        StartCoroutine(ShowLoadingUI());
        timeRemaining = 602f;
        isAcceptedPolice = false;                
    }

    void Update()
    {
        gameUI.transform.localScale = new Vector3(Screen.width / 720f, Screen.height / 1440f, 1f);
        //testText.text = Input.acceleration.magnitude.ToString();

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            DisplayTime(timeRemaining);
        }
        else
        {
            timeRemaining = 0;
            PhotonNetwork.Disconnect();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(Physics.Raycast(ray, out hit))
            {
                if(hit.transform.gameObject.tag == "Player" && !hit.transform.gameObject.GetComponent<PhotonView>().AmOwner)
                {      
                    itemDialog.SetActive(true);
                    selectedObject = hit.transform.gameObject;
                    ShowItemsOfPlayers((string)hit.transform.gameObject.GetComponent<PlayerManager>().itemInventory);

                    if(PlayerPrefs.GetInt("JOB_ID") == 5)
                    {
                        stealBtn.SetActive(true);
                    }
                }
            }
        }                
    }

    public void StealBtnclick()
    {
        string stolenName = selectedObject.gameObject.GetComponent<PlayerManager>().playerName;
        StartCoroutine(ShowChatText("You steal " + stolenName.Split(":")[0] + "'s item."));

        SyncSteal(new object[] { stolenName, playerName });
    }

    public void BackBtnClick()
    {
        if (PhotonNetwork.IsConnected)
        {
            MainPun.isPlaying = false;
            PhotonNetwork.Disconnect();
        }
    }

    public void SendBtnClick()
    {
        string sendString = PhotonNetwork.NickName + ": " + chatInput.text + " " + DateTime.UtcNow;
        chatInput.text = "";
        
        SyncMessage(new object[] { sendString });
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene("MenuScene");
    }    

    public void CloseRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SyncLeaveRoom(new object[] { 1 });
        }
    }

    public void ShowItem()
    {
        ShowItemsOfPlayers(ownItems);      
    }

    public void ShowItemsOfPlayers(string itemLists)
    {       
        itemInventory = JsonUtility.FromJson<ItemInfos>(itemLists);
        string addressableGroup = "Assets/AddressableSprites/Shop&Inventory";

        for (int i = 0; i < ownItemLists.Length; i++)
        {
            ownItemLists[i].SetActive(false);
        }

        if(itemInventory.items != null)
        {
            for (int i = 0; i < itemInventory.items.Count; i++)
            {
                ownItemLists[i].SetActive(true);
                ownItemLists[i].transform.GetChild(0).GetComponent<Text>().text = itemInventory.items[i].name;
                LoadImageAsync(ownItemLists[i].GetComponent<Image>(), addressableGroup + "/" + itemInventory.items[i].item_id + ".png");
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

        // Addressables.Release(handle);
        // handleList.Add(handle);
        Addressables.Release(handle);
    }

    public void HelpDoctorBtnClick()
    {
        SyncHelp(new object[] { playerName, 0 });      
    }

    public void HelpPoliceBtnClick()
    {
        SyncHelp(new object[] { playerName, 1 });      
    }

    public void AcceptBtnClick()
    {
        SyncAcceptHelp(new object[] { tempName, 0 });

        if (PlayerPrefs.GetInt("JOB_ID") == 6)
        {
            isAcceptedPolice = true;
        }
    }

    public void RejectBtnClick()
    {
        SyncAcceptHelp(new object[] { tempName, 1 });
    }

    public void SyncLeaveRoom(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncMessage(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(3, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncHelp(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(4, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncAcceptHelp(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(6, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SyncSteal(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(7, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == 1)
        {
            PhotonNetwork.Disconnect();
        }  
        else if(eventCode == 3)
        {
            object[] infos = (object[])photonEvent.CustomData;
            
            StopAllCoroutines();
            StartCoroutine(ShowChatText(infos[0].ToString()));
        }
        else if(eventCode == 4)
        {
            object[] infos = (object[])photonEvent.CustomData;
            string infoName = ((string)infos[0]).Split(":")[0];
            tempName = (string)infos[0];      

            if ((int)infos[1] == 0 && PlayerPrefs.GetInt("JOB_ID") == 4)
            {
                doctorDialog.SetActive(true);
                doctorText.text = infoName + " is calling you...";                
            }
            else if ((int)infos[1] == 1 && PlayerPrefs.GetInt("JOB_ID") == 6)
            {
                doctorDialog.SetActive(true);
                doctorText.text = infoName + " is calling you...";
            }
        }
        else if(eventCode == 6)
        {
            object[] infos = (object[])photonEvent.CustomData;

            if((string)infos[0] == playerName)
            {
                if ((int)infos[1] == 0)
                {
                    StartCoroutine(ShowChatText("Your request is accepted."));
                }
                else
                {
                    StartCoroutine(ShowChatText("Your request is rejected."));
                }
            }                        
        }
        else if(eventCode == 7)
        {
            object[] infos = (object[])photonEvent.CustomData;
            string victimName = infos[0].ToString().Split(":")[0];
            string thiefName = infos[1].ToString().Split(":")[0];           

            if((string)infos[0] == playerName)
            {
                StartCoroutine(ShowChatText("Your item is stolen by " + thiefName));
            }
            else
            {
                StartCoroutine(ShowChatText(thiefName + " steal " + victimName + "'s item."));
            }


            GameObject[] miniMapIcons = GameObject.FindGameObjectsWithTag("MiniMap");

            for (int i = 0; i < miniMapIcons.Length; i++)
            {
                if (miniMapIcons[i].GetComponent<MapObject>().playerName == (string)infos[1])
                {
                    miniMapIcons[i].GetComponent<Image>().sprite = thiefIcon;
                    break;
                }
            }
        }
    }

    IEnumerator GetServerTime(string url)
    {
        UnityWebRequest uwr = UnityWebRequest.Get(url);
        yield return uwr.SendWebRequest();
        loadingUI.SetActive(false);

        if (uwr.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);   
        }
    }

    IEnumerator ShowChatText(string chatStr)
    {
        chatText.text = chatStr;
        chatTextDialog.SetActive(true);

        yield return new WaitForSeconds(3f);

        chatTextDialog.SetActive(false);
    }

    IEnumerator ShowLoadingUI()
    {
        loadingUI.SetActive(true);
        yield return new WaitForSeconds(2f);
        loadingUI.SetActive(false);
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
