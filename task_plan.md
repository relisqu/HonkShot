# Task Plan — Pause menu + Settings (sound, language) + Restart

## Goal
Esc (keyboard) / Start (gamepad) opens a pause menu with a dim overlay in GameplayScene.
Buttons: Resume, Settings, Restart (new run). Settings: Master / Music / Effects sliders and a
language selector. Use the project's own localization (ILocalizationService + RU.txt/EN.txt).
Settings persist via ES3.

## User decisions (2026-11-09)
- 3 sliders: Master / Music / Effects
- Languages: RU + EN for now, more later -> data-driven list (LanguagesConfigSO)
- Restart = new run (reload active scene)
- On pause: music + game sounds stop, UI ("system") sounds keep playing; slider change plays a
  preview sound at roughly the new volume
- Pause allowed everywhere (also over popups / item selection)
- No builds / Play Mode by Claude — user tests. Only console compile check.
- User is "not sure" about the design — try it, expect iteration.

## Phases
| # | Phase | Status |
|---|-------|--------|
| 1 | Input: `Pause` action in PlayerControls.inputactions (Esc, Gamepad Start) | complete |
| 2 | Settings: SettingsData + SettingsService (ES3 key "settings"), LanguagesConfigSO | complete |
| 3 | ProjectMonoInstaller: bind settings, drop hardcoded "Ru" | complete |
| 4 | AudioManager: category volumes, UI channel ignores listener pause; TimeManager.SetMenuPaused | complete |
| 5 | UI scripts: PauseMenuView, SettingsMenuView, VolumeSliderView, LanguageSelectorView | complete |
| 6 | Localization keys in RU.txt / EN.txt | complete |
| 7 | (MCP) Create LanguagesConfig asset (RU default; EN with empty SystemLanguages until translated), PauseMenu prefab, wire into GameplayScene | complete |
| 8 | Console compile check, hand-off test list | complete (0 errors) — awaiting user playtest |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| CS0246 SettingsService not found — Unity skipped SettingsService.cs in compile list | 1 | touch + force ImportAsset didn't help; deleted and recreated via MCP create_script -> compiled |
