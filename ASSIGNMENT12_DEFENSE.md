# Assignment 12 defense — simple script map

## Scripts
- **SakuraAudio.cs** manages all sound. It stays alive between scenes. It has two music sources and twenty effect sources. Its prefab contains all clip and mixer references.
- **SakuraVolumeSettings.cs** connects all three volume sliders to SaveSystem. It updates slider positions when saved values change.
- **SakuraButtonSound.cs** plays a click when a Unity Button runs its click event.
- **SaveSystem.cs** is the old Assignment 11 save system. It now stores musicVolume, sfxVolume and audioVersion. The old volume field is Master. Progress uses the same JSON file.
- **ProgressMenu.cs** is the existing menu. Its existing Master slider now uses SakuraVolumeSettings to call SaveSystem.SetVolume. The setup tool adds two matching sliders.
- **PlayerMovement.cs** plays jump, landing and shot sounds. A shot plays when the projectile is created.
- **PlayerHealth.cs** plays damage and death sounds. Its existing protection prevents repeated damage during invulnerability.
- **HeartPickup.cs** plays a pickup once, before the heart is destroyed.
- **Enemy.cs** plays damage and death. Its contact attack first plays a warning, waits 0.45 seconds, and checks if the player still touches it.
- **BossController.cs** plays a warning before a projectile. It waits at least 0.45 seconds. Contact damage also has a warning and a second contact check.
- **TrialEnemy.cs** is the base for BatPatroller, CrateThrower and CrownCharger. It plays the warning when an enemy enters preparation. It plays the attack sound when it enters Attack. The original states control the timing.
- **GameManager.cs** asks for the pause snapshot. It plays victory and defeat once when a level ends.
- **Portal.cs** plays the portal sound once when the exit opens.
- **SakuraAudioSetup.cs** is an Editor tool. It creates and connects the mixer, prefab and sliders, and saves scenes and source routing.
- **SakuraAudioSynthesis.cs** creates the original WAV files from math. It does not download music.
- **SakuraAudioTests.cs / SakuraAudioTestRunner.cs / SakuraAudioBuild.cs** are Editor checks and build tools. The runner is excluded from player builds.

## Communication
```text
Player / enemies / pickup / UI / GameManager
                    |
                    v
              SakuraAudio.Play
                    |
             SFX voice pool ----> SFX ----> Master ----> speakers

Scene change ---> SakuraAudio crossfade
                    |
               two music sources ---> PauseFX ---> Music ---> Master

Settings sliders ---> SaveSystem ---> existing JSON file
                         |
                    Changed event
                         |
              SakuraAudio + settings display

Pause button ---> GameManager ---> SakuraAudio ---> mixer snapshot
```

## Main ideas
**Crossfade:** the new track grows louder while the old one becomes quieter for one second. The code uses unscaled delta time. Gameplay scenes use the same track, so changing levels does not start it again.

**Groups:** Music and SFX are separate children of Master. A slider changes only its own group. Music sources enter the PauseFX child of Music.

**Snapshots:** Normal and Paused are saved mixer states. Paused lowers PauseFX by 10 dB and lowers its filter cutoff to 1100 Hz. The transition lasts 0.3 seconds. The Music slider is on a different stage, so pause does not replace the user's volume.

**Decibels:** the slider is between zero and one. We use `20 * Log10(Max(value, 0.0001))`. One is 0 dB, half is about -6 dB, and zero is -80 dB.

**Save:** SaveSystem writes the three values with the old progress data. Old saves receive default Music and SFX values. Scores and unlocked levels remain unchanged when audio values are added.

**Variation:** every short effect uses a free source. Its pitch is between 0.92 and 1.08. Two overlapping sounds have different sources, so one does not change the other. Music stays at normal pitch.

**Enemy warning:** the warning happens at preparation, before damage. It is not the player's damage sound. The player can jump or move away during this time.

## Why existing files changed
Gameplay scripts know the exact moment of an action, so audio calls belong there. GameManager knows pause and results. SaveSystem already owns progress and settings, so it also owns volume. The scenes and result-panel prefab receive source routes and click components. MainMenu receives the two extra sliders in its existing pink style. No separate game was created.

