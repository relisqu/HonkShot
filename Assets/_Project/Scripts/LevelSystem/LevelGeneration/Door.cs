using System;
using DG.Tweening;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Assets.Scripts.LevelCreator
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private Transform _doorBlockerTransform;
        [FormerlySerializedAs("Room")] [SerializeField]
        private Room _room;

        public bool IsCurrentlyOpened => isCurrentlyOpened;

        private bool isCurrentlyOpened;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isCurrentlyOpened || !other.gameObject.TryGetComponent(out PlayerMovement player)) return;
            GoThroughDoor(player);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!isCurrentlyOpened || !other.gameObject.TryGetComponent(out PlayerMovement player)) return;
            if (_room == null) return;
            GoThroughDoor(player);
        }

        public void Open()
        {
            isCurrentlyOpened = true;
            _doorBlockerTransform.transform.DOScaleY(0f, 0.4f);
        }

        public void Close()
        {
            isCurrentlyOpened = false;
        }

        public void GoThroughDoor(PlayerMovement player)
        {
            if(_room==null)
            {
                return;
            }
            _room.SetActive();
            player.transform.position = _room.SpawnPointTransform.position;
            LevelManager.Instance.EnterRoom(room: _room);
        }

        public void SetRoom(Room roomInstance)
        {
            _room = roomInstance;
        }
    }
}