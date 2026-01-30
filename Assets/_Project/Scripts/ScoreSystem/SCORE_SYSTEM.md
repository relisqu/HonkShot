# Score System Implementation

## Overview

A meta-currency point system for the roguelike-pinball game. Points are earned by killing enemies and can be used for future meta-gameplay features.

## Formula

```
Score = (EnemyKill × Speed + DamageDealt/10 + ItemBuffs) × TimeCoefficient
```

| Component | Range | Description |
|-----------|-------|-------------|
| EnemyKill | 1-10 | Based on enemy difficulty rating |
| Speed | 1.0-2.0 | Multiplier based on ball velocity at kill |
| DamageDealt | /10 | Killing blow damage divided by 10 |
| ItemBuffs | Variable | Additive bonus from equipped items |
| TimeCoefficient | 1-1000 | Multiplier based on room clear speed |

## Files Created

| File | Purpose |
|------|---------|
| `ScoreSystem/ScoreData.cs` | Serializable data class for persistence |
| `ScoreSystem/ScoreManager.cs` | Core manager with formula implementation |
| `Items/PermanentItems/ScoreBuffItem.cs` | Item for score bonuses |

## Files Modified

| File | Changes |
|------|---------|
| `Config/EnemyConfigData.cs` | Added `difficultyRating` field (1-10) |
| `Enemies/BaseEnemy.cs` | Added `DifficultyRating` property |
| `Enemies/EnemyHealth.cs` | Integrated score awarding on death |
| `Progress/ProgressData.cs` | Added `ScoreData` for save system |
| `Progress/ProgressSaver.cs` | Added score management methods |

## Setup Instructions

### 1. Add ScoreManager to Scene
Add `ScoreManager` component to a persistent GameObject (e.g., GameManager).

### 2. Configure Enemy Difficulty
On each enemy prefab's `BaseEnemy` component, set `Difficulty Rating`:
- 1-2: Easy enemies (basic walkers)
- 3-4: Medium enemies (shooters)
- 5-7: Hard enemies (maniacs, chasers)
- 8-10: Bosses

### 3. Optional: Tune ScoreManager Settings
In inspector:
- `Min/Max Speed Threshold`: Ball speed range for multiplier
- `Optimal Room Clear Time`: Target time for max time coefficient
- `Max Room Clear Time`: Time after which coefficient reaches minimum

## API Reference

### ScoreManager

```csharp
// Singleton access
ScoreManager.Instance

// Current run score
long score = ScoreManager.Instance.CurrentRunScore;

// Events
ScoreManager.Instance.OnScoreChanged += (long totalScore) => { };
ScoreManager.Instance.OnScoreAwarded += (long awarded, ScoreBreakdown breakdown) => { };

// Manual score addition
ScoreManager.Instance.AddScore(100);

// Reset for new run
ScoreManager.Instance.ResetRunScore();

// Item buff system
ScoreManager.Instance.ScoreBonusModifierSystem.AddModifier(modifier);
```

### ProgressSaver

```csharp
// Add run score to total (call at run end)
progressSaver.AddRunScore(runScore);

// Spend meta-currency
bool success = progressSaver.SpendScore(amount);

// Get total saved score
long total = progressSaver.GetTotalScore();

// Access score data
ScoreData data = progressSaver.ScoreData;
```

### ScoreBreakdown Struct

```csharp
public struct ScoreBreakdown
{
    public int DifficultyPoints;    // Enemy difficulty (1-10)
    public float SpeedMultiplier;   // Speed component (1.0-2.0)
    public float DamagePoints;      // Damage/10
    public float ItemBuffs;         // Item bonuses
    public float TimeCoefficient;   // Time multiplier (1-1000)
    public float FinalScore;        // Calculated result
}
```

## Creating Score Buff Items

1. Create new item prefab
2. Add `ScoreBuffItem` component
3. Configure `ScoreStatModifiers` list:
   - `Add` type: Flat bonus points per kill
   - `Mult` type: Percentage multiplier

Example: +5 bonus points per kill
```
Type: Add
Value: 5
Order: 0
```

## Future Considerations

- UI display for current score during gameplay
- End-of-run score summary screen
- Meta shop using `ProgressSaver.SpendScore()`
- Leaderboards using `ScoreData.HighestRunScore`
- Achievements based on `ScoreData.TotalEnemiesKilled`
