using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainPun : MonoBehaviourPunCallbacks
{
    public GameObject mainUI;
    public GameObject roomUI;
    public GameObject loadingUI;
    public GameObject[] roomList;
    public GameObject[] roomListsObject;
    public Text[] playersNumText;
    public Text alertText;
    public int maxPlayers;
    public static bool isPlaying;
    public bool isConnectedFirst;

    List<RoomInfo> rooms = new List<RoomInfo>();    

    string gameVersion = "1";

    private void Awake()
    {
        isPlaying = false;
        isConnectedFirst = true;     
    }

    void Start()
    {
          
    }

    void Update()
    {
        
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnJoinedLobby()
    {
        loadingUI.SetActive(false);
        mainUI.SetActive(false);
        roomUI.SetActive(true);      
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomLists)
    {
        if (isConnectedFirst)
        {
            isConnectedFirst = false;
            rooms = roomLists;
        }
        else
        {
            foreach (RoomInfo info in roomLists)
            {
                if (info.RemovedFromList)
                {
                    rooms.Remove(info);          
                }
                else
                {
                    rooms.Add(info);                                 
                }
            }
        }

        RefreshRoomList();
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.NickName = UIController.Instance.profileUsername.text;        
        SceneManager.LoadScene("GameScene01");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //loadingUI.SetActive(false);
        //StartCoroutine(DelayToShowAlert("Please join again..."));
        CreateGame();
    }

    // UI
    public void PlayGame()
    {
        ConnectToRegion();
        loadingUI.SetActive(true);
    }

    public void HomeBtnClick()
    {
        PhotonNetwork.Disconnect();
        mainUI.SetActive(true);
        roomUI.SetActive(false);
        isConnectedFirst = true;
    }

    // Connect to region
    void ConnectToRegion()
    {
        AppSettings regionSettings = new()
        {
            UseNameServer = true,
            //FixedRegion = "usw",
            FixedRegion = "asia",
            AppIdRealtime = "f23d6adc-4a05-416a-98b6-9a718aa89fb9",
            AppVersion = gameVersion,
        };
        PhotonNetwork.ConnectUsingSettings(regionSettings);
    }

    public void RefreshRoomList()
    {
        print("Refresh room: " + rooms.Count);

        for (int i = 0; i < 12; i++)
        {
            roomListsObject[i].name = "room";
            playersNumText[i].text = "0 / 30";
        }

        List<int> removedRooms = new List<int>();

        for (int i = 0; i < rooms.Count - 1; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                if (rooms[i].Name == rooms[j].Name)
                {
                    removedRooms.Add(i);
                }
            }
        }

        for (int i = 0; i < removedRooms.Count; i++)
        {
            print("Removed: " + rooms[removedRooms[i]].Name);
            print("Removed: " + rooms[removedRooms[i]].PlayerCount);
            rooms.RemoveAt(removedRooms[i]);            
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].IsOpen && rooms[i].IsVisible && rooms[i].PlayerCount < 30)
            {
                roomListsObject[i].name = rooms[i].Name;
                playersNumText[i].text = rooms[i].PlayerCount.ToString() + " / 30";
            }
        }
    }

    // Create room
    public void CreateGame()
    {
        string roomName = "room" + UnityEngine.Random.Range(5, 100).ToString() + System.DateTime.Now.Minute + System.DateTime.Now.Second;

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = maxPlayers;

        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    // Join room
    public void JoinRoom()
    {
        bool isFulled = false;
        string roomName = EventSystem.current.currentSelectedGameObject.name;

        for (int i = 0; i < rooms.Count; i++)
        {
            if(rooms[i].Name == roomName && rooms[i].PlayerCount == 30)
            {
                isFulled = true;
                break;
            }
        }

        if (isFulled)
        {
            StartCoroutine(DelayToShowAlert("Room is fulled."));
        }
        else
        {
            PhotonNetwork.JoinRoom(roomName);
            loadingUI.SetActive(true);
        }                
    } 

    IEnumerator DelayToShowAlert(string str)
    {
        alertText.gameObject.SetActive(true);
        alertText.text = str;
        yield return new WaitForSeconds(3f);
        alertText.gameObject.SetActive(false);
    }
}
