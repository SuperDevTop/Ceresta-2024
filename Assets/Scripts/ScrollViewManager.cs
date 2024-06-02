using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ScrollViewManager : MonoBehaviour
{
    public GameObject buttonTemplate;
    private Transform templateParent;
    public Image jobImage;

   // public Button[] buttons;
    private List<Button> buttonsList = new List<Button>();
    public ScrollRect scrollRect;
    private string addressableGroup = "Assets/AddressableSprites/Jobs";
    private void OnEnable()
    {
        LoadJobList();
    }
    // Method to handle item click event
    public void OnItemClicked(int index)
    {
        // Handle the selected item's index
        Debug.Log("Selected item index: " + index);

        if (UIController.Instance.shopPanel.activeInHierarchy)
        {
            Debug.Log("Shop func:");

        }
        else if(UIController.Instance.inventoryPanel.activeInHierarchy)
        {
            Debug.Log("Inventory func:");
        }

    }

    public void LoadJobList()
    {
        templateParent = buttonTemplate.transform.parent;
        foreach (Transform obj in buttonTemplate.transform.parent)
        {
            if (obj.gameObject.activeInHierarchy)
            {
                Destroy(obj.gameObject);
            }
            Debug.Log(":Des");
            //  Destroy(obj.gameObject);
        }
        if (buttonTemplate != null)
        {
            templateParent = buttonTemplate.transform.parent;

            Debug.Log("::");

            foreach (var item in LoginManager.Instance.allJobItems)
            {
               GameObject obj= Instantiate(buttonTemplate, buttonTemplate.transform.parent);
                obj.GetComponent<JobTemplate>().jobItem= item;
                
                obj.SetActive(true);

                obj.GetComponent<JobTemplate>().LoadImage();

               
                //Check if the current job ID is same
                if (PlayerPrefs.GetInt("UserJobId") == item.id)
                {
                    if (obj.GetComponent<Button>() != null)
                    {
                        buttonsList.Add(obj.GetComponent<Button>());

                        obj.GetComponent<Button>().onClick.Invoke();
                        MoveScrollbarToButton(obj.GetComponent<Button>()); 
                    }
                    
                }
                
                
            }

        }
       
    }
    void MoveScrollbarToButton(Button button)
    {
        // Get the index of the clicked button
        int index = buttonsList.IndexOf(button);

        // Calculate the normalized position of the button in the scroll view
        float normalizedPosition = Mathf.Clamp01((float)index / (buttonsList.Count - 1));

        // Set the vertical scrollbar value to center the button
        scrollRect.verticalScrollbar.value = 1f - normalizedPosition;
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
        foreach (Transform obj in buttonTemplate.transform.parent)
        {
            if (obj.transform.GetSiblingIndex()!=0) 
            {
                Destroy(obj.gameObject);
                Debug.Log(":Des");
            }
           
            //  Destroy(obj.gameObject);
        }
    }

}
