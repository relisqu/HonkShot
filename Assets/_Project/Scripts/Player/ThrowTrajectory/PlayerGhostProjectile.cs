using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerGhostProjectile : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private BouncingObject _bouncingObject;

        public Rigidbody2D Rigidbody2D => _rigidbody2D;
        public BouncingObject BouncingObject => _bouncingObject;


    }
}