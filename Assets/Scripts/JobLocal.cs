using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class JobLocal : MonoBehaviour
{
    public TMP_Text jobText;
    public TMP_Text descriptionText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        GetJobs();
    }
    public void ShowCurrentJob()
    {
     //   jobText.text = 
    }
    public void GetJobs()
    {
        LoginManager.Instance.GetJobs();


    }
    public void SetJob()
    {
        PlayerPrefs.SetInt("JOB_ID", UIController.Instance.selectedJobID);
        LoginManager.Instance.SetJob(UIController.Instance.selectedJobID);
       // UIController.Instance.profileImage.sprite = 
    }

}
