using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using Facebook.Unity;
using System;
using UnityEngine.UI;

public class FacebookManager : MonoBehaviour
{
    public TextMeshProUGUI FB_userName;
    public RawImage rawImg;

    #region Initialize
    // Awake function from Unity's MonoBehavior
    void Awake()
    {
        FB.Init(SetInit, OnHideUnity);

        if (!FB.IsInitialized)
        {
            FB.Init(() =>
            {
                if (FB.IsInitialized)
                    FB.ActivateApp();
                else
                    print("Couldn't initialize");
            },
            isGameShown =>
            {
                if (!isGameShown)
                    Time.timeScale = 0;
                else
                    Time.timeScale = 1;
            });
        }
        else
            FB.ActivateApp();
    }

    void SetInit()
    {
        if (FB.IsLoggedIn)
        {
            Debug.Log("Facebook is logged in");
            string s = "client token" + FB.ClientToken + "User Id" + AccessToken.CurrentAccessToken.UserId + "token string" + AccessToken.CurrentAccessToken.TokenString;
        }
        else
        {
            print("Facebook is not logged in");
        }
        DealWithFbMenus(FB.IsLoggedIn);
    }
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    void DealWithFbMenus(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            FB.API("/me?fields=first_name", HttpMethod.GET, DisplayUsername);
            
        }
        else
        {
            print("Not logged in");

        }
    }
    void DisplayUsername(IResult result)
    {
        if(result.Error!= null)
        {
            Debug.Log(result.Error);
        }
        else
        {
            string name = "" + result.ResultDictionary["first_name"];
            if (FB_userName != null)
            {
                FB_userName.text = name;
                Debug.Log("Got facebook name: " + name);
            }
            

        }
    }
    #endregion

  
    public void Facebook_Login()
    {
        List<string> permissions= new List<string>();
        permissions.Add("public_profile");
        FB.LogInWithReadPermissions(permissions, AuthCallBack);
    }
    void AuthCallBack(IResult result)
    {
        if (FB.IsLoggedIn)
        {
            SetInit();
            //AccessToken class will have session details
            var aToken = AccessToken.CurrentAccessToken;

            print(aToken.UserId);

            foreach (string perm in aToken.Permissions)
            {
                print(perm);
            }
        }
        else
        {
            print("Failed to log in");
        }

    }
    public void Facebook_LogOut()
    {
        StartCoroutine(LogOut());
    }
    IEnumerator LogOut()
    {
        FB.LogOut();
        while (FB.IsLoggedIn)
        {
            print("Logging Out");
            yield return null;
        }
        print("Logout Successful");
        // if (FB_profilePic != null) FB_profilePic.sprite = null;
        if (FB_userName != null) FB_userName.text = "";
        if (rawImg != null) rawImg.texture = null;
    }

    #region other

    private static void ShareCallback(IShareResult result)
    {
        Debug.Log("ShareCallback");
        SpentCoins(2, "sharelink");
        if (result.Error != null)
        {
            Debug.LogError(result.Error);
            return;
        }
        Debug.Log(result.RawResult);
    }
    public static void SpentCoins(int coins, string item)
    {
        var param = new Dictionary<string, object>();
        param[AppEventParameterName.ContentID] = item;
        FB.LogAppEvent(AppEventName.SpentCredits, (float)coins, param);
    }

  

    #endregion
}
