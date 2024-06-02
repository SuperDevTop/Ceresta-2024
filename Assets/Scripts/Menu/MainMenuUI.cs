using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuUI : MonoBehaviour
{
    //public GameObject gameUI;

    void Start()
    {
        string saveFilePath = Path.Combine(Application.streamingAssetsPath, "PlayerData.json");

        if (Application.platform == RuntimePlatform.Android)
        {
            // Android path differs slightly
            saveFilePath = "jar:file://" + Application.dataPath + "!/assets/" + "PlayerData.json";
        }

        StartCoroutine(LoadJsonFile(saveFilePath));
    }

    
    void Update()
    {
        //gameUI.transform.localScale = new Vector3(Screen.width / 720f, Screen.height / 1440f, 1f);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("GameScene01");
    }

    IEnumerator LoadJsonFile(string filePath)
    {
        // Use Unity's WWW class to load the file
        WWW www = new WWW(filePath);
        yield return www;

        if (string.IsNullOrEmpty(www.error))
        {
            // File loaded successfully
            string jsonData = www.text;

            PlayerPrefs.SetString("INFOS_ROOM1", jsonData);            
        }
        else
        {
            Debug.LogError("Error loading JSON file: " + www.error);
        }
    }
}
