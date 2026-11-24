# Assignment 12 — submission and recording

## Upload checklist
- [ ] GitHub repository URL. Push the actual new commits yourself; the agent does not push.
- [ ] This week's real commit links. Use `git log --since="2026-11-23" --oneline`; do not alter dates.
- [ ] README.md with the new sound section and audio credits.
- [ ] DECISIONS.md, with your own review of the marked entry.
- [ ] APK, or the accepted short video recorded from your iPhone.
- [ ] Real AudioMixer screenshot showing Master, Music, SFX and Paused. The additional PauseFX bus is intentional.
- [ ] Separate 2–3 minute MP4 defense, spoken in English with your own voice and actual game sound.
- [ ] Check that all uploaded links open for the teacher.

## Before recording
1. Open Sakura in Unity 6000.6.2f1, then Assets/Scenes/MainMenu.unity.
2. Open Window > Audio > Audio Mixer, select SakuraAudioMixer, and show the All view. Keep Inspector available for PauseFX > Lowpass.
3. Prepare ASSIGNMENT12_DEFENSE.md and the actual C# files in the code editor. Show code from this project, not a separate demo.
4. Use headphones or reduce speaker volume so game sound does not mask your voice. Make a short test recording and listen to it.
5. Keep the player's real save. If you need a fresh demonstration, back it up first; do not press Reset by accident.
6. Prepare real recorded gameplay segments for a heart pickup, enemy death, player death/defeat and victory. Edit those real segments into the defense if a full playthrough does not fit three minutes. Do not use simulated evidence or claim automated victories are your playthrough.
7. On iPhone, record the actual installed game if available. Installing a native iOS build needs a Mac with Xcode, Apple signing and a physical device. An APK cannot run on iPhone. If filming the computer screen with an iPhone is the intended allowed alternative, confirm that interpretation with your teacher; do not label it an iPhone build.

## Suggested 2:50 sequence and spoken English
| Time | What to show | What to say |
|---|---|---|
| 0:00–0:15 | MainMenu running; click Settings and Back, then Play. Let the crossfade be heard. | “This is Sakura, my mobile platform game. I added a complete sound pass to the existing project. The menu has calm music. When I start a level, the game music fades in over one second. Both tracks are original sounds generated for this project.” |
| 0:15–0:45 | Jump, shoot, land, take damage, collect a heart and kill an enemy. Leave short gaps so each sound is audible. | “Here you can hear the jump, shot, landing, damage, pickup and enemy death. The sounds start in the gameplay scripts at the correct action. Repeated effects use a small random pitch change. Each active effect uses its own source, so overlapping sounds do not change each other.” |
| 0:45–1:05 | Approach a contact enemy; move away after warning. Show the boss warning and projectile. Optionally use real trial-enemy clips. | “The enemy gives an audio warning before the attack. The contact enemy and boss give at least zero point four five seconds to react. I can move away before contact damage. The three enemies from Assignment Ten use their existing preparation states. A warning plays once when a state begins.” |
| 1:05–1:20 | Real victory and player death/defeat clips. | “These are the victory and defeat sounds. The player death sound comes before the defeat result. The game manager prevents a result from playing many times.” |
| 1:20–1:40 | Pause for two seconds, resume, return to Settings and move Music/SFX independently. | “Pause makes the music quieter and softer. It does not stop the track. Resume restores the normal sound. Music and sound effects have separate sliders. Master controls both. These controls also use the existing touch interface.” |
| 1:40–2:00 | Close the actual application and reopen it. Show the saved slider positions. If using Editor Play Mode instead, label it honestly; it is not a phone restart. | “The settings are stored in the same JSON file as my progress. After closing and reopening the game, the values are still here. Old saves receive default audio values, and unlocked levels and scores stay safe.” |
| 2:00–2:25 | Unity AudioMixer: Master, Music, SFX, PauseFX; click Normal and Paused; inspect Lowpass. | “This is the real mixer. Music and SFX are children of Master. PauseFX is a separate music stage. Its Paused snapshot lowers the volume by ten decibels and sets the low-pass filter to eleven hundred hertz. The user volume is stored on a different stage.” |
| 2:25–2:50 | Defense diagram, SakuraAudio.Crossfade, SaveSystem and SakuraVolumeSettings. | “This map shows the script communication. Gameplay sends sound events to SakuraAudio. The sliders send values to SaveSystem, which updates the mixer and JSON file. The formula is twenty times the logarithm of the slider value. Zero becomes minus eighty decibels. Crossfading uses unscaled time, so it also works during pause.” |

The mandatory eight events are jump, damage, pickup, enemy warning, death, UI click, victory and defeat. Check all eight are actually audible in the exported MP4. Keep pauses for game audio; shorten spoken text if needed. Do not read claims about device testing that you have not performed.

## Exact windows and paths
- Mixer: Assets/Audio/Mixers/SakuraAudioMixer.mixer, opened in Window > Audio > Audio Mixer.
- Music and effects: Assets/Audio/Music and Assets/Audio/SFX.
- Settings: MainMenu > SETTINGS, Canvas/SettingsPanel in the scene.
- Service prefab: Assets/Resources/SakuraAudio.prefab.
- Runtime code: Assets/Scripts/Audio; gameplay event calls remain in Assets/Scripts and Assets/Assignment10/Runtime/TrialEnemy.cs.
- Save code: Assets/Assignment11/SaveSystem.cs.
- Desktop save: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/My project (2)/sakura-progress.json`.
- Build command: Tools > Sakura > Build Assignment 12 APK.
- Build output: Builds/Android/Sakura-Assignment12.apk, if the build succeeds.
- Automated test command: Tools > Sakura > Test Assignment 12 Audio.

## Personal review
Listen to the music style and the effect balance yourself. The synthesized instruments suggest plucked strings, flute-like tones and percussion; they are not recordings of traditional instruments. Confirm they match your intended Sakura atmosphere. Also verify every enemy on the physical device, touch sliders, restart persistence and audio through the phone speaker before marking the submission complete.

Current APK status: no working Assignment 12 APK. The real attempts failed (non-ASCII path first; Java/Gradle loopback error on the short-path retry, plus unresolved Player script warnings). See ASSIGNMENT12_REPORT_RU.md. Do not upload an older APK as Assignment 12 evidence.
