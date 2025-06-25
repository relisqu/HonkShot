using UnityEngine;
using Scripts.Health;

public class StickyMineItem : MonoBehaviour
{
    public StickyMine minePrefab; 
    public float explosionDelay = 1f;
    public float explosionDamage = 20f;

    private PlayerAttackController _attack;

    void Start()
    {
        _attack = GetComponentInParent<PlayerAttackController>();
        if (_attack != null)
        {
            _attack.OnHit += AttachMineToEnemy;
        }
    }

    void OnDestroy()
    {
        if (_attack != null)
        {
            _attack.OnHit -= AttachMineToEnemy;
        }
    }

    private void AttachMineToEnemy(GameObject enemy)
    {
        if (enemy == null || minePrefab == null) return;
        var mine = Instantiate(minePrefab, enemy.transform);
        mine.transform.localPosition = Vector3.zero;
        if (mine != null)
        {
            mine.Init(enemy, explosionDelay, explosionDamage);
        }
    }
} 