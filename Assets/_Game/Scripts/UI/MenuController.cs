//using JetBrains.Annotations;
//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using TMPro;
//using Unity.VisualScripting;
//using UnityEngine;

//public class MenuController : MonoBehaviour
//{
//    [SerializeField] Color higthlighedColor;
//    [SerializeField] GameObject menu;
//    List<TextMeshProUGUI> menuItems;
//    public event Action<int> onMenuSelected;
//    public event Action onBack;

//    int selectedItem =0;
//    private void Awake()
//    {
//        menuItems = menu.GetComponentsInChildren<TextMeshProUGUI>().ToList();
        
//    }
//    public void OpenMenu()
//    {
//        menu.SetActive(true);
//        UpdateItemSelection();
//    }
//    public void CloseMenu()
//    {
//        menu.SetActive(false);
        
//    }
//    public void HandleUpdate()
//    { 
//        int prevSelection=selectedItem;

//        if(Input.GetKeyDown(KeyCode.DownArrow))
//        {
//            ++selectedItem;
//        }
//        else if(Input.GetKeyDown(KeyCode.UpArrow))
//            --selectedItem;
//        selectedItem = Math.Clamp(selectedItem, 0, menuItems.Count - 1);
//        if(prevSelection != selectedItem) 
//            UpdateItemSelection();

//        if(Input.GetKeyDown(KeyCode.Return))
//        {
//           onMenuSelected?.Invoke(selectedItem);
//            CloseMenu();
//        }
//        else if(Input.GetKeyDown(KeyCode.M))
//        {
//            onBack?.Invoke();
//            CloseMenu();
//        }
//    }
//    void UpdateItemSelection()
//    {
//        for(int i = 0; i < menuItems.Count; ++i)
//        {
//            if (i == selectedItem)
//                menuItems[i].color = higthlighedColor;
//            else
//                menuItems[i].color = Color.black;
//        }
//    }
//}
