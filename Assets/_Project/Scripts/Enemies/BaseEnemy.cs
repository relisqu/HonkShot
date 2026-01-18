using UnityEngine;

namespace Scripts.Enemies
{
    public abstract class BaseEnemy : MonoBehaviour
    {
        [Header("Enemy Identification")]
        [SerializeField] private int _enemyId = 0;
        [SerializeField] private string _enemyName = "";

        [Header("Score Settings")]
        [SerializeField] [Range(1, 10)] private int _difficultyRating = 1;

        public int EnemyId => _enemyId;
        public string EnemyName => _enemyName;
        public int DifficultyRating => _difficultyRating;
        
        public void SetEnemyId(int id)
        {
            _enemyId = id;
        }
        
        public void SetEnemyName(string name)
        {
            _enemyName = name;
        }

        public void SetDifficultyRating(int rating)
        {
            _difficultyRating = Mathf.Clamp(rating, 1, 10);
        }
    }
}
