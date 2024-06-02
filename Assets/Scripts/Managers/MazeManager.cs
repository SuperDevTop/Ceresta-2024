using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    static MazeManager instance;
    public static MazeManager Instance => instance;
    public void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
            gameObject.name = GetType().Name;
        }
    }

    public void GetStartTime()
    {
        WebRequestHandler<GetStartTimeResponse> request = new WebRequestHandler<GetStartTimeResponse>(OnGotStartTimeResponse);
        string url = "/game/get_starttime/";

        StartCoroutine(request.SendRequestAsync(url, "GET", ""));
    }

    
    private void OnGotStartTimeResponse(WebResponse<GetStartTimeResponse> response)
    {
        if (response.status == 1)
        {
            Debug.Log("Got start time.");

        }
        else
        {
            Debug.Log("Getting start time issue:" + response.message);
        }

        Debug.Log(response);
        Debug.Log(response.message);
        Debug.Log(response.detail);
        Debug.Log(response.data);
        
       // UIController.Instance.timeText.text = response.message;

    }
    public void GetMazeOpen()
    {
        WebRequestHandler<IsMazeOpenResponse> request = new WebRequestHandler<IsMazeOpenResponse>(IsMazeOpenResponse);
        string url = "/game/is_open/";

        StartCoroutine(request.SendRequestAsync(url, "GET", ""));
    }

   
    private void IsMazeOpenResponse(WebResponse<IsMazeOpenResponse> response)
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

        Debug.Log(response.detail);
    }

    public void GetMapData()
    {
        WebRequestHandler<GetMazeMapDataResponse> request = new WebRequestHandler<GetMazeMapDataResponse>(GetMapDataResponse);
        string url = "/game/get_map_data/";

        StartCoroutine(request.SendRequestAsync(url, "GET", ""));
    }

 
    private void GetMapDataResponse(WebResponse<GetMazeMapDataResponse> response)
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

    public void GetMazeReward(int mazeId, int boxType, string token)
    {

        WebRequestHandler<GetRewardResponse> request = new WebRequestHandler<GetRewardResponse>(OnGotMazeRewardResponse);
        string url = "/game/get_reward/";

        string data = JsonUtility.ToJson(new GetRewardRequest(mazeId, boxType, token));
        StartCoroutine(request.SendRequestAsync(url, "POST", data));
    }

    private void OnGotMazeRewardResponse(WebResponse<GetRewardResponse> response)
    {
        if (response.status == 1)
        {
            Debug.Log("Got reward succ");

        }
        else
        {
            Debug.Log("Getting reward issue:" + response.message);

        }
    }

}

[Serializable]
public class GetStartTimeResponse
{
    [SerializeField]
    public string time;

}

[Serializable]
public class IsMazeOpenResponse
{
    [SerializeField]
    public string open;

}

[Serializable]
public class GetMazeMapDataResponse
{
    [SerializeField]
    public string open;

}


[Serializable]
public class GetRewardRequest
{
    [SerializeField]
    public int maze_id;
    [SerializeField]
    public int box_type;
    [SerializeField]
    public string token;

    [SerializeField]
    public GetRewardRequest(int mazeId, int boxType, string token)
    {
        this.maze_id= mazeId;
        this.box_type= boxType;
        this.token= token;
    }
}

[Serializable]
public class GetRewardResponse
{
    [SerializeField]
    public string open;

    [SerializeField]
    public string loc;
    [SerializeField]
    public string msg;
    [SerializeField]
    public string type;

}
