using System;
using System.Linq;
using Scripts.LevelSystem.LevelGeneration;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class LevelsCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private LevelGenerator _levelGenerator;

        private void Start()
        {
            LevelManager.Instance.CompletedRoom += LevelManager_CompletedRoom;
        //    _text.SetText(
          //      $"Пройдено комнат: 0 из {_levelGenerator.Rooms.Count}");
        }

        private void OnDestroy()
        {
            LevelManager.Instance.CompletedRoom -= LevelManager_CompletedRoom;
        }


        private void LevelManager_CompletedRoom()
        {
           // _text.SetText(
          ///      $"Пройдено комнат: {_levelGenerator.Rooms.Count(room => room.IsCleared())} из {_levelGenerator.Rooms.Count}");
        }
    }
}