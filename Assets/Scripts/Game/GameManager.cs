using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public bool keyCollected = false;

    [Space(10)]
    public GameObject player;
    public GameObject prefabPlayer;
    //public GameObject arrowObject;
    public GameObject gridMap;

    [Space(10)]
    public GameObject firePrefab;
    public GameObject keyPrefab;
    public GameObject finishLine;

    public static GameObject LocalPlayerInstance;

    private void Awake()
    {
        if (PhotonNetwork.IsConnected && !MainPun.isPlaying && PlayerManager.LocalPlayerInstance == null)
        {
            player = PhotonNetwork.Instantiate(prefabPlayer.name, new Vector3(21.26996f, -139.9034f, 0), Quaternion.identity, 0);
        }

        if (!PhotonNetwork.IsConnected)
        {
            player = Instantiate(prefabPlayer, new Vector3(-0.13f, -0.16f, 0), Quaternion.identity);
        }

        instance = this;        
    }

    void Start()
    {
       
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.Log("OnPlayerEnteredRoom() " + other.NickName); // not seen if you're the player connecting   

        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 30)
            {
                PhotonNetwork.CurrentRoom.IsVisible = false;
                PhotonNetwork.CurrentRoom.IsOpen = false;
            }

            string wallInfos = JsonUtility.ToJson(GameMapGenerator.instance.removedInfo);
            SyncWallInfos(new object[] { wallInfos });
        }
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.Log("OnPlayerLeftRoom() " + other.NickName); // seen when other disconnects

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }
    }

    public void SyncWallInfos(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(2, content, raiseEventOptions, SendOptions.SendReliable);
    }        
}
