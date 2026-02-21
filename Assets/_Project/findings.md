# Findings

## Balatro Background Shader (from source, Godot port)
- Fully procedural, no textures
- Pixelation: `pixel_size = length(screenSize) / pixel_filter` then floor UV to grid
- Spiral: atan2-based angle + uv_len creates vortex distortion
- Organic pattern: 5-iteration sin/cos distortion loop
- Color: 3-color blend weighted by `paint_res` (pattern intensity)
- Key params: spin_rotation_speed, move_speed, contrast, lighting, spin_amount, pixel_filter

## iquilezles Domain Warping
- Replace `f(p)` with `f(p + fbm(p))`
- 2-level nesting: q = fbm pairs, r = fbm(p + 4*q + offsets), final = fbm(p + 4*r)
- Offset vectors (5.2, 1.3), (1.7, 9.2), (8.3, 2.8) break symmetry
- Produces marble/smoke-like organic patterns

## Project Shader Setup
- Shaders in: `Assets/_Project/Shaders/`
- Existing shaders: all .shadergraph (Bloom, Pixelize, Vignette, Fire, Background, etc.)
- URP v12+, Forward rendering, SRP Batcher enabled
- 2D rendering pipeline
