using System;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Assets.Scripts.LevelCreator
{
    public class Door : MonoBehaviour
    {
        [FormerlySerializedAs("DoorAnimator")] [SerializeField]
        private Animator _doorAnimator;

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
        }

        public void Close()
        {
            isCurrentlyOpened = false;
        }

        public void GoThroughDoor(PlayerMovement player)
        {
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