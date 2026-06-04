using System;
using Scripts.Audio;
using Scripts.Items;
using Scripts.Items.StatSystems;
using Scripts.LevelSystem;
using Scripts.LevelSystem.LevelObjects;
using Scripts.Player.Dash;
using Scripts.Player.InputHandling;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player
{
    public class PlayerBallMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerDashController _playerDashController;
        [SerializeField] private GooseFireSystem _gooseFireSystem;
        [SerializeField] private BouncingObject _bouncingObject;

        [Header("Input Handling Parameters")] [Space] [SerializeField]
        private float _forceModifier;

        [FormerlySerializedAs("_maxForce")] [SerializeField]
        private float _maxForceMagnitude;

        [SerializeField] private float _maxDragForceMagnitude = 20f;

        [SerializeField] private float _minBallForce;

        [Header("Ball Movement Parameters")] [Space] [SerializeField]
        private float _additionalDragStartTime = 2000f;


        [SerializeField] private float _additionalDragForce = 1.01f;
        public Action<Collision2D> OnBounced;
        public float MaxForceMagnitude => _maxForceMagnitude;

        private float _throwStartTime;
        private float _prevSpeed;

        private float _defaultDragValue;
        public float CurrentSpeed => _rigidbody2D.linearVelocity.magnitude;
        public Vector2 CurrentMovement => _rigidbody2D.linearVelocity;

        private NumericStatModifierSystem _throwForceModifierSystem = new();
        public NumericStatModifierSystem ThrowForceModifierSystem => _throwForceModifierSystem;

        private NumericStatModifierSystem _dragForceModifierSystem = new();
        public NumericStatModifierSystem DragForceModifierSystem => _dragForceModifierSystem;

        private void Awake()
        {
            if (_gooseFireSystem == null)
                _gooseFireSystem = GetComponent<GooseFireSystem>();
        }

        private void Start()
        {
            _defaultDragValue = _rigidbody2D.linearDamping;
            _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            float currentSpeed = _rigidbody2D.linearVelocity.magnitude;

            _prevSpeed = currentSpeed;

            if (_playerStatus.PlayerState == PlayerState.Ball &&
                Time.time - _throwStartTime > _additionalDragStartTime && !_gooseFireSystem.IsUltimate)
            {
                var additionalDragForce = Mathf.Clamp01(_additionalDragForce * Time.deltaTime);
                _rigidbody2D.linearDamping += additionalDragForce * additionalDragForce;
            }
            else
            {
                _rigidbody2D.linearDamping = _defaultDragValue * _dragForceModifierSystem.Calculate(1f);
            }
        }

        private void InputHandler_OnInputCancelled()
        {
        }

        private void OnEnable()
        {
            _inputHandler.OnDragFinished += InputHandler_OnDragFinished;
            _inputHandler.OnInputCancelled += InputHandler_OnInputCancelled;
        }


        private void OnDisable()
        {
            _inputHandler.OnInputCancelled -= InputHandler_OnInputCancelled;
            _inputHandler.OnDragFinished -= InputHandler_OnDragFinished;
        }

        private void InputHandler_OnDragFinished(Vector2 force)
        {
            if (_playerStatus.PlayerState == PlayerState.Swapping)
            {
                ThrowGoose(force);
                _playerDashController.DecreaseDashCount();
            }
        }

        public void ThrowGoose(Vector2 dragForce)
        {
            ThrowRigidbody(_rigidbody2D, dragForce);
            _playerStatus.SetPlayerState(PlayerState.Ball);
            _throwStartTime = Time.time;
            if (_bouncingObject)
                _bouncingObject.SetCurrentVelocity(_rigidbody2D.linearVelocity);
            if (_gooseFireSystem != null)
                _gooseFireSystem.OnLaunchOrAcceleration();
        }

        public float GetDragVelocityMagnitude(Rigidbody2D rigidbody, Vector2 dragForce)
        {
            var forceMagnitude = Mathf.Min(dragForce.magnitude, _maxForceMagnitude) * _forceModifier;
            var currentVelocity = rigidbody.linearVelocity.magnitude;
            if (currentVelocity + forceMagnitude > _maxDragForceMagnitude)
            {
                forceMagnitude = Mathf.Max(0, _maxDragForceMagnitude - currentVelocity);
            }

            var resultForce = _throwForceModifierSystem.Calculate(forceMagnitude);
            return currentVelocity + resultForce;
        }

        public void ThrowRigidbody(Rigidbody2D rigidbody, Vector2 dragForce)
        {
            var currentVelocity = GetDragVelocityMagnitude(rigidbody, dragForce);
            rigidbody.linearVelocity = -currentVelocity * dragForce.normalized;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_playerStatus.PlayerState != PlayerState.Ball) return;

            if (other.gameObject.TryGetComponent(out LevelSolidObject _))
            {
                var force = _rigidbody2D.linearVelocity.magnitude / _forceModifier;
                force = Mathf.Clamp(force, 0, 1);

                AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "gooseCollision", force);
            }

            OnBounced?.Invoke(other);
        }

        public void StopMovement()
        {
            _rigidbody2D.linearVelocity = Vector2.zero;
        }

        private RigidbodyType2D _preFreezeBodyType;
        private bool _isPhysicsFrozen;
        public bool IsPhysicsFrozen => _isPhysicsFrozen;

        public void FreezePhysics()
        {
            if (_isPhysicsFrozen) return;
            _isPhysicsFrozen = true;
            _preFreezeBodyType = _rigidbody2D.bodyType;
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        }

        public void UnfreezePhysics()
        {
            if (!_isPhysicsFrozen) return;
            _isPhysicsFrozen = false;
            _rigidbody2D.bodyType = _preFreezeBodyType;
            _rigidbody2D.linearVelocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0f;
        }
    }
}