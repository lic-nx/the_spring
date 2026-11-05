# File Descriptions - The Spring Game (Unity)

This document provides a detailed description of each script file in the Assets/scripts directory, organized by their purpose and location.

## Root Scripts (Main Gameplay/UI)

### AnimationControl.cs
Manages animation state transitions for game objects/characters.

### ButterflyFlyAway.cs
Handles butterfly behavior when flying away from a trigger/event.

### Button_sound_controller.cs
Base class providing audio functionality (`PlayMusicWithDelay`) for UI buttons inheriting from it.

### CirclePatrol2D.cs
2D circular patrol AI behavior for enemies/objects.

### Crush_block.cs
Handles crushable/destructible blocks, implements IPointerClickHandler for interaction.

### DragBlock2D.cs
Implements drag-and-drop mechanics for blocks in 2D with physics/audio support.

### EventControllerScr.cs
Central event controller, inherits from Button_sound_controller.

### ExpandableMenu.cs
Handles expandable/collapsible menu UI animations and interactions.

### FloatingAnim.cs
Adds floating/bobbing animation to objects.

### FlowerProtection.cs
Protects flowers from damage/destruction under certain conditions.

### Go_to_main_menu.cs
Button to return to main menu scene, inherits from Button_sound_controller.

### LeafPowerup.cs
Implements leaf power-up collectible/mechanic.

### LevelGenerator.cs
Procedurally generates levels for the flower-growing-to-sun mechanic. Places blocks to create a path from flower to sun while flower grows toward triggers with turn angle constraints. Handles solid/breakable/movable block types.

### LevelMenu.cs
Level selection menu logic, loads scenes by level index.

### Level_display.cs
Displays current level number in UI.

### Line_rendered.cs
Renders lines/graphics for visual effects.

### LocalizedText.cs
Simple localization wrapper for displaying translated text (Russian/other languages).

### Mainmenu.cs
Main menu controller - handles scene navigation (PlayGame, ChooseLevel, ExitGame), integrates with Yandex Games SDK (YG2) for saves.

### RandomizeAnim.cs
Randomly starts animations from different points/sequences.

### RotationAnim.cs
Rotates objects continuously over time.

### SceneList.txt
List of scene names/paths used in the game.

### SceneSetings.cs
Holds scene configuration/settings data.

### Shaking_block.cs
Creates shaking/vibration effect on blocks.

### SimpleButterfly.cs
Simple butterfly animation/movement behavior.

### TutorialBlock.cs
Individual tutorial step/block behavior.

### UIController.cs
General UI controller for menus and HUD.

### UI_PopUp.cs
Manages popup UI windows/dialogs.

### WalkPingPong2D.cs
Ping-pong back-and-forth movement for objects/platforms.

### WorldSpaceCanvasAspectScaler.cs
Adjusts world-space canvas to maintain proper aspect ratio.

### button_next.cs
UI button handler for advancing to the next level/scene.

### button_pause.cs
UI button handler for pausing/unpausing the game.

### button_pushsound.cs
UI button that plays a sound effect on press.

### dont_destroy_music.cs
Persists the music AudioSource across scene loads using DontDestroyOnLoad.

### enemy.cs
Enemy behavior/controller script.

### flower.cs
Core flower object behavior for the main gameplay mechanic.

### image_controler.cs
Controls UI image swapping/updates.

### level_text.cs
Text component for level display UI.

### moverPlatform.cs
Moving platform behavior in levels.

### music_controller.cs
Global music manager singleton, persists music across scenes.

### player_move.cs
Player movement controller for 2D gameplay.

### restart_button.cs
Button to restart current level.

### server_saves.cs
Yandex Games SDK integration for cloud saves (reads/writes player progress via YG2.saves).

### soundEffectController.cs
Plays one-shot sound effects.

### swipe_menu.cs
Swipe-based menu navigation handler.

### trigger_checker.cs
Checks for trigger collisions and fires events.

### tutorial.cs
Tutorial flow controller managing tutorial steps.


## Garden System Scripts (`garden/`)

The garden system appears to be a separate gameplay mechanic (gardening/farming) with its own UI, inventory, and drag-and-drop systems.

### garden/ActionButtonObjectPool.cs
Object pool for action buttons in pot menus to improve performance.

### garden/CurrencyManager.cs
Manages in-game currency (coins/money) for the garden shop system.

### garden/DropZoneManager.cs
Manages drop zones for drag-and-drop interactions in the garden.

### garden/Flower.cs
Represents a planted flower in the garden with growth logic.

### garden/FlowerRemovalManager.cs
Handles removing/deleting flowers from pots.

### garden/GameSaveManager.cs
Saves and loads garden game state.

### garden/GlobalClickHandler.cs
Global click handler for garden interactions.

### garden/GrowthConditions.cs
ScriptableObject defining growth conditions for different flower types.

### garden/InputHelper.cs
Helper utilities for input handling in the garden system.

### garden/InventoryManager.cs
Manages player's inventory of seeds/items.

### garden/InventorySlotUI.cs
UI representation of an inventory slot.

### garden/InventoryUIManager.cs
Controls the inventory UI display and interactions.

### garden/PanelSkript.cs
Panel/menu script for garden UI.

### garden/PhysicalActionButton.cs
Physical/world-space action button for pot interactions.

### garden/Pot.cs
Represents a flower pot that can hold planted flowers/seeds.

### garden/PotActionMenu.cs
Action menu for interacting with pots (plant, water, etc.).

### garden/PotActionMenuInputHandler.cs
Handles input for pot action menus.

### garden/PotActionMenuPool.cs
Object pool for pot action menus.

### garden/PotDragManager.cs
Manages dragging pots in the garden.

### garden/SeedDragManager.cs
Manages dragging seeds from inventory to pots.

### garden/SeedGhostUI.cs
Ghost/preview UI shown when dragging seeds.

### garden/SeedItem.cs
ScriptableObject representing a seed item type.

### garden/Shop.cs
Garden shop for buying seeds/items.

### garden/ShopItemUI.cs
UI representation of a shop item.

### garden/Shovel.cs
Shovel tool for removing plants from pots.

### garden/SpriteButton.cs
Sprite-based button implementation.

### garden/SpriteReplacePanel.cs
Panel for replacing sprites/customization.

### garden/SpriteReplacePanelManager.cs
Manages sprite replacement UI panels.

### garden/SunCollector.cs
Collects sun resources in the garden.

### garden/WateringCan.cs
Watering can tool for watering plants.

### garden/WorldDraggable.cs
Base class/component for world-space draggable objects.

### garden/Zone_for_pot.cs
Defines zones where pots can be placed.

### garden/iCardDropArea.cs
Interface/implementation for card drop areas.

### garden/mony_count.cs
Displays and updates money/currency count in UI.


## Other Files

- **SceneList.txt**: Contains list of scene names/paths for the game.
- **garden/tutorial.md**: Tutorial documentation for the garden system.
- **garden/*.prefab**: Unity prefab assets for garden UI elements and objects (Button, Flower, Pot, seeds, menus).
- **garden/*.asset**: ScriptableObject assets (GrowthConditions, SeedItem, test2).
- ***.meta** files: Unity metadata files (auto-generated, should not be manually edited).
