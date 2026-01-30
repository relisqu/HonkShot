# Fire System Implementation Plan

## Goal
Implement a comprehensive fire system for GooseFireSystem.cs with:
- Fire scaled 0-100
- Multiple fire gain sources: dash, damage, kills, environment interactions
- Environment difficulty levels affecting fire gain
- Continuous interaction support with delays for certain elements
- Item modifier support

## Phases

### Phase 1: Core GooseFireSystem Refactor `[complete]`
- [x] Add fire gain modifier system for items
- [x] Add dash fire gain (+5 default)
- [x] Add damage dealt fire gain (damage/5)
- [x] Add enemy kill fire gain (+20)
- [x] Subscribe to ScoreManager events

### Phase 2: Environment Interaction System `[complete]`
- [x] Create IFireInteractable interface with difficulty level
- [x] Create FireInteractionType enum (Bounce, Continuous)
- [x] Add interaction delay support for continuous elements
- [x] Fire gain formula: 1-10 based on difficulty

### Phase 3: Update Environment Objects `[complete]`
- [x] Add fire interaction to BounceObject
- [x] Add fire interaction to MoveRoadMovement (continuous with delay)

### Phase 4: Item Integration `[complete]`
- [x] Create FireGainBuffItem example

## Key Files Modified
- `Scripts/Player/GooseFireSystem.cs` - Main fire system with new formula
- `Scripts/Player/IFireInteractable.cs` - Interface for environment objects
- `Scripts/LevelSystem/LevelObjects/BounceObject.cs` - Bounce fire interaction
- `Scripts/LevelObjects/MoveRoads/MoveRoadMovement.cs` - Continuous fire interaction
- `Scripts/Items/PermanentItems/FireGainBuffItem.cs` - Example item for fire boost

## Implementation Summary

### Fire Gain Sources:
1. **Dash**: +5 points (configurable via `_fireGainPerDash`)
2. **Damage Dealt**: damage / 5 (configurable via `_damageToFireDivisor`)
3. **Enemy Kill**: +20 points (configurable via `_fireGainPerKill`)
4. **Environment Interaction**: 1-10 points based on difficulty level (1-10)

### Environment Difficulty Formula:
`fireGain = Lerp(minGain, maxGain, (difficultyLevel - 1) / (maxDifficulty - 1))`

### Continuous Interaction Cooldown:
- MoveRoad and similar objects have cooldown to prevent spam
- Default cooldown: 0.5 seconds (configurable per object)
- Tracked via Dictionary<instanceId, remainingCooldown>

### Item Modifier System:
- `FireGainModifierSystem` on GooseFireSystem allows items to modify fire gain
- Supports Add and Mult modifiers
- FireGainBuffItem demonstrates usage pattern
