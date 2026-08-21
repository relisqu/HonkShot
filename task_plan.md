# Task Plan: Aim-Zoom Feature (punchy overshoot on throw release)

## Goal
Finish the "goose aim-zoom" camera feel. Live zoom-out while aiming already exists.
Remaining gap = punchy overshoot snap-back on real throw release; keep clean ease on cancel.

## Key Discovery (2026-08-08)
The live aim-zoom is ALREADY IMPLEMENTED in `PlayerSwapAnimationHandler.cs`, not missing:
- `HandleCameraZoom()` (line 110): while Swapping, reads live drag, normalizes vs
  `PlayerBallMovement.MaxForceMagnitude`, `targetSize = _originalSize + forceScale * _maxZoomOut`,
  tweens `Lens.OrthographicSize` with `Ease.OutQuad`, `_zoomDuration = 0.1f`.
- `PlayerMovement_DragStarted()` captures `_originalSize`.
- `PlayerMovement_DragFinished` (throw) AND `PlayerMovement_InputCancelled` (cancel)
  BOTH call the same `ResetCameraZoom()` → plain `Ease.OutQuad` back to `_originalSize`. NO overshoot,
  and no differentiation between throw and cancel.

=> Building the separate `CameraAimZoom` component (old plan) would DUPLICATE and fight this over the
   same `Lens.OrthographicSize`. Correct approach: extend `PlayerSwapAnimationHandler` instead.

## Remaining Design Gap (from settled design)
- On real throw release: **punchy overshoot** snap back to base size (Ease.OutBack / punch).
- On cancel: clean ease back, no overshoot (current behavior — keep).

## Current Phase
FINAL approach: bypass pixel-perfect during aim → real smooth zoom + punchy overshoot. Compiles clean.
Needs USER playtest (MCP play sessions don't tick frames, so no automated runtime verification possible).

## Final Implementation (bypass pixel-perfect during aim)
Root cause proven: URP PixelPerfectCamera (UpscaleRenderTexture) hard-locks rendered ortho to 10.38461;
Cinemachine lens is ignored. Direct Camera.orthographicSize writes hold only when BOTH the URP
PixelPerfectCamera AND the CinemachineBrain are disabled.
- On DragStarted: disable brain + urp-pixel-perfect + cm-pixel-perfect, capture crisp base.
- During aim: drive `_outputCamera.orthographicSize` directly (zoom-out scales with drag power).
- On throw: OutBack overshoot snap back (the punch); on cancel: clean OutQuad. Re-enable all on complete.
- Tweens SetUpdate(true); Update() safety-net prevents stuck-disabled state.
- Refs `_urpPixelPerfect`(Main Camera)+`_cmPixelPerfect`(vcam) wired via MCP; scene saved+restored.
- Tradeoff (user-accepted): pixel-perfect momentarily OFF + camera position frozen during the brief aim.

## Phases

### Phase 1: Confirm approach
- [x] Confirmed by user: extend `PlayerSwapAnimationHandler`, no new component
- **Status:** complete

### Phase 2: Ortho overshoot (ABANDONED)
- [x] Implemented OutBack overshoot on throw release
- [x] User reported "no effect". Root cause: URP PixelPerfectCamera + CinemachinePixelPerfect
      (UpscaleRenderTexture, pixelRatio=4) quantize the lens ortho size; a sub-unit overshoot near
      base doesn't cross a snap step → invisible. (Zoom-OUT still reads smooth because it spans many
      fine steps.) Serialized field values were verified correct, so not a defaults bug.
- **Status:** abandoned (see findings.md)

### Phase 3: Shake punch on throw (IMPLEMENTED)
- [x] Reverted overshoot; `ResetCameraZoom()` back to single clean OutQuad for throw + cancel
- [x] Added serialized `_throwShakeDuration=0.15`, `_throwShakeStrength=3` ("Throw Release Punch")
- [x] `PlayerMovement_DragFinished` calls `CameraShakeHandler.Instance.ShakeCameraOutsideOfQueue(...)`
      (throw only; cancel gets no punch)
- [x] Recompiled, 0 console errors; instance fields verified 0.15 / 3
- **Status:** complete

### Phase 4: Tuning (user-side)
- [ ] Play-mode confirm the punch reads well; tune `_throwShakeStrength` / `_throwShakeDuration` on player
- **Status:** pending (designer tuning only)

## Decisions Made
| Decision | Rationale |
|----------|-----------|
| Extend existing handler, no new component | Avoids two systems fighting over Lens.OrthographicSize |

## Errors Encountered
| Error | Attempt | Resolution |
|-------|---------|------------|
| (none) | — | — |
