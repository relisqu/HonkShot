using Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Scripts.PointSystem
{
    public class AttackPointsObject : MonoBehaviour, IFireInteractable
    {
        [FormerlySerializedAs("_pointObjectType")] [SerializeField]
        private AttackPointsObjectType attackPointsObjectType;

        private float _stayPoints;

        [SerializeField] private FireInteractionType _interactionType;
        [SerializeField] private int _difficultyLevel;
        [SerializeField] private float _interactionCooldown;

        public FireInteractionType InteractionType => _interactionType;
        public int DifficultyLevel => _difficultyLevel;
        [Tooltip("For interaction with ")] public float InteractionCooldown => _difficultyLevel;

        public void IncreaseStayPoints()
        {
            _stayPoints += _difficultyLevel * Time.deltaTime;
        }

        public void EarnStayPoints()
        {
            if (_difficultyLevel != 0)
            {
                GooseFireSystem.Instance.OnEnvironmentInteraction(this, (int)_stayPoints);
            }

            Debug.Log($"Earned stay points {_stayPoints}");
        }

        public void EarnTouchPoints()
        {
            if (_difficultyLevel != 0)
            {
                GooseFireSystem.Instance.OnEnvironmentInteraction(this, (int)_difficultyLevel);
            }

            Debug.Log($"Earned touch points {_difficultyLevel}");
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            if(InteractionType == FireInteractionType.Manual) return;
            if (collision.gameObject.TryGetComponent(out GooseFireSystem _))
            {
                EarnTouchPoints();
            }
        }

        private float _startInteractTime;

        public void OnTriggerEnter2D(Collider2D other)
        {
            if(InteractionType == FireInteractionType.Manual) return;
            _stayPoints += _difficultyLevel;
            _startInteractTime = Time.time;
            if (other.gameObject.TryGetComponent(out GooseFireSystem _))
            {
                EarnTouchPoints();
            }
        }

        public void OnTriggerStay2D(Collider2D other)
        {
            if(InteractionType == FireInteractionType.Manual) return;
            if (other.TryGetComponent(out GooseFireSystem _))
            {
                if (InteractionType == FireInteractionType.Continuous)
                {
                    if (Time.time - _startInteractTime > _interactionCooldown)
                    {
                        _stayPoints += _difficultyLevel;
                        _startInteractTime = Time.time;
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if(InteractionType == FireInteractionType.Manual) return;
            if (other.TryGetComponent(out GooseFireSystem gooseFireSystem))
            {
                EarnStayPoints();
                _stayPoints = 0;
                _startInteractTime = 0;
            }
        }
    }
}