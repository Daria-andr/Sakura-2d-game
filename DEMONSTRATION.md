# Sakura Assignment 11 — demonstration checklist

Open the existing project:
C:/Users/Дария/Desktop/Unity Projects/sakura
Use Unity 6000.6.2f1 and Assets/Scenes/MainMenu.unity.

## Record a 4–6 minute demonstration
Use Windows Snipping Tool's video mode or another installed recorder. Select the Unity Game view or editor window, press Record, then follow the sequence below. Stop the recording and save it as Sakura-Assignment11-demo.mp4. No recorder was controlled or video produced by this implementation.

1. Show Assets/Assignment11/Data and select each LevelData asset. Explain: “Each existing level has its own ScriptableObject. These fields control real gameplay.”
2. Select the Assignment11 - Level and Saves object in each gameplay scene to show the assigned asset. BossFight uses Level3Data.
3. In MainMenu press Play. Open Level Select: initially only Level 1 is available. Open Settings and change volume. Show that sakura-progress.json changed.
4. Play Level 1: A/D or arrow keys to move, Space to jump, F to shoot. Mobile movement, jump and shoot buttons also work. Defeat enemies and enter the open portal. Show the completion panel and score.
5. Return to the menu. Show Level 1 completed, best score recorded and Level 2 unlocked. Continue through Level 2 and the existing boss to demonstrate all three levels.
6. Pause during gameplay. Show JSON written after pause. In the editor, switching focus also triggers saving.
7. Stop Play Mode and start again. Show the same unlocked levels, scores and volume. The automated restart report also verifies a completely separate Unity process.
8. Show Settings -> Reset Progress. Press Cancel and demonstrate that progress remains. Open it again and press Yes, Reset. Show that only Level 1 is unlocked, scores are cleared and volume is restored.
9. With Play Mode stopped, keep a copy of the production save. Rename sakura-progress.json to sakura-progress-demo-backup.json. Start Play Mode and show defaults without a crash. Stop, replace the generated file text with “broken JSON”, restart and show recovery. Restore your backed-up save when done.
10. To prove configurable gameplay, stop Play Mode, note Level1Data.playerMovement.moveSpeed, change it to another positive value, enter Level 1 and show changed movement. Stop and restore the original value (5). Never change jump values while demonstrating platform reachability.

Save location on this machine:
C:/Users/Дария/AppData/LocalLow/DefaultCompany/My project (2)/sakura-progress.json

## Automated verification
Logs/Assignment11-tests.txt records Play Mode assertions. Logs/Assignment11-restart.txt records the separate-process reload check. The test harness is editor-only and does not ship in Android builds. These tests simulate victories; they do not replace a manual full gameplay recording.

## APK blocker and exact remaining action
Unity 6000.6.2f1 has no Android Build Support module. In Unity Hub, find that installed version, choose Add modules, and install Android Build Support with Android SDK & NDK Tools and OpenJDK. Reopen Sakura and choose Tools > Sakura > Build Assignment 11 APK. Expected output: Builds/Android/Sakura-Assignment11.apk. Run it on an Android device and repeat the touch-control and persistence checks. Android compilation/device execution remain unverified until then.
