---
name: seed_to_flower_initialization
description: Initialize flower prefab with first growth-stage sprite from GrowthConditions when seed is dropped
source: auto-skill
extracted_at: '2026-06-09T18:06:34.233Z'
---

## Overview
This skill captures the pattern for creating a flower instance from a seed drop and automatically assigning the initial sprite based on the `GrowthConditions` asset associated with the seed.

### Steps Implemented
1. **Add GrowthConditions reference to `SeedItem`** – a new `public GrowthConditions growthConditions;` field was added so each seed can specify its growth configuration.
2. **Update `SeedDragDrop.OnDropInPot`** – after instantiating the flower prefab, the script now:
   * Parents the flower to the pot.
   * Retrieves the `Flower` component.
   * Calls `flowerComp.Initialize(seedItem.growthConditions);` to pass the configuration.
3. **Expose stage sprites in `GrowthConditions`** – a public read‑only `StageSprites` property was added to provide access to the sprite array.
4. **Set initial sprite in `Flower.Initialize`** – the method now obtains the `SpriteRenderer` on the flower, checks that `StageSprites` contains at least one sprite, and assigns the first sprite as the visual representation of the newly created flower. A warning is logged if sprites are missing.

### Why this approach
* Keeps data-driven configuration: designers can assign different sprite arrays per seed type without code changes.
* Centralises sprite selection logic inside `Flower.Initialize`, ensuring any future creation path (not just drag‑and‑drop) will automatically receive the correct initial visual.
* Adds safety checks and informative warnings to avoid silent failures.

### Reuse
When adding new plant types:
* Create a `GrowthConditions` asset with the desired `stageSprites` array.
* Assign that asset to the corresponding `SeedItem`.
* The existing drag‑and‑drop and flower initialization code will handle the rest.
