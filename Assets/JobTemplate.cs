using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class JobTemplate : MonoBehaviour
{
    public JobItem jobItem;
    public TMP_Text name;
    public TMP_Text description;
    public Image jobImage;

    public GameObject reqJobItem;
    private string addressableGroup = "Assets/AddressableSprites/Jobs";

    string defaultJobName;
    string defaultJobDescription;

    // Start is called before the first frame update
    void Start()
    {
        defaultJobName = name.text;
        defaultJobDescription = description.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClickJob()
    {
        name.text= jobItem.name;
        description.text= jobItem.description;

        description.text = description.text.ToString() + "\nRequired Gold: " +  jobItem.allow_gold;

        UIController.Instance.selectedJobID = jobItem.id;
        UIController.Instance.selectedJobImage = jobImage;

        foreach (Transform obj in this.transform.parent)
        {
            if (obj.gameObject.activeInHierarchy)
            {
                obj.transform.localScale= Vector3.one;
            }
        }
       // transform.localScale = new Vector3(1.2f,1.2f,1.2f);
        transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
        LoadReqItems();

        //Check if the selected job is the current job
       if (jobItem.id == PlayerPrefs.GetInt("UserJobId", 0))
        {
            UIController.Instance.PlayerCurrentJob();
        }



    }
    void LoadReqItems()
    {
       Transform templateParent = reqJobItem.transform.parent;
        foreach (Transform obj in templateParent)
        {
            if (obj.gameObject.transform.GetSiblingIndex()!=0)
            {
                Destroy(obj.gameObject);
            }
            Debug.Log(":Des");
            //  Destroy(obj.gameObject);
        }
       // Instantiate(reqJobItem);

        if (reqJobItem != null)
        {
            templateParent = reqJobItem.transform.parent;

            Debug.Log("::");

            foreach (var item in jobItem.items)
            {
                GameObject obj = Instantiate(reqJobItem, templateParent);
               // obj.GetComponent<JobTemplate>().jobItem = item;
                obj.GetComponent<ItemRequire>().item = item;
                obj.GetComponent<ItemRequire>().LoadItem();

                obj.SetActive(true);

            }

            CheckPlayerHaveReqItem();
        }
    }
    public void CheckPlayerHaveReqItem()
    {
        //Check if player has enough gold
        int currentGold = PlayerPrefs.GetInt("UserGold");
        if (jobItem.allow_gold > currentGold)
        {
            Debug.Log("Player does not have allowed gold. Returning back.");
            UIController.Instance.PlayerDontHaveReqItemsJob();
            return;
        }

        int countMatch = 0;
         foreach (var invItem in ShopManager.Instance.allInventoryItems)
        {
            foreach (var item in jobItem.items)
            {
               if (item.name == invItem.name)
                {
                    Debug.Log("Match");
                    countMatch++;
                }
            }
        }
         if(countMatch == jobItem.items.Count)
        {
            Debug.Log("Player have the req. items");
            UIController.Instance.PlayerHaveReqItemsJob();
        }
        else
        {
            Debug.Log("Player DOES NOT have the req. items");
            UIController.Instance.PlayerDontHaveReqItemsJob();
        }


    }
    public void LoadImage()
    {
        LoadImageAsync(jobImage, addressableGroup + "/" + jobItem.name.ToLower() + ".png");
    }

    private async void LoadImageAsync(Image image, string addressableKey)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(addressableKey);

        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Sprite sprite = handle.Result;
            image.sprite = sprite;
        }
        else
        {
            Debug.LogError("Failed to load image: " + handle.DebugName);
        }

        // Addressables.Release(handle);
        // handleList.Add(handle);
        UIController.Instance.jobHandeList.Add(handle);
    }
   
    private void OnDisable()
    {
      //  ResetToDefaultText();
    }
    public void ResetToDefaultText()
    {
        name.text = defaultJobName;
        description.text = defaultJobDescription;

        Transform templateParent = reqJobItem.transform.parent;

        foreach (Transform obj in templateParent)
        {
            if (obj.gameObject.transform.GetSiblingIndex() != 0)
            {
                Destroy(obj.gameObject);
            }
           
            
        }

        UIController.Instance.PlayerDontHaveReqItemsJob();
    }


}
