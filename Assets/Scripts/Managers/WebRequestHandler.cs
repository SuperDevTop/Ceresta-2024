using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Collections.Generic;

public class WebRequestHandler<T>
{
    Action<WebResponse<T>> OnResponse;
    string ServerURL= "https://backend-test-k12i.onrender.com";

    public WebRequestHandler(Action<WebResponse<T>> ResponseCallback)
    {
       // ServerURL = AppManager.Instance.currentEnvironment.ServerURL;
        OnResponse = ResponseCallback;
    }

    public IEnumerator SendRequestAsync(string endpoint, string type, string data = "")
    {
        string uri = ServerURL + endpoint;

        using (UnityWebRequest request = new UnityWebRequest(uri, type))
        {

            if (data != "")
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sending Request:" + uri);
            Debug.Log("payload:" + data);

            yield return request.SendWebRequest();

            WebResponse<T> response = new WebResponse<T>();
            string responseData;
            if (request.result == (UnityWebRequest.Result.ProtocolError | UnityWebRequest.Result.ConnectionError | UnityWebRequest.Result.DataProcessingError))
            {
                response.message = request.error;
                Debug.Log("Error:" + request.error);
            }
            else
            {
                Debug.Log("API URL:" + endpoint);
                Debug.Log("Response:" + request.downloadHandler.text);
               if (request.downloadHandler.text == "Internal Server Error")
                {
                    UIController.Instance.GiveNotification(request.downloadHandler.text);
                    UIController.Instance.loadingSpinner.SetActive(false);
                }

                if (request.downloadHandler.text.StartsWith("["))
                {
                    // responseData = "Detail: " + 
                    responseData = "{\"data\": {\"data\": " + request.downloadHandler.text + "} }";
                    Debug.Log(responseData);
                }
                else
                {
                    responseData = request.downloadHandler.text;
                }
                if ( (!string.IsNullOrEmpty(request.downloadHandler.text))&&((!request.downloadHandler.text.Contains(":"))||(request.downloadHandler.text.Contains("id"))))
                {
                    response = JsonUtility.FromJson<WebResponse<T>>(responseData);
                }
                if (request.downloadHandler.text.Contains(":")&& UIController.Instance.MainGameUI.activeInHierarchy)
                {
                    string[] parts = request.downloadHandler.text.Split(' ');
                    string[] smallerParts= parts[1].Split('"');


                    DateTime serverTimeUtc = DateTime.Parse(smallerParts[0]);

                   // UIController.Instance.timeText.text = serverTimeUtc.ToString("HH:mm");

                    if ((UIController.Instance.MainGameUI.activeInHierarchy)&&(!TimeShower.Instance.hasSetTimer))
                    {
                       // TimeShower.Instance.CountdownShow(serverTimeUtc.ToString("HH:mm:ss"));

                        //TimeShower.Instance.CountdownShow(serverTimeUtc.ToString("HH:mm:ss")); //This was working
                        TimeShower.Instance.CountdownShowDateTime(serverTimeUtc);
                    }
                   
                }
            }

            if (OnResponse != null)
                OnResponse.Invoke(response);
        }
    }

    public IEnumerator SendPostRequestAsync(string endpoint, WWWForm form)
    {
        string uri = ServerURL + endpoint;

        using (UnityWebRequest request = UnityWebRequest.Post(uri, form))
        {
            Debug.Log("Sending Post Form Request:" + uri);

            yield return request.SendWebRequest();

            WebResponse<T> response = new WebResponse<T>();

            if (request.result == (UnityWebRequest.Result.ProtocolError | UnityWebRequest.Result.ConnectionError | UnityWebRequest.Result.DataProcessingError))
            {
                response.message = request.error;
                Debug.Log("Error:" + request.error);
            }
            else
            {
                Debug.Log("API URL:" + endpoint);
                Debug.Log("Response:" + request.downloadHandler.text);

                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    response = JsonUtility.FromJson<WebResponse<T>>(request.downloadHandler.text);
                }
            }

            if (OnResponse != null)
                OnResponse.Invoke(response);
        }
    }

    public IEnumerator SendRequestAsync(string endpoint, string type, string token, string data = "")
    {
        string uri = ServerURL + endpoint;

        Debug.Log("Access Token : " + token);
        Debug.Log("uri : " + uri);


        using (UnityWebRequest request = new UnityWebRequest(uri, type))
        {

            if (data != "")
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(data);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("Sendign Request:" + uri);
            Debug.Log("payload:" + data);

            yield return request.SendWebRequest();

            WebResponse<T> response = new WebResponse<T>();
            string responseData;

            if (request.result == (UnityWebRequest.Result.ProtocolError | UnityWebRequest.Result.ConnectionError | UnityWebRequest.Result.DataProcessingError))
            {
                response.message = request.error;
                Debug.Log("Error:" + request.error);
            }
            else
            {
                Debug.Log("API URL:" + endpoint);
                Debug.Log("Response:" + request.downloadHandler.text);

                if(endpoint == "/shop/get_inventory_list/")
                {
                    string tempItem = request.downloadHandler.text;
                    tempItem = "{\"items\": " + request.downloadHandler.text + " }";
                    PlayerPrefs.SetString("ITEM_INVENTORY", tempItem);        
                }

                if (request.downloadHandler.text.StartsWith("["))
                {
                    // responseData = "Detail: " + 
                    responseData = "{\"data\": {\"data\": " + request.downloadHandler.text + "} }";
                    Debug.Log(responseData);
                }
                else
                {
                    responseData = request.downloadHandler.text;
                }

                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    response = JsonUtility.FromJson<WebResponse<T>>(responseData);
                }
            }

            if (OnResponse != null)
            {
                OnResponse.Invoke(response);
                Debug.Log("On response called");



            }

        }
    }
}

[Serializable]
public class WebResponse<T>
{
    [SerializeField] public int status;
    [SerializeField] public string message;
    [SerializeField] public T data;

    [SerializeField] public int id;
    [SerializeField] public string name;
    [SerializeField] public string detail;
    // [SerializeField] public detail detail;
    [SerializeField] public int gold;
    [SerializeField] public string url;
    [SerializeField] public int job_id;
    [SerializeField] public string access_token;
    [SerializeField] public string refresh_token;
    public WebResponse()
    {
        status = 0;
        message = "We are sorry, something went wrong, please try again later...";
    }
}
[Serializable]
public class detail
{
    public Dictionary<string, int> loc;
    public string msg;
    public string type;
}