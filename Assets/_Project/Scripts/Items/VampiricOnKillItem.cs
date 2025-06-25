using UnityEngine;
using Scripts.Health;
using Scripts.LevelSystem.LevelGeneration;
using System.Collections.Generic;

public class VampiricOnKillItem : MonoBehaviour
{
    public float healPercent = 0.05f; // 5% of max health per kill

    private HealthController _health;
    private List<HealthController> _subscribedEnemies = new List<HealthController>();

    void Start()
    {
        _health = GetComponentInParent<HealthController>();
        if (LevelManager.Instance)
        {
            LevelManager.Instance.EnteredRoom += OnRoomEntered;
            OnRoomEntered(); 
        }
    }

    void OnDestroy()
    {
        if (LevelManager.Instance)
        {
            LevelManager.Instance.EnteredRoom -= OnRoomEntered;
        }
        UnsubscribeAll();
    }

    private void OnRoomEntered()
    {
        UnsubscribeAll();
        var room = LevelManager.Instance.CurrentRoom;
        if (!room) return;
        var enemies = room.GetComponentsInChildren<HealthController>();
        foreach (var enemy in enemies)
        {
            if (enemy == _health) continue; // Don't subscribe to player
            enemy.OnDied += () => OnEnemyKilled(enemy);
            _subscribedEnemies.Add(enemy);
        }
    }

    private void UnsubscribeAll()
    {
        _subscribedEnemies.Clear();
    }

    private void OnEnemyKilled(HealthController enemy)
    {
        if (_health)
        {
            float healAmount = _health.GetMaxHealth() * healPercent;
            _health.AddHealth(healAmount);
        }
    }
} 