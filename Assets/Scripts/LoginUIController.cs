using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoginUIController : MonoBehaviour
{
    [Header("Login Panel")]
    public TMP_InputField emailLogin;
    public TMP_InputField passwordLogin;

    [Header("Register Panel")]
    public TMP_InputField usernameRegister;
    public TMP_InputField emailRegister;
    public TMP_InputField passwordRegister;

    [Header("Change Account Panel")]
    public TMP_InputField emailToChange;

    [Header("Change Password Panel")]
    public TMP_InputField oldPwd;
    public TMP_InputField newPwd;


    public void Start()
    {
        //Invoke(nameof(CheckAlreadyLoggged), 0.1f);

        Invoke(nameof(LoginAuto), .1f);
    }

    public void CheckAlreadyLoggged()
    {
       string accesstoken= PlayerPrefs.GetString("UserAccessToken", "");

        if (!string.IsNullOrEmpty(accesstoken))
        {
            UIController.Instance.UserLoggedInUI();

        }
    }
    public void LoginAuto()
    {
        string username = PlayerPrefs.GetString("UserEmail", "");
        string password = PlayerPrefs.GetString("UserPassword", "");

        if((!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)))
        {
            LoginManager.Instance.Login(username, password);
        }
        else
        {
            Debug.Log("Will need to login manually");
        }

    }
    public void Login()
    {
        LoginManager.Instance.Login(emailLogin.text, passwordLogin.text);

        PlayerPrefs.SetString("UserEmail", emailLogin.text);
        PlayerPrefs.SetString("UserPassword", passwordLogin.text);
    }

    public void Register()
    {
        LoginManager.Instance.Register(usernameRegister.text, emailRegister.text, passwordRegister.text);
    }

    public void Logout()
    {
        LoginManager.Instance.Logout();

        
    }

    public void ChangePassword()
    {
        string email = PlayerPrefs.GetString("Email","");
        LoginManager.Instance.ChangePassword(email, oldPwd.text, newPwd.text);
    }

    public void ChangeAccount()
    {
        LoginManager.Instance.ChangeAccount();
    }
    private void OnDisable()
    {
        PlayerPrefs.Save();
    }
    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
