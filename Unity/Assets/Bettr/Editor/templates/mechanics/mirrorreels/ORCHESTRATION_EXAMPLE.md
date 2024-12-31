# ORCHESTRATION EXAMPLE.md

## Mirror Reels Mechanic with Detailed Visual Orchestration

The **Mirror Reels** mechanic is an engaging slot machine feature where two or more reels mirror each other's symbols, displaying identical results during a spin. This mechanic enhances the chances of landing high-paying combinations by increasing symbol alignment across multiple reels. The orchestration of this feature involves both visual and audio elements to create an immersive and exciting player experience.

### Visual Animation Orchestration

The **Mirror Reels** mechanic is visually enhanced through detailed animations and sound effects to build anticipation and excitement. Here's how the animation orchestration unfolds:

---

## Scenario:
- **Grid Configuration**: 5 reels, each with 3 rows.
- **Mirror Reels**:
  - **Reels 2 and 4** mirror each other during the spin, showing identical symbols.

---

### 1. **Pre-Activation Anticipation**

Before the mirror reels mechanic is activated, the game builds excitement by giving players subtle visual and audio cues that something special is about to happen:

| Step | Event                            | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | Function |
|------|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Highlighting Potential Mirror Reels** | The reels that are likely to mirror are highlighted with glowing borders or shimmering effects.                                                                                                              | Reels 2 and 4 glow faintly with shimmering light, giving the player a hint that these reels might mirror.                                                                                                        | `HighlightReels(reels)` |
| 2    | **Reel Pulsing**                 | The source reel (reel 2) starts pulsing or glowing faintly to draw attention.                                                                                                                                                                      | Reel 2 begins a subtle pulsing effect, drawing attention to the possibility that it will mirror its symbols.                                                                                                      | `PulseSourceReel(sourceReel)` |
| 3    | **Sound Cues**                   | An escalating sound effect builds suspense as the player anticipates the mirror reels activation.                                                                                                                                                 | A rising musical note or sound effect increases in intensity, creating suspense for the player.                                                                                                                  | `PlayEscalatingSound()` |

---

### 2. **Mirror Reels Activation**

When the mirror reels mechanic is triggered, the orchestration intensifies:

| Step | Event                            | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | Function |
|------|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Mirroring Animation**          | A visual effect such as a beam of light or electric surge travels from reel 2 (source) to reel 4 (target), visually linking the reels as they mirror.                                                                                               | A **beam of light** shoots from reel 2 to reel 4, visually linking the two reels as the symbols begin to mirror.                                                                                                | `ActivateMirrorEffect(sourceReel, targetReel)` |
| 2    | **Symbol Reflection**            | The symbols on the target reel flash briefly or glow as they mirror the source reel’s symbols.                                                                                                                                                    | Reel 4 glows brightly as the symbols from reel 2 are copied over.                                                                                                                                                 | `ReflectSymbols(targetReel)` |

---

### 3. **Sound and Music Cues**

Audio cues play an essential role in heightening the excitement of the mirror reels feature:

| Step | Event                            | Description                                                                                                                                                                                                                                      | Sound Orchestration                                                                                                                                                                                            | Function |
|------|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Mirroring Sound Effect**       | A distinct sound effect, such as a chime or magical tone, plays as the reels mirror.                                                                                                                                                              | A **magical chime** or **laser zap** sound plays as reel 4 mirrors reel 2.                                                                                                                                       | `PlayMirrorSound()` |
| 2    | **Music Shift**                  | The background music shifts to a more intense or upbeat track to signal the activation of the mirror reels mechanic.                                                                                                                               | The game’s background music becomes more intense, adding excitement to the moment.                                                                                                                              | `ShiftBackgroundMusic()` |
| 3    | **Symbol Lock Sound**            | A satisfying click or chime plays when the mirrored reels lock into place.                                                                                                                                                                        | A **click sound** marks the locking of reel 4 after the symbols are mirrored.                                                                                                                                     | `PlayLockSound()` |

---

### 4. **Post-Mirroring Celebration**

After the mirror reels have been synchronized, celebratory animations and sound effects reinforce the increased win potential:

| Step | Event                            | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | Function |
|------|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Mirrored Reels Glow**          | The mirrored reels continue to glow or pulse with light after the mirroring process is complete.                                                                                                                                                  | Reels 2 and 4 pulse with a glowing effect, signaling their potential for creating winning combinations.                                                                                                         | `GlowMirroredReels(reels)` |
| 2    | **Win Celebration Animation**    | If the mirrored reels contribute to a win, celebratory animations such as coins flying out of the reels or fireworks burst across the screen.                                                                                                      | **Coins** or **fireworks** explode from the reels when the mirrored reels help form a winning combination.                                                                                                      | `CelebrateWin(reels)` |
| 3    | **Audio Win Cues**               | A triumphant fanfare or the sound of cascading coins plays when the player wins from the mirrored reels.                                                                                                                                           | A **triumphal sound** or **coins cascading** is heard when the mirrored reels trigger a win.                                                                                                                    | `PlayWinSound()` |

---

## Function Headers for New Functions

- `HighlightReels(reels)`: Highlights the reels that might be involved in the mirror reels mechanic by applying glowing borders or shimmering effects.
- `PulseSourceReel(sourceReel)`: Causes the source reel to pulse or glow, building anticipation.
- `PlayEscalatingSound()`: Plays an escalating sound effect to increase anticipation before the mirror reels mechanic is triggered.
- `ActivateMirrorEffect(sourceReel, targetReel)`: Initiates the visual effect linking the source reel to the target reel(s), activating the mirror reels feature.
- `ReflectSymbols(targetReel)`: Copies the symbols from the source reel to the target reel and plays the appropriate animation.
- `PlayMirrorSound()`: Plays the sound effect associated with the mirroring of the reels.
- `ShiftBackgroundMusic()`: Changes the background music to a more intense track when the mirror reels mechanic is triggered.
- `PlayLockSound()`: Plays a sound effect when the mirrored reels lock into place after the symbols are copied.
- `GlowMirroredReels(reels)`: Makes the mirrored reels glow or pulse after the mirroring is complete.
- `CelebrateWin(reels)`: Plays celebratory animations such as coins or fireworks when the mirrored reels help create a winning combination.
- `PlayWinSound()`: Plays an audio cue, such as a fanfare or coin sound, when the player wins with the mirrored reels.

---

### Example Timeline

1. **Pre-Activation -> Reels 2 and 4 Begin Glowing**
2. **Mirror Activation -> Reel 4 Mirrors Reel 2**
3. **Post-Mirroring -> Reels Continue Glowing**
4. **Win Celebration -> Coins and Sound Effects Play**

---

### Conclusion

The orchestration of the **Mirror Reels** mechanic combines detailed visual and sound effects to create an engaging and exciting player experience. The synchronization of the reels, accompanied by audio cues, builds suspense and amplifies the excitement of forming winning combinations.
