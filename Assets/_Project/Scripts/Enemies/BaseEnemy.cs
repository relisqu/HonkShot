using UnityEngine;

namespace Scripts.Enemies
{
    public abstract class BaseEnemy : MonoBehaviour
    {
        [Header("Enemy Identification")]
        [SerializeField] private int _enemyId = 0;
        [SerializeField] private string _enemyName = "";
        
        public int EnemyId => _enemyId;
        public string EnemyName => _enemyName;
        
        public void SetEnemyId(int id)
        {
            _enemyId = id;
        }
        
        public void SetEnemyName(string name)
        {
            _enemyName = name;
        }
    }
}
