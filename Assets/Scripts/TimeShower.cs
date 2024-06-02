using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Networking;

public class TimeShower : MonoBehaviour
{
    public string timeFormat = "HH:mm";
    public int remainSeconds;

    public float totalHours = 0f; // Total hours for the countdown
    public float totalMinutes = 10f; // Total minutes for the countdown
    public float totalSeconds = 10f; 

    public bool hasSetTimer;
    static TimeShower instance;

    public static TimeShower Instance => instance;

    private void OnEnable()
    {
        // StartCoroutine(UpdateTime());
        // StartCoroutine(GetServerTime());
        Invoke(nameof(GetBackendTimeRepeat), 0.5f);
    }
    private void Awake()
    {

        if (instance != null)
            Destroy(gameObject);
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.name = GetType().Name;
        }
        hasSetTimer = false;
    }

    /* private IEnumerator UpdateTime()
     {
         while (true)
         {
             DateTime currentTime = DateTime.Now;
             string formattedTime = currentTime.ToString(timeFormat);
             UIController.Instance.timeText.text = formattedTime;

             yield return new WaitForSeconds(60f);
         }
     }
 */
  
    public void CountdownShow(string time)
    {
        string[] parts = time.Split(':');

        totalHours = int.Parse(parts[0]);
        totalMinutes= int.Parse(parts[1]);
        Debug.Log(totalHours);
        Debug.Log(totalMinutes);


        //GET LOCAL TIME
        DateTime currentTime = DateTime.Now;
        string formattedTime = currentTime.ToString("HH:mm:ss");
        string[] partsLocal = formattedTime.Split(':');
        int localHours = int.Parse(partsLocal[0]); 
        int localMinute = int.Parse(partsLocal[1]);
        int localSeconds = int.Parse(partsLocal[2]);

        
        totalHours = Mathf.Abs(localMinute - int.Parse(parts[0]));
        totalMinutes = Mathf.Abs(localSeconds - int.Parse(parts[1]));

        Debug.Log($"start_time API TIME: {time} |  Local Time: {formattedTime} |  Countdown time: {totalHours}: {totalMinutes}");


        if (!hasSetTimer)
        {
            StartCoroutine(StartCountdown());
            hasSetTimer = true;
        }
        
    }
    public void CountdownShowDateTime(DateTime utcTime)
    {


        //GET LOCAL TIME
        DateTime currentTime = DateTime.Now;

        TimeSpan difference = currentTime - utcTime;

        Debug.Log($"start_time API TIME: {utcTime} |  Local Time: {currentTime}");
        Debug.Log("COUNTDOWN TIME: " + difference.Hours + ":" + difference.Minutes + ":" + difference.Seconds);

        totalHours= difference.Hours;
        totalMinutes= difference.Minutes;
        totalSeconds = difference.Seconds;
      //  Debug.Log($"start_time API TIME: {time} |  Local Time: {formattedTime} |  Countdown time: {totalHours}: {totalMinutes}");


        if (!hasSetTimer)
        {
            StartCoroutine(StartCountdown());
            hasSetTimer = true;
        }

    }
    private IEnumerator StartCountdown()
    {
        
        this.gameObject.transform.parent = transform.parent.parent;
        float remainingTime = totalHours * 3600 + totalMinutes * 60 + totalSeconds; // Convert total hours and minutes to seconds
        //float remainingTime = totalHours * 60 + totalMinutes ;
        while (remainingTime > 0f)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
             UIController.Instance.timeText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);

            //UIController.Instance.timeText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        UIController.Instance.timeText.text = "00:00";

        Debug.Log("Countdown finished!");
    }

    private IEnumerator GetBackendTime()
    {
        MazeManager.Instance.GetStartTime();
        yield return new WaitForSeconds(10f);
        GetBackendTimeRepeat();
      
    }
    public void GetBackendTimeRepeat()
    {
        MazeManager.Instance.GetStartTime();
        //InvokeRepeating(nameof(GetBackendTime), 1f - Time.time % 1f, 1f);
      //  StartCoroutine(GetBackendTime());
    }

    private void Start()
    {
       // StartCoroutine(GetServerTime());
    }

    private IEnumerator GetServerTime()
    {
       // UnityWebRequest www = UnityWebRequest.Get("http://worldclockapi.com/api/json/utc/now"); // Example API for server time in UTC
        UnityWebRequest www = UnityWebRequest.Get("https://worldtimeapi.org/api/timezone/utc"); // Example API for server time in UTC
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string jsonText = www.downloadHandler.text;
            ServerTimeData serverTimeData = JsonUtility.FromJson<ServerTimeData>(jsonText);

            string[] parts = serverTimeData.datetime.Split('T');
            string[] shortParts=  parts[1].Split('.');

            Debug.Log(parts[1]);
            Debug.Log(shortParts[0]);
            DateTime serverTimeUtc = DateTime.Parse(shortParts[0]);
            

            UIController.Instance.timeText.text = serverTimeUtc.ToString("HH:mm");
        }
        else
        {
            Debug.LogError("Failed to fetch server time: " + www.error);
        }
    }

    [Serializable]
    private class ServerTimeData
    {
        public string datetime;
    }
}
