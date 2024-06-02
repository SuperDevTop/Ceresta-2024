using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviourPunCallbacks
{
    public GameObject gameUI;
    public Text roomNameText;
    public Text gameStatus;
    public Text networkStatus;
    public string roomName;
    public int maxPlayers;
    string gameVersion = "1";

    private void Awake()
    {
        //Application.runInBackground = true;
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 30;

        //PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        gameStatus.text = "Connecting ...";
        ConnectToRegion();
    }

    void Update()
    {
        gameUI.transform.localScale = new Vector3(Screen.width / 720f, Screen.height / 1440f, 1f);
        networkStatus.text = PhotonNetwork.NetworkClientState.ToString();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        
    }

    public override void OnJoinedLobby()
    {
        gameStatus.text = "Creating room ...";

        //RoomOptions roomOptions = new RoomOptions();
        //roomOptions.IsOpen = true;
        //roomOptions.IsVisible = true;
        //roomOptions.MaxPlayers = maxPlayers;

        //PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        gameStatus.text = "Created room";    
        roomNameText.text = PhotonNetwork.CurrentRoom.Name + ": " + (PhotonNetwork.CurrentRoom.PlayerCount - 1);
        //SceneManager.LoadScene("GameScene");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to create room: " + message);
        ConnectToRegion();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name + ": " + (PhotonNetwork.CurrentRoom.PlayerCount - 1);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomNameText.text = PhotonNetwork.CurrentRoom.Name + ": " + (PhotonNetwork.CurrentRoom.PlayerCount - 1);
    }

    void ConnectToRegion()
    {
        AppSettings regionSettings = new()
        {
            UseNameServer = true,
            FixedRegion = "asia",
            AppIdRealtime = "f23d6adc-4a05-416a-98b6-9a718aa89fb9",
            AppVersion = gameVersion,
        };
        PhotonNetwork.ConnectUsingSettings(regionSettings);
    }

    public void CloseRoom()
    {
        PhotonNetwork.DestroyAll();
    }
}
