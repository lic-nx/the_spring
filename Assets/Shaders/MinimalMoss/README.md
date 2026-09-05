# Minimal Sprite Moss

Минималистичная зелёная шапка для `SpriteRenderer`.

## Подключение

1. Назначить `MinimalSpriteMoss.mat` в `SpriteRenderer`.
2. Добавить на тот же объект компонент `MinimalMossSpriteParams`.
3. Не создавать отдельный material для каждого объекта: компонент передаёт `_MossLocalMinY`, `_MossLocalMaxY` и `_Seed` через `MaterialPropertyBlock`.

Компонент берёт диапазон высоты из `SpriteRenderer.sprite.bounds`, поэтому маска не зависит от положения спрайта в SpriteAtlas или sprite sheet. `_MainTex`, альфа и `SpriteRenderer.color` продолжают обрабатываться самим SpriteRenderer.

## Debug Mode

- `FullGreen` — весь непрозрачный спрайт ярко-зелёный. Проверяет material, pass, `_MainTex` и альфу.
- `HeightMask` — низ чёрный, верх белый. Проверяет локальную высоту спрайта.
- `Noise` — показывает используемую noise texture.
- `Final` — итоговая зелёная шапка.

Рекомендуемые значения первой рабочей версии:

```text
MossHeight = 0.72
MossSoftness = 0.04
NoiseScale = 4
NoiseStrength = 0.12
MossAmount = 1
```

При `NoiseStrength = 0` граница ровная. При `0.12` noise только деформирует границу; дополнительных edge/crack/hanging/shadow/highlight-масок в шейдере нет.

## Render setup

Основной pass использует:

```text
RenderPipeline = UniversalPipeline
Queue = Transparent
RenderType = Transparent
LightMode = Universal2D
Blend SrcAlpha OneMinusSrcAlpha
ZWrite Off
Cull Off
```

В текущем проекте ссылки `GraphicsSettings` и `QualitySettings` указывают на отсутствующий URP pipeline asset. Поэтому в шейдере оставлен эквивалентный совместимый pass: без него Unity молча показывала `Sprites/Default`, из-за чего material визуально ничего не менял. После восстановления URP asset автоматически используется основной `Universal2D` pass.
