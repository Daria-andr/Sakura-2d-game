
##### Assignment 12 — Sound Pass

#### System
SakuraAudio is one persistent service created from Assets/Resources/SakuraAudio.prefab before a scene loads. It contains the responsibilities of AudioManager and MusicManager in one small class. Two looping music sources use a one-second crossfade based on unscaled time. Reopening gameplay scenes keeps the same track and playback position. A pool of 20 separate effect sources prevents one sound's pitch from changing another sound. If all voices are busy, a new effect is skipped.

#### Events and actual integration
| Event | Trigger |
|---|---|
| Jump / landing | PlayerMovement.Update, ground transitions and accepted jump |
| Projectile launch | PlayerMovement.ShootWithDelay, after projectile creation |
| Damage | PlayerHealth.PlayDamageSound; Enemy.TakeDamage; BossController.TakeDamage; TrialEnemy bullet collision |
| Pickup | HeartPickup.OnTriggerEnter2D, once per heart |
| Enemy warning | Enemy.ContactAttack; BossController.Attack and ContactAttack; TrialEnemy.Enter with telegraph=true |
| Enemy attack | Contact damage resolution, boss projectile release, TrialEnemy attack state entry |
| Death | PlayerHealth.Die; Enemy.Die; BossController.TakeDamage lethal branch; TrialEnemy defeat |
| UI click | SakuraButtonSound on existing Button components |
| Victory / defeat | GameManager.FinishLevel / GameOver, guarded by levelFinished |
| Portal | Portal.OpenPortal, once |

Sakura currently shoots projectiles. No sword or dash mechanic was added. BatPatroller, CrateThrower and CrownCharger keep their existing preparation states in Assets/Assignment10/10Assign.unity. This earlier assignment scene stays outside the four-scene mobile campaign. Their shared TrialEnemy base plays warning and attack sounds once at state entry. Contact enemies now warn 0.45 seconds before checking contact again; escaping prevents damage. Boss projectile preparation is at least 0.45 seconds. Existing trial wind-up timing is retained.

#### Mixer
Assets/Audio/Mixers/SakuraAudioMixer.mixer:

```text
Master (MasterVolume)
+-- Music (MusicVolume)
|   +-- PauseFX (snapshot attenuation and Lowpass)
+-- SFX (SFXVolume)
```

Both music sources route through Music/PauseFX. The additional child bus keeps snapshot settings separate from exposed user volume. Normal: 0 dB and 22000 Hz. Paused: -10 dB and 1100 Hz. Transition: 0.3 seconds. The mixer uses UnscaledTime. SFX, including menu clicks, remain available during pause. User volumes are not rewritten by a snapshot.

#### Settings and saving
MainMenu > Settings contains Master, Music and SFX sliders, each 0–1. SakuraVolumeSettings binds all three sliders, uses SetValueWithoutNotify and removes only its own listeners on disable. Existing ProgressMenu continues to open and refresh the Settings panel; its former persistent Master callback is replaced by the same explicit listener lifecycle as Music and SFX.

`20f * Mathf.Log10(Mathf.Max(value, 0.0001f))` converts the sliders to decibels; zero gives -80 dB. AudioListener.volume is unity to avoid applying Master twice.

The existing Assignment 11 SaveSystem and `sakura-progress.json` are retained. `volume` remains Master; `musicVolume`, `sfxVolume` and `audioVersion` are added. Old version-1 saves keep their progress and receive Music=0.75 and SFX=0.85. Invalid audio fields use defaults. There is no separate PlayerPrefs save. Desktop location: `%USERPROFILE%/AppData/LocalLow/DefaultCompany/My project (2)/sakura-progress.json`. On mobile, use Application.persistentDataPath. The filename deliberately remains the existing one rather than introducing save.json.

#### Audio assets, origin and license
All active Assignment 12 audio is original deterministic synthesis made for Sakura by the coding assistant during this task on 24 November 2026. No downloaded recordings, external samples, commercial songs or copied melodies were used. Music uses original 16-bar patterns, plucked harmonic tones, sustained tones and synthesized percussion. Effects use enveloped sine sweeps, harmonics and seeded noise. Generator: `Assets/Editor/Audio/SakuraAudioSynthesis.cs`. WAVs: 44.1 kHz, 16-bit mono. Music tails wrap across the loop boundary. WAV peak amplitude is capped at 0.65; game mixing still needs a human listening check.

The newly generated WAV assets are offered under CC0 1.0 to the extent rights exist. No attribution is required; this provenance table is retained for the assignment. License reference: https://creativecommons.org/publicdomain/zero/1.0/ . There is no external original-source URL: each source is the local generator named above.

| Asset | File | Purpose | Creator | Original source URL | License | Attribution |
|---|---|---|---|---|---|---|
| Menu Garden | Audio/Music/MenuGarden.wav | Calm menu loop, 80 BPM | Coding assistant, original synthesis | None; local generator | CC0 1.0 | None |
| Sakura Journey | Audio/Music/SakuraJourney.wav | Gameplay loop, 120 BPM | Same | None; local generator | CC0 1.0 | None |
| Jump | Audio/SFX/Jump.wav | Jump | Same | None; local generator | CC0 1.0 | None |
| Hit | Audio/SFX/Hit.wav | Damage | Same | None; local generator | CC0 1.0 | None |
| Pickup | Audio/SFX/Pickup.wav | Heart pickup | Same | None; local generator | CC0 1.0 | None |
| Windup | Audio/SFX/Windup.wav | Enemy warning | Same | None; local generator | CC0 1.0 | None |
| Death | Audio/SFX/Death.wav | Actor death | Same | None; local generator | CC0 1.0 | None |
| Click | Audio/SFX/Click.wav | UI click | Same | None; local generator | CC0 1.0 | None |
| Victory | Audio/SFX/Victory.wav | Win result | Same | None; local generator | CC0 1.0 | None |
| Defeat | Audio/SFX/Defeat.wav | Lose result | Same | None; local generator | CC0 1.0 | None |
| Shot | Audio/SFX/Shot.wav | Player projectile | Same | None; local generator | CC0 1.0 | None |
| Attack | Audio/SFX/Attack.wav | Enemy attack | Same | None; local generator | CC0 1.0 | None |
| Land | Audio/SFX/Land.wav | Landing | Same | None; local generator | CC0 1.0 | None |
| Portal | Audio/SFX/Portal.wav | Portal opening | Same | None; local generator | CC0 1.0 | None |

Paths in the table are relative to Assets. Existing Assets/Sounds/shoot.mp3, damage.mp3 and portal.mp3 are preserved historical assets; their original source and license are unknown. New event calls and serialized actor references use generated clips instead. Do not claim a license for the old files or redistribute them as licensed Assignment 12 audio without checking their origin.

#### Variation and mobile
Repeated effects select a new pitch from 0.92–1.08 on a free source. Victory, defeat and clicks retain pitch 1. Music pitch is never randomized. Music uses streaming Vorbis; short effects use decompressed PCM. Existing touch controls, build scenes and mobile configuration are preserved.

#### Testing
Use Tools > Sakura > Test Assignment 12 Audio. Tests use `Logs/Assignment12TestSave/progress.json`, not player progress. Read Logs/Assignment12-tests.txt for actual outcomes. Separate-process persistence, scene audit and Android build tools are in SakuraAudioBuild. Use Tools > Sakura > Build Assignment 12 APK. Generated logs and builds are not committed.

Manual checks: listen to both music loops and their boundary; move all three sliders; pause and resume; retry and change levels; jump, shoot, take damage, collect a heart, kill an enemy, win and lose. Hear every enemy warning before impact. Also open the Assignment 10 scene and check its three enemies. Verify phone touch input, volume after closing/reopening the app, and audio on the physical device. Automated checks do not replace human listening or phone tests.

See ASSIGNMENT12_DEFENSE.md and ASSIGNMENT12_SUBMISSION.md for the script map and recording sequence.

