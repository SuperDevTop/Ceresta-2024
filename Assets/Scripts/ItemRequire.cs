using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ItemRequire : MonoBehaviour
{
    public items item;

    public TMP_Text item_Text;
    public TMP_Text gold_Text;
    public Image item_Image;

    private string addressableGroup = "Assets/AddressableSprites/Shop&Inventory";
    string itemGroup;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadItem()
    {
        itemGroup = addressableGroup + "/" + item.img_path;

        item_Text.text = item.name;
        gold_Text.text = item.price.ToString();
        LoadImageAsync(item_Image, itemGroup);
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

    }
}
