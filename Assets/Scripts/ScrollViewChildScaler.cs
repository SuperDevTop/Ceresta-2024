using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollViewChildScaler : MonoBehaviour
{
    /* public ScrollRect scrollRect;
     public float scaleMultiplier = 1.2f; // Scale multiplier for the selected item
     public float centerOffset = 50f; // Offset from the center to consider an item as "selected"

     private RectTransform[] items;
     private int selectedIndex = -1;

     private void Start()
     {
         // Get all child items
         items = new RectTransform[transform.childCount];
         for (int i = 0; i < transform.childCount; i++)
         {
             items[i] = transform.GetChild(i).GetComponent<RectTransform>();
         }
         scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
     }
     private void OnScrollValueChanged(Vector2 value)
     {
         // Update the scales of the items
         UpdateItemScales();
     }
     private void OnDestroy()
     {
         // Unsubscribe from the onValueChanged event when the script is destroyed
         scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
     }
     private void UpdateItemScales()
     {
         // Find the index of the item closest to the center of the scroll view
         float closestDistance = float.MaxValue;
         int newSelectedIndex = -1;
         for (int i = 0; i < items.Length; i++)
         {
             float distance = Mathf.Abs(GetDistanceFromCenter(items[i]));
             if (distance < closestDistance)
             {
                 closestDistance = distance;
                 newSelectedIndex = i;
             }
         }

         // Scale the selected item and reset the scale of others
         if (newSelectedIndex != selectedIndex)
         {
             selectedIndex = newSelectedIndex;
             for (int i = 0; i < items.Length; i++)
             {
                 items[i].localScale = i == selectedIndex ? Vector3.one * scaleMultiplier : Vector3.one;
             }
         }
     }

     *//*private void Update()
     {
         // Find the index of the item closest to the center of the scroll view
         float closestDistance = float.MaxValue;
         int newSelectedIndex = -1;
         for (int i = 0; i < items.Length; i++)
         {
             float distance = Mathf.Abs(GetDistanceFromCenter(items[i]));
             if (distance < closestDistance)
             {
                 closestDistance = distance;
                 newSelectedIndex = i;
             }
         }

         // Scale the selected item and reset the scale of others
         if (newSelectedIndex != selectedIndex)
         {
             selectedIndex = newSelectedIndex;
             for (int i = 0; i < items.Length; i++)
             {
                 items[i].localScale = i == selectedIndex ? Vector3.one * scaleMultiplier : Vector3.one;
             }
         }
     }*//*

     private float GetDistanceFromCenter(RectTransform item)
     {
         Vector3 itemCenter = item.position + Vector3.up * item.rect.height * 0.5f;
         Vector3 scrollCenter = scrollRect.transform.position;
         return Mathf.Abs(scrollRect.transform.InverseTransformPoint(itemCenter).y - scrollRect.transform.InverseTransformPoint(scrollCenter).y);
     }*/

    public ScrollRect scrollRect;
    public float scaleMultiplier = 1.2f; // Scale multiplier for the selected item
    public float centerOffset = 50f; // Offset from the center to consider an item as "selected"

    private RectTransform[] items;
    private RectTransform viewportRect;

    private void Start()
    {
        // Get the viewport of the scroll view
        viewportRect = scrollRect.viewport;

        // Get all child items
        items = new RectTransform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            items[i] = transform.GetChild(i).GetComponent<RectTransform>();
        }
    }

    private void LateUpdate()
    {
        // Update the scales of the items
        UpdateItemScales();
    }

    private void UpdateItemScales()
    {
        // Get the center position of the viewport
        Vector2 viewportCenter = (Vector2)viewportRect.position + viewportRect.rect.size * 0.5f;

        // Find the index of the item closest to the center of the viewport
        float closestDistance = float.MaxValue;
        int selectedIndex = -1;
        for (int i = 0; i < items.Length; i++)
        {
            Vector3 itemCenter = (Vector2)items[i].position + items[i].rect.size * 0.5f;
            float distance = Vector2.Distance(itemCenter, viewportCenter) ;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                selectedIndex = i;
            }
        }

        // Scale the selected item and reset the scale of others
        for (int i = 0; i < items.Length; i++)
        {
            items[i].localScale = i == selectedIndex ? Vector3.one * scaleMultiplier : Vector3.one;
        }
    }
}
