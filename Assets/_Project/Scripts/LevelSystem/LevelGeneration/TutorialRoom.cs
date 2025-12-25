using Scripts.UI;
using UnityEngine;

namespace Scripts.LevelSystem.LevelGeneration
{
    public class TutorialRoom : MonoBehaviour
    {
        [SerializeField] private Room _room;
        [SerializeField] private PopupWindow _startPopupWindow;

        public Room Room => _room;

        public void ShowLevelStart()
        {
            _startPopupWindow.Show();
        }
    }
}