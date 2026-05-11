# QWEN.md – Project Overview

## Project Type
This is a **Unity** game project. The `Assets/scripts` folder contains dozens of C# MonoBehaviour scripts that implement gameplay mechanics, UI handling, audio control, and level management.

## Directory Overview
```
Assets/scripts/
├─ AnimationControl.cs            # Controls animation state machines
├─ button_*.cs                    # UI button behaviours (next, pause, push sound)
├─ Button_sound_controller.cs    # Base class providing `PlayMusicWithDelay`
├─ CirclePatrol2D.cs              # Simple 2‑D circular patrol AI
├─ Crush_block.cs                 # Logic for destructible blocks
├─ dont_destroy_music.cs          # Persists music across scenes
├─ DragBlock2D.cs                 # Drag‑and‑drop block handling
├─ enemy.cs                       # Enemy behaviour placeholder
├─ EventControllerScr.cs          # Central event dispatcher
├─ FloatingAnim.cs                # Floating animation helper
├─ flower.cs                      # Flower object script
├─ Go_to_main_menu.cs             # Returns to main menu scene
├─ image_controler.cs             # Handles UI image swaps
├─ Level_display.cs               # Shows level number UI
├─ level_text.cs                  # Text component for level UI
├─ LevelMenu.cs                   # Level‑selection menu logic
├─ Line_rendered.cs               # Rendering helper for line graphics
├─ LocalizedText.cs               # Simple localisation wrapper
├─ Mainmenu.cs                    # Main menu UI, scene navigation, quit handling
├─ moverPlatform.cs               # Platform movement script
├─ music_controller.cs            # Global music manager (singleton)
├─ player_move.cs                 # Player movement controller
├─ RandomizeAnim.cs               # Random animation selector
├─ restart_button.cs              # Restart current level button
├─ RotationAnim.cs                # Rotates objects over time
├─ SceneSetings.cs                # Scene configuration data holder
├─ server_saves.cs                # Yandex Games save integration
├─ Shaking_block.cs               # Shaking block effect
├─ SimpleButterfly.cs             # Simple butterfly animation
├─ soundEffectController.cs       # Plays one‑shot SFX
├─ swipe_menu.cs                  # Swipe‑based menu handling
├─ trigger_checker.cs             # Trigger event helper
├─ tutorial.cs                    # Tutorial flow controller
├─ TutorialBlock.cs               # Individual tutorial step object
├─ UI_PopUp.cs                    # Pop‑up UI manager
├─ UIController.cs                # General UI controller
├─ WalkPingPong2D.cs              # Ping‑pong movement back‑and‑forth
├─ WorldSpaceCanvasAspectScaler.cs# Adjusts world‑space canvas aspect
└─ *.meta                         # Unity meta files (generated automatically)
```

## Key Concepts
- **Inheritance** – Most UI scripts inherit from `Button_sound_controller`, which provides a helper `PlayMusicWithDelay` for playing a sound clip before executing a callback.
- **Yandex Games (YG) Integration** – Several scripts (`Mainmenu.cs`, `server_saves.cs`) reference `YG2.saves` to read/write persistent data.
- **Scene Management** – `Mainmenu.cs` and `LevelMenu.cs` use `SceneManager.LoadScene` and `SceneUtility.GetBuildIndexByScenePath` to navigate between levels and tutorial scenes.
- **Audio Persistence** – `dont_destroy_music.cs` makes the music `AudioSource` survive scene loads via `DontDestroyOnLoad`.
- **Localization** – `LocalizedText.cs` provides a simple lookup for UI strings, currently used for Russian messages in the code.

## Building & Running
1. **Open the Project** – Launch **Unity Hub**, click **Add**, and select the root folder `/mnt/c/Users/Chesterh/Desktop/the_spring`.
2. **Resolve Packages** – Unity will automatically import the required packages. If any custom packages are referenced (e.g., Yandex Games SDK), ensure they are present in `Packages/manifest.json`.
3. **Play Mode** – Press the **Play** button in the Unity Editor to run the game directly.
4. **Build** – Open **File → Build Settings**, add all scenes listed in `Assets/Scenes/` (or as referenced by the scripts), choose the target platform, and click **Build**.

> **TODO:** Document any command‑line build steps (e.g., `Unity -batchmode -projectPath …`) if the project is built outside the editor.

## Development Conventions
- **Naming** – Scripts follow PascalCase for class names and snake_case for public fields when Unity serialization is required (e.g., `public AudioClip soundClip`).
- **Meta Files** – Unity `.meta` files are version‑controlled alongside source files; never delete them manually.
- **Audio Clips** – UI sound clips are assigned via the Inspector and referenced in code via public `AudioClip` fields.
- **Event Handling** – UI button `OnClick` events are wired to public methods like `PlayGame`, `ChooseLevel`, and `ExitGame`.
- **Testing** – No automated test framework is included. Manual testing is performed by entering Play Mode and interacting with UI elements.

## Suggested Next Steps
- Add a **README.md** at the project root describing overall game concept, controls, and any external SDK requirements.
- Consider integrating Unity Test Framework for unit/PlayMode tests.
- Document the Yandex Games save flow and any required SDK initialization.

---
*This QWEN.md file was generated automatically to give future interactions a quick reference to the Unity scripts in `Assets/scripts`.*