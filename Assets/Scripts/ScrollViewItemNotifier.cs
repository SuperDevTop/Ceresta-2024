using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewItemNotifier : MonoBehaviour
{
    public ScrollViewManager scrollViewManager;
    public int item_ID;
    public int quantity;
    // public int index;

    public void OnItemSelected(GameObject item_ID)
    {
        int siblingIndex = transform.GetSiblingIndex();

        // Send the index of this item to scroll view
        scrollViewManager.OnItemClicked(siblingIndex - 1);
    }
}


