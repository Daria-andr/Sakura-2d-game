# Sakura Assignment 11 — delivery report

Implementation is integrated into the existing project:
C:/Users/Дария/Desktop/Unity Projects/sakura

Open Assets/Scenes/MainMenu.unity and press Play. No manual reference or button setup is required.

## Implemented
- Three connected LevelData assets for existing Level1, Level2 and BossFight.
- Typed gameplay configuration applied before actor initialization, including spawned bullets and hazards.
- Progressive combat difficulty using the original mechanics.
- JSON progress, completed/unlocked levels, current level, best scores and master volume.
- Missing/corrupt/incompatible save recovery and safe exception handling.
- Automatic saving at victory, settings changes, pause, focus loss and quit.
- Level Select and Settings added to the original main menu.
- Reset confirmation with working Yes and Cancel behavior.
- Light pink menu panels and readable dark rose text.
- Existing mobile controls, art, enemies, portals and boss reused.
- Fixed Enemy.TakeDamage ignoring bullet damage.
- Fixed desktop SafeArea calculation after visual testing revealed an off-screen menu.

## Existing files modified
Assets/Scripts/: Bomb.cs, BossController.cs, BossProjectile.cs, Bullet.cs, Enemy.cs, GameManager.cs, Hazard.cs, HazardSpawner.cs, HeartPickup.cs, MainMenu.cs, PlayerHealth.cs, PlayerMovement.cs, Rock.cs, SafeArea.cs.
Assets/Scenes/: MainMenu.unity, Level1.unity, Level2.unity, BossFight.unity.
ProjectSettings/EditorBuildSettings.asset.
Unity also expanded the existing dynamic Oswald Bold SDF font data for the added UI text.

## New files
Assets/Assignment11/LevelData.cs, LevelContext.cs, SaveSystem.cs, ProgressMenu.cs.
Assets/Assignment11/Data/Level1Data.asset, Level2Data.asset, Level3Data.asset.
Assets/Assignment11/Editor/Assignment11Setup.cs, Assignment11Build.cs, Assignment11Tests.cs.
Assets/Assignment11/Assignment11SmokeTests.cs (editor-only).
Unity .meta files for these assets/folders.
Project-root DECISIONS.md and DEMONSTRATION.md.
See CHANGED-FILES.txt for file-level changes relative to the backup.

## Scenes and GameObjects
All four campaign scenes have an “Assignment11 - Level and Saves” GameObject with LevelContext.
The three gameplay scenes serialize a reference to the corresponding LevelData asset.
MainMenu reuses Canvas/SafeArea/Card and MainMenuManager. ProgressMenu references saved LevelSelection, SettingsPanel and ResetConfirmation panels, three level buttons, a volume slider and labels. All button/slider actions are wired as persistent listeners.
The build scene order is MainMenu, Level1, Level2, BossFight.

## Verified
Unity 6000.6.2f1 compiled the implementation.
63 automated Play Mode assertions passed, including invalid save recovery, locked-level checks, data-driven actors, movement, spawned bullets, settings, reset/cancel, pause/focus events, all three completion paths and score persistence.
A separate Unity process verified saved progress, best score and volume.
Direct UI testing verified main-menu placement, Level Select, Settings, the Reset Confirmation panel, Cancel, Play, in-game Pause and return to Main Menu.
Screenshots are included.

Automated victories use the actual damage and completion code, with enemies defeated programmatically and portal entry simulated. A full manual playthrough, Android device test and complete recording of every requirement were not performed.

## Save file
C:/Users/Дария/AppData/LocalLow/DefaultCompany/My project (2)/sakura-progress.json

The original company/product identity was preserved. Android uses its own Application.persistentDataPath.
Tests use a separate Assignment11Tests/progress.json file.

## Backup
C:/Users/Дария/Documents/Codex/2026-11-18/files-pasted-by-the-user-unity/work/Sakura-before-Assignment11
This contains Assets, Packages, ProjectSettings and existing root files from before implementation, including uncommitted work. Existing Git history was not rewritten.

## APK and video: remaining work
No APK was produced. Android Build Support is absent from Unity 6000.6.2f1. Install that version's Android Build Support, SDK/NDK Tools and OpenJDK through Unity Hub, then choose Tools > Sakura > Build Assignment 11 APK. The command targets Builds/Android/Sakura-Assignment11.apk and includes all four scenes.
No video was produced. Available computer-use controls supplied screenshots, but no recording was made. DEMONSTRATION.md gives an exact recording sequence and save-recovery demonstration.
These deliverables must not be marked ready until the APK is built/tested and the video is recorded.

## Quick teacher demonstration
1. Show the three assets and scene references; change a movement value and demonstrate its effect.
2. Play a level, finish it, show unlock and best score.
3. Change volume, pause, restart and inspect JSON persistence.
4. Demonstrate Cancel and confirmed Reset.
5. With Play Mode stopped, back up and remove/corrupt the save, then show safe defaults on restart.
6. Show DECISIONS.md and the verification reports.
