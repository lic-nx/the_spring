# Minimal Sprite Moss

Single-pass unlit moss overlay for `SpriteRenderer` in the URP 2D Renderer.

## Use

1. Assign `MinimalSpriteMoss.mat` to a SpriteRenderer.
2. Keep the SpriteRenderer sprite and color/tint as usual; `_MainTex` is supplied by the renderer.
3. Duplicate the material, or set `_Seed` with a `MaterialPropertyBlock`, to vary the contour between blocks.

The defaults target a thin moss cap over roughly the top 10–25% of an upright sprite. The shader preserves the original sprite alpha and therefore does not expand its silhouette.

## Cost

- one sprite sample;
- one 64 x 64 repeatable noise sample;
- one transparent `Universal2D` pass;
- no loops, blur, normal maps, displacement, procedural noise, or lighting.

Android ETC1 external alpha is supported only in the corresponding shader variant and adds its required alpha sample there.

The vertical mask uses the sprite UV `y` axis. The project's current block textures are individual, upright sprites, which matches this setup.
