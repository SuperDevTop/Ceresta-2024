using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Events;
using System.Globalization;

public class LoginManager : MonoBehaviour
{
    static LoginManager instance;
    public static LoginManager Instance => instance;

    string usernameToLogin;
    public List< JobItem> allJobItems;

    public ScrollViewManager jobViewManager;


    public void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.name = GetType().Name;
    }
    public void Register(string username, string email, string password)
    {
        if (email.Length == 0 || password.Length == 0 || username.Length==0)
        {
            if (UIController.Instance != null)
            {
               // UIController.Instance.UserLoggedInUI();
                UIController.Instance.GiveNotification("Username, email and password are required");
                return;
            }
        }
        WebRequestHandler<RegisterResponse> request = new WebRequestHandler<RegisterResponse>(OnRegisteredResponse);
        string url = "/user/register/";

        string data = JsonUtility.ToJson(new RegisterRequest(username, email, password));
        StartCoroutine(request.SendRequestAsync(url, "POST", data));

        UIController.Instance.loadingSpinner.SetActive(true);
    }

    private void OnRegisteredResponse(WebResponse<RegisterResponse> response)
    {
        UIController.Instance.loadingSpinner.SetActive(false);

        /*if (response.status == 1)
        {
            Debug.Log("Have registered succ");
          
        }
        else
        {
            Debug.Log("Register issue:" + response.message);

        }*/
        Debug.Log(response.message);
        if (response.message.Contains("successfully"))
        {
            if (UIController.Instance != null)
            {
                UIController.Instance.UserRegistered();
            }
            UIController.Instance.GiveNotification(response.message);
            Debug.Log("Giving response.message");

        }
        else
        {
            /*if ((response.detail.msg != null)&&(response.detail.msg.Length>0))
            {
                UIController.Instance.GiveNotification(response.detail.msg);
                Debug.Log("Giving response.detail.msg");
            }
            else
            {
                UIController.Instance.GiveNotification(response.detail.ToString());
                Debug.Log("Giving response.detail");
            }*/
            
            UIController.Instance.NotifyText.text = response.detail;
            
        }
        
    }

    public void Login(string email, string password)
    {
        if(email.Length==0|| password.Length == 0)
        {
            if (UIController.Instance != null)
            {
               // UIController.Instance.UserLoggedInUI();
                UIController.Instance.GiveNotification("Username and password are required");
                return;
            }
        }
        WebRequestHandler<LoginResponse> request = new WebRequestHandler<LoginResponse>(OnLoginResponse);
        string url = "/user/login/";

        string data = JsonUtility.ToJson(new LoginRequest(email, password));
        StartCoroutine(request.SendRequestAsync(url, "POST", data));

        UIController.Instance.loadingSpinner.SetActive(true);
        PlayerPrefs.SetString("Email", email);
    }

    private void OnLoginResponse(WebResponse<LoginResponse> response)   
    {
        UIController.Instance.loadingSpinner.SetActive(false);
        Debug.Log("Access TOKEN: "+ response.access_token);
        Debug.Log("PLAYER ID: " + response.id);
        Debug.Log("PLAYER NAME: " + response.name);
       // PlayerPrefs.SetString("UserAccessToken", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3MTIzODg4NDcsInN1YiI6IjIzIn0.oJX9nLQScGVmOiyE0ENNNtsTzuRALYE3fAmuVfJ7ReE"); 
        if (response.access_token!=null && response.access_token.Length>0)
        {
            Debug.Log("Access token: "+ response.access_token);
            PlayerPrefs.SetString("UserAccessToken", response.access_token);

            Debug.Log("Refresh token: " + response.refresh_token);
            PlayerPrefs.SetString("UserRefreshToken", response.refresh_token);

            PlayerPrefs.SetInt("UserGold", response.gold);

            PlayerPrefs.SetInt("UserJobId", response.job_id);

            Debug.Log("USER GOLD: "+ response.gold);
            if (UIController.Instance != null)
            {
                UIController.Instance.UserLoggedInUI();
                UIController.Instance.GiveNotification("You have successfully logged in.");

               
            }
            if (ShopManager.Instance != null)
            {
                ShopManager.Instance.GetInventoryList();
            }
        }
        else
        {
            UIController.Instance.GiveNotification(response.detail);
        }
      /*  if (response.status == 1)
        {
            Debug.Log("Have login succ");

            if (UIController.Instance != null)
            {
                UIController.Instance.UserLoggedInUI();
            }
        }
        else
        {
            Debug.Log("Login issue:" + response.message);

        }*/
        Debug.Log(response.message);
        GetJobs();
        // UIController.Instance.GiveNotification(response.message);

    }

    public void Logout()
    {
        UIController.Instance.loadingSpinner.SetActive(true);
        WebRequestHandler<LogoutResponse> request = new WebRequestHandler<LogoutResponse>(OnLogoutResponse);
        string url = "/user/logout";
        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");
        string data = JsonUtility.ToJson(new LogoutRequest());
        StartCoroutine(request.SendRequestAsync(url, "POST", accesstoken, data));
    }

    private void OnLogoutResponse(WebResponse<LogoutResponse> response)
    {
        UIController.Instance.loadingSpinner.SetActive(false);
        PlayerPrefs.DeleteAll();

        UIController.Instance.UserLoggedOutUI();
       /* if (response.status == 1)
        {
            Debug.Log("Have logged out succ");

        }
        else
        {
            Debug.Log("Logout issue:" + response.message);

        }
        Debug.Log(response);*/
       if (response.message.Contains("Successfully"))
        {
            //Logged out successfully
            UIController.Instance.GiveNotification(response.message);

        }
        else
        {
            UIController.Instance.GiveNotification(response.detail);

        }
        
    }

    public void GetUsers()
    {
        WebRequestHandler<GetUsersDataResponse> request = new WebRequestHandler<GetUsersDataResponse>(GetUsersDataResponse);
        string url = "/user/getusers";

        StartCoroutine(request.SendRequestAsync(url, "GET", ""));
    }
    private void GetUsersDataResponse(WebResponse<GetUsersDataResponse> response)
    {
        if (response.status == 1)
        {
            Debug.Log("Got users data.");

        }
        else
        {
            Debug.Log("Getting Users Data issue:" + response.message);
        }

        Debug.Log(response);
        UIController.Instance.GiveNotification(response.message);
    }

    public void ChangeAccount()
    {

        WebRequestHandler<ChangeAccountResponse> request = new WebRequestHandler<ChangeAccountResponse>(OnAccountChangedResponse);
        string url = "/user/change_account/";

        string data = JsonUtility.ToJson(new ChangeAccountRequest());
        StartCoroutine(request.SendRequestAsync(url, "POST", data));
    }

    private void OnAccountChangedResponse(WebResponse<ChangeAccountResponse> response)
    {
        if (response.status == 1)
        {
            Debug.Log("Account Changed succ");

        }
        else
        {
            Debug.Log("Account changing issue:" + response.message);

        }
        Debug.Log(response);
        UIController.Instance.GiveNotification(response.message);
    }

    public void ChangePassword(string email, string oldPassword,string newPassword)
    {
        if (email.Length == 0 || oldPassword.Length == 0 || newPassword.Length == 0)
        {
            if (UIController.Instance != null)
            {
              //  UIController.Instance.UserLoggedInUI();
                UIController.Instance.GiveNotification("All fields required");
                return;
            }
        }

        WebRequestHandler<PasswordChangeResponse> request = new WebRequestHandler<PasswordChangeResponse>(OnPasswordChangedResponse);
        string url = "/user/change_password/";

        string accesstoken = PlayerPrefs.GetString("UserAccessToken","");
        string data = JsonUtility.ToJson(new PasswordChangeRequest(email, oldPassword, newPassword));
        StartCoroutine(request.SendRequestAsync(url, "POST", accesstoken, data));
    }

    private void OnPasswordChangedResponse(WebResponse<PasswordChangeResponse> response)
    {
     /*   if (response.status == 1)
        {
            Debug.Log("Have changed password succ");

        }
        else
        {
            Debug.Log("Password changing issue:" + response.message);

        }
        UIController.Instance.GiveNotification(response.message);*/

        if (response.message.Contains("successfully"))
        {
            UIController.Instance.GiveNotification(response.message);
        }
        else
        {
            UIController.Instance.GiveNotification(response.detail);
        }
       
    }

    public void GetGold()
    {
        WebRequestHandler<GetNicknameResponse> request = new WebRequestHandler<GetNicknameResponse>(GetGoldResponse);
        string url = "/user/get_gold/";

        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");

        StartCoroutine(request.SendRequestAsync(url, "GET", accesstoken, ""));
    }
    private void GetGoldResponse(WebResponse<GetNicknameResponse> response)
    {
        if (response.detail != null)
        {
            if (response.detail.Contains("Not authenticated"))
            {
                UIController.Instance.GiveNotification(response.detail);
            }
        }
        else
        {
            PlayerPrefs.SetInt("UserGold", response.gold);
            Debug.Log("Update the UI Gold");
            UIController.Instance.UpdateGoldText();  
        }

      

        Debug.Log(response);
    }

    public void GetNickname()
    {
        WebRequestHandler<GetNicknameResponse> request = new WebRequestHandler<GetNicknameResponse>(GetNicknameResponse);
        string url = "/user/get_nickname/";

        StartCoroutine(request.SendRequestAsync(url, "GET", ""));
    }
    private void GetNicknameResponse(WebResponse<GetNicknameResponse> response)
    {
        if (response.status == 1)
        {
            Debug.Log("Got nickname.");

        }
        else
        {
            Debug.Log("Getting nickname issue:" + response.message);
        }

        Debug.Log(response);
    }
    public void GetJobs()
    {
        WebRequestHandler<GetJobsResponse> request = new WebRequestHandler<GetJobsResponse>(GetJobResponse);
        string url = "/user/get_jobs/";
        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");
        StartCoroutine(request.SendRequestAsync(url, "GET", accesstoken, ""));
    }
    private void GetJobResponse(WebResponse<GetJobsResponse> response)
    {
        /* if (response.status == 1)
         {
             Debug.Log("Got nickname.");

         }
         else
         {
             Debug.Log("Getting nickname issue:" + response.message);
         }*/
        Debug.Log(response);
        Debug.Log(response.data.ToString());
        print(response.data);
        allJobItems= new List<JobItem>();
        foreach (var job in response.data.data)
        {
            Debug.Log(job.id+ ": " + job.name);
            allJobItems.Add(job);
        }
        Debug.Log(response.message);
        Debug.Log(response.detail);
        Debug.Log(response);

        UIController.Instance.UpdateProfileImageLogin();

        jobViewManager.LoadJobList();
    }

    public void SetJob(int jobID)
    {
        WebRequestHandler<SetJobsResponse> request = new WebRequestHandler<SetJobsResponse>(SetJobResponse);
        string url = "/user/set_job/";
        string accesstoken = PlayerPrefs.GetString("UserAccessToken", "");

        string data = JsonUtility.ToJson(new SetJobRequest(jobID));
        StartCoroutine(request.SendRequestAsync(url, "POST", accesstoken, data));

        PlayerPrefs.SetInt("UserJobId", jobID);
    }
    private void SetJobResponse(WebResponse<SetJobsResponse> response)
    {
        /* if (response.status == 1)
         {
             Debug.Log("Got nickname.");

         }
         else
         {
             Debug.Log("Getting nickname issue:" + response.message);

         }*/
        if (response.message.Contains("successfully"))
        {
            UIController.Instance.GiveNotification(response.message);
        }
        else
        {
            UIController.Instance.GiveNotification(response.detail); 
        }

        UIController.Instance.UpdateProfileImageJobChange();

        Debug.Log(response);
        Debug.Log(response.message);
        Debug.Log(response.detail);
    }
}

[Serializable]
public class RegisterRequest
{
    [SerializeField]
    public string username;
    [SerializeField]
    public string email;
    [SerializeField]
    public string password;

    public RegisterRequest(string username, string email, string password)
    {
        this.username = username;
        this.email = email;
        this.password = password;
    }
}

[Serializable]
public class RegisterResponse
{

    [SerializeField]
    public Detail detail;

   /* [SerializeField]
    public string loc;
    [SerializeField]
    public string msg;
    [SerializeField]
    public string type;*/

}

[Serializable]
public class LoginRequest
{
    [SerializeField]
    public string email;
    [SerializeField]
    public string password;

    public LoginRequest(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}


[Serializable]
public class LoginResponse
{
    //Success response
    [SerializeField]
    public string access_token;
    [SerializeField]
    public string refresh_token;


    //Validation Error
    /*[SerializeField]
    public Detail detail;*/
   /* public string loc;
    [SerializeField]
    public string msg;
    [SerializeField]
    public string type;*/

}
[Serializable]
public class Detail
{
   
    [SerializeField]
    public Dictionary<string,int> loc;
    [SerializeField]
    public string msg;
    [SerializeField]
    public string type;

 
}

[Serializable]
public class LogoutRequest
{
    [SerializeField]
    public string username;

    public LogoutRequest()
    {
        //this.username = username;
    }
}
[Serializable]
public class LogoutResponse
{
    //Success response
    [SerializeField]
    public Detail detail;

}

[Serializable]
public class GetUsersDataResponse
{
    //Success response
    [SerializeField]
    public string message;

}

[Serializable]
public class PasswordChangeRequest
{
    [SerializeField]
    public string email;
    [SerializeField]
    public string old_password;
    [SerializeField]
    public string new_password;

    public PasswordChangeRequest(string email, string oldPassword, string new_password)
    {
        this.email = email;
        this.old_password = oldPassword;
        this.new_password = new_password;
    }
}
[Serializable]
public class SetJobRequest
{
    [SerializeField]
    public int job_id;

    public SetJobRequest( int jobID)
    {
        this.job_id = jobID;
    }
}
[Serializable]
public class PasswordChangeResponse
{
    //Success response
    [SerializeField]
    public string access_token;
    [SerializeField]
    public string refresh_token;

    //Validation Error
    [SerializeField]
    public string loc;
    [SerializeField]
    public string msg;
    [SerializeField]
    public string type;

}

[Serializable]
public class ChangeAccountRequest
{
  

    public ChangeAccountRequest()
    {
        
    }
}

[Serializable]
public class ChangeAccountResponse
{
    //Success response
    [SerializeField]
    public string message;
   

}
[Serializable]
public class GetGoldResponse
{
    //Success response
    [SerializeField]
    public string message;

}
[Serializable]
public class GetNicknameResponse
{
    //Success response
    [SerializeField]
    public string message;

}
[Serializable]
public class GetJobsResponse
{
    //Success response
    [SerializeField]
    public string message;
    [SerializeField]
    public List<JobItem> data;

}
[Serializable]
public class SetJobsResponse
{
    //Success response
    [SerializeField]
    public string message;

}
[Serializable]
public class JobItem
{
    //Success response
    [SerializeField]
    public string name;
    [SerializeField]
    public int id;
    [SerializeField]
    public string description;
    [SerializeField]
    public int speed;
    [SerializeField]
    public int allow_gold;
    [SerializeField]
    public List<items> items;
}

[Serializable]
public class items
{
    [SerializeField]
    public int id;
    [SerializeField]
    public string name;
    [SerializeField]
    public string description;
    [SerializeField]
    public int price;
    [SerializeField]
    public string hp;
    [SerializeField]
    public string sp;
    [SerializeField]
    public string img_path;
   
}