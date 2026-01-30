using System.Collections;
using UnityEngine;
using Scripts.Health;
using Scripts.Items;
using Scripts.Items.PlayerItemManager;
using Scripts.Items.StatSystems;
using UnityEngine.Serialization;

public class ShieldSpawnerItem : Item
{
    [Header("Shield Spawner Settings")] public float baseCooldown = 10f;
    [Header("Stat Modifier")] public NumericStatModifier CooldownModifier;

    private ShieldController _shieldController;
    private bool _isSpawning = false;
    private string _id;

    private NumericStatModifierSystem _shieldCooldownModifier;

    public NumericStatModifierSystem ShieldCooldownModifier => _shieldCooldownModifier;

    void Start()
    {
        // Get references
        if (_shieldController == null)
        {
            _shieldController = GetComponentInParent<ShieldController>();
        }


        foreach (var item in PlayerInventory.Instance.ownedItems)
        {
            if (item.Id == _id)
            {
                _shieldCooldownModifier.AddModifier(CooldownModifier);
                Destroy(gameObject);
            }
        }


        StartCoroutine(ShieldSpawnRoutine());
    }

    private IEnumerator ShieldSpawnRoutine()
    {
        while (true)
        {
            // Wait for cooldown
            yield return new WaitForSeconds(GetCurrentCooldown());

            // Check if we can spawn a shield
            if (_shieldController != null && _shieldController.Shields.Count < _shieldController.GetMaxShields())
            {
                SpawnShield();
            }
        }
    }

    private void SpawnShield()
    {
        if (_shieldController == null) return;

        _shieldController.AddShield();
        Debug.Log($"Shield spawned! Total shields: {_shieldController.Shields.Count}");
    }

    private float GetCurrentCooldown()
    {
        return _shieldCooldownModifier.Calculate(baseCooldown);
    }


    public override void InitItem(PlayerItemSO playerItemSO)
    {
        _id = playerItemSO.Id;
    }

    public GameObject GameObject => gameObject;
}