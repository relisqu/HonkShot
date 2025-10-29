using System;
using Scripts.LevelSystem.LevelGeneration;
using Scripts.Player.Dash;
using Scripts.PointSystem;
using UnityEngine;

namespace Scripts.Player.Interactions
{
    public class ResetHandler : MonoBehaviour
    {
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private PlayerDashController _playerDashController;
        [SerializeField] private PointReceiver _pointReceiver;

        private void ResetStats()
        {
            _playerDashController.ResetDashes();
            _pointReceiver.ResetAttack();
            _playerMovement.GetRigidbody().linearVelocity = Vector3.zero;
        }

        private void LevelManager_EnteredRoom()
        {
            ResetStats();
        }

        private void PlayerMovement_DragStarted()
        {
            _pointReceiver.AddMaxPoints();
            _pointReceiver.ResetAttack();
        }

        private void LevelManager_CompletedRoom()
        {
            _pointReceiver.AddMaxPoints();
            _pointReceiver.ResetAttack();
            _playerDashController.ResetDashes();
        }

        private void Start()
        {
            _levelManager.EnteredRoom += LevelManager_EnteredRoom;
            _levelManager.CompletedRoom += LevelManager_CompletedRoom;
            _playerMovement.DragStarted += PlayerMovement_DragStarted;
        }

        private void OnDestroy()
        {
            _levelManager.EnteredRoom -= LevelManager_EnteredRoom;
            _levelManager.CompletedRoom -= LevelManager_CompletedRoom;
            _playerMovement.DragStarted -= PlayerMovement_DragStarted;
        }
    }
}