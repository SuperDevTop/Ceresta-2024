using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FilterContent : MonoBehaviour
{
    public TMP_InputField searchInputField;
    public Transform scrollViewContent;

    private void Start()
    {
        // Subscribe to the OnValueChanged event of the search input field
        searchInputField.onValueChanged.AddListener(FilterScrollViewContent);
    }

    public void FilterScrollViewContent(string searchQuery)
    {
        string query = searchQuery.ToLower();

        foreach (Transform item in scrollViewContent)
        {
            string itemText = item.GetComponentInChildren<ShopTemplate>().refShopItem.name;

            if (itemText != null)
            {
                string itemString = itemText.ToLower();

                bool isMatch = itemString.Contains(query);

                if (item.transform.GetSiblingIndex() != 0)
                {
                    item.gameObject.SetActive(isMatch);
                }
                
            }
        }
    }

}
