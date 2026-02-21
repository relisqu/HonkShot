# Task Plan: Balatro-Style UI Background Shader

## Goal
Create a URP-compatible HLSL UI shader that produces a Balatro-like animated marbling background with pixelation, consistent across all resolutions and aspect ratios.

## Phases

### Phase 1: Research & Planning [complete]
- [x] Explore existing shader directory structure
- [x] Study Balatro shader reference (Godot port from source)
- [x] Study iquilezles domain warping technique
- [x] Understand URP UI shader requirements

### Phase 2: Implement Shader [in_progress]
- [x] Create `BalatroBackground.shader` in `_Project/Shaders/`
- [x] Implement procedural noise (hash + value noise + fbm)
- [x] Implement domain warping (iquilezles nested fbm)
- [x] Implement spiral + rotation (Balatro atan-based)
- [x] Implement center bias
- [x] Implement pixelation (resolution-independent)
- [x] Implement gradient2D color mapping
- [x] Implement Balatro 3-color fallback
- [x] Add UI stencil/blend support

### Phase 3: Verification [complete]
- [x] Document usage instructions

## Key Decisions
- Use Dave Hoskins hash for reliable cross-platform noise
- 4 octave fbm, 2-level nested domain warping (5 fbm calls total)
- Normalize UVs by screen diagonal for aspect-ratio independence
- `_PixelFilter` controls pixel density relative to screen diagonal
- Gradient2D: X = pattern value, Y = center distance
- Shader keyword `_USE_GRADIENT` toggles gradient texture vs 3-color fallback
