using UnityEngine;
using UnityEngine.UI;
using System;

namespace Scripts.UI
{
    public class ItemSelectionUI : MonoBehaviour
    {
         public Button[] itemButtons; // Assign in inspector
         public GameObject panel; // Assign in inspector
       
         private Action<int> _onItemSelected;
         public bool IsActive => _isActive;
         private bool _isActive;
       
         public void Show(Action<int> onItemSelected)
               {
                   _isActive = true;
                   panel.SetActive(true);
                   _onItemSelected = onItemSelected;
                   for (int i = 0; i < itemButtons.Length; i++)
                   {
                       int index = i;
                       itemButtons[i].onClick.RemoveAllListeners();
                       itemButtons[i].onClick.AddListener(() => SelectItem(index));
                   }
               }
       
               private void SelectItem(int index)
               {
                   _isActive = false;
                   panel.SetActive(false);
                   _onItemSelected?.Invoke(index);
               }
    }
} 