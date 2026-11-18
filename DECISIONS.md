# Assignment 11 — Data and Saves

## Existing game and scope
Sakura is a 2D platform game with movement, jumping, shooting, patrol enemies, falling hazards, hearts and a final boss. The existing campaign is MainMenu -> Level1 -> Level2 -> BossFight. These scenes, art, mobile controls and win/lose panels are reused. The separate Assignment10 trial and old assignment scene are not campaign levels and are not included in this build.

## Level data
Three LevelData ScriptableObjects are saved in Assets/Assignment11/Data:
- Level1Data.asset -> Level1
- Level2Data.asset -> Level2
- Level3Data.asset -> BossFight

Each gameplay scene has an Assignment11 - Level and Saves GameObject with a LevelContext component and a direct asset reference. Its early Awake selects the data before actors initialize. Typed LevelBalance.Apply methods read that asset in actor Awake, including newly spawned projectiles and hazards. Data includes player movement, jump, health, invulnerability, shooting, enemy health and movement, damage, score rewards, hazard timing and boss attacks. Geometry, input thresholds, animation visuals and UI sizes remain technical or presentation settings.

The first level has one-hit enemies moving at 1.5 units/second. The second has two-hit enemies moving at 2 units/second. The final level reuses the existing eight-health boss with a faster second attack phase. Hazard intervals are 3, 2.5 and 2 seconds where a spawner is present. Player jump and movement are retained to preserve platform reachability. No extra combat mechanics are introduced. Inactive legacy scenes retain their serialized values when no LevelContext is present.

The old Enemy.TakeDamage implementation discarded the supplied damage and always used one. It now respects configured bullet damage.

## JSON progress
SaveSystem uses JsonUtility and stores sakura-progress.json under Application.persistentDataPath. Schema version 1 stores highest unlocked level, current level, three completed flags, three best scores and master volume. Indices are zero-based. PlayerPrefs is not used.

The existing company/product identity is retained. On this Windows installation the production file is:
C:/Users/Дария/AppData/LocalLow/DefaultCompany/My project (2)/sakura-progress.json

Missing, empty, malformed, incomplete, incompatible and inconsistent data recover to defaults. Defaults unlock only Level 1, clear completed flags and scores, and use full volume. Validation rejects invalid array lengths, negative scores, impossible progression and invalid volume. Writes use a temporary file and atomic replacement. Storage exceptions are caught and reported; gameplay can continue in memory.

## Automatic saving
Saving occurs on successful level completion, changing volume, in-game pause, application pause, loss of focus and quit. Choosing a level also saves the current level. Failed attempts do not unlock levels. Best scores never decrease. The normal Play button resumes the saved current unlocked level.

## Interface and reset
The existing MainMenu canvas and card are reused. Existing gameplay HUD and controls remain intact. Level Select, Settings and Reset Confirmation panels are saved in the scene, with serialized references and persistent button listeners. Buttons use soft pink, panels pale pink, and text dark rose. One master-volume slider controls existing audio through AudioListener.volume. The existing SafeArea component now uses the Game View dimensions on desktop and valid device safe bounds on mobile; visual testing found and corrected an off-screen desktop menu.

Reset Progress opens an in-game confirmation. Cancel only closes the panel. Yes, Reset restores progression, best scores and volume, writes JSON and immediately refreshes menu controls. Reset is available from the main menu settings, so no active level needs to be interrupted.

## Verification and limitations
The automated Unity Play Mode harness uses an isolated Assignment11Tests/progress.json file. It checks missing and corrupt files, validation, settings, reset/cancel, locked levels, each scene's LevelData, a changed data value, movement, spawned bullets, pause/focus saves and existing portal/boss completion paths. Combat victories are simulated through the actual damage/completion code; this is not a claim of a full manual playthrough.
See Logs/Assignment11-tests.txt and Logs/Assignment11-restart.txt for observed results.

## Android and video
The four campaign scenes are enabled in build settings. Tools > Sakura > Build Assignment 11 APK builds Builds/Android/Sakura-Assignment11.apk once Android Build Support, SDK/NDK Tools and OpenJDK are installed for Unity 6000.6.2f1. This installation currently lacks Android Build Support, so no APK is claimed.
A screen-recording checklist is provided in DEMONSTRATION.md. No video is claimed unless a recording is supplied separately.
