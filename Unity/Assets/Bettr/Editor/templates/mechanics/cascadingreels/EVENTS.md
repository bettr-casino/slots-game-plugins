# EVENTS for Cascading Reels Mechanic with Animation Events

This document lists the key events that occur during the execution of the Cascading Reels Mechanic in a slot machine game, focusing on cascade-specific events. It now includes animation events that dictate visual feedback during reel spinning, symbol removal, and cascading.

---

## BaseGame States

1. **Waiting**: The game is in an idle state, waiting for player input.
    - **Animation**: Show idle animation for the base game (e.g., idle UI effects, button highlights).
  
2. **Spinning**: The game has entered the spinning state, and the reels are actively spinning.
    - **Animation**: Trigger reel spin animations for all active reels, with possible sound effects and lighting effects.

3. **Completed**: The game has finished all spins, cascades, and evaluations, and payouts are being calculated.
    - **Animation**: Play win celebration animations, showing the total payout with effects like flashing lights, coin animations, and celebratory sounds.

---

## Reel States

1. **Waiting**: The reel is idle, waiting to be triggered for a spin.
    - **Animation**: Show idle animation for each reel (e.g., a slight reel wobble effect or glow effect on the reel borders).
  
2. **Spinning**: The reel is actively spinning after being triggered by the base game.
    - **Animation**: Play the reel spinning animation, making the symbols blur as they spin with accompanying sound effects.

3. **Stopped**: The reel has stopped spinning and awaits evaluation.
    - **Animation**: Play reel stop animation, including possible slow-motion effects when stopping on important symbols.

4. **ReachedOutcomeStopIndex**: The reel has stopped at a specific pre-determined outcome.
    - **Animation**: Highlight the winning symbols on the reel with a glowing effect or flashing border.

5. **SpinStartedRollBack**: The reel performs a backward animation roll, often for visual effects during or after a spin.
    - **Animation**: Play a smooth backward reel roll animation, creating a slight visual "bounce" effect.

6. **SpinStartedRollForward**: The reel performs a forward animation roll, often used for smooth transitions during or after a spin.
    - **Animation**: Play a forward reel roll animation, giving a smooth transition for cascading symbols.

7. **SpinEndingRollBack**: A final backward roll effect when the reel is ending its spin.
    - **Animation**: Trigger a quick reverse motion for visual feedback before the reel comes to a full stop.

8. **SpinEndingRollForward**: A final forward roll effect when the reel is ending its spin.
    - **Animation**: Play a final smooth roll forward before the reel stops completely.

---

## Cascade Events with Animation Events

### 1. **Cascade Started**
- **Event**: After the reels stop spinning and a winning combination is formed, the game enters the cascading phase.
- **State**: `Spinning`
- **Trigger**: Winning combination detected on the reels.
- **Applies To**: **BaseGame**
- **Animation**: Start a cascade initiation animation. This may include highlighting the winning symbols with a glowing or pulsing effect and transitioning into the symbol removal animation.

### 2. **Symbol Removal**
- **Event**: Winning symbols are removed from the reels to make space for new symbols to cascade down.
- **State**: `ReachedOutcomeStopIndex`
- **Trigger**: Winning combination detected.
- **Applies To**: **Symbols**, **Reels**
- **Animation**: Play a symbol removal animation, where the winning symbols either disappear, explode, or shrink with a sound effect (e.g., popping or sparkle effect) to indicate removal.

### 3. **Symbol Cascade**
- **Event**: Symbols cascade down to fill empty spaces created by the removal of winning symbols.
- **State**: `SpinStartedRollForward` or `SpinStartedRollBack`
- **Trigger**: Space created on the reel after symbol removal.
- **Applies To**: **Symbols**, **Reels**
- **Animation**: Animate the symbols falling or sliding down to fill the empty spaces. This can include a tumbling or rolling effect, depending on the theme of the game. Use smooth, continuous motion to create a satisfying visual for the cascade.

### 4. **End of Cascade**
- **Event**: No further cascades occur, and the reels return to a stopped state.
- **State**: `Stopped`
- **Trigger**: No new winning combinations are formed.
- **Applies To**: **BaseGame**, **Reels**
- **Animation**: Play a soft stopping animation where the symbols settle into their final positions, possibly with a gentle shake or "bounce" as they stop moving.

### 5. **Cascade Completed**
- **Event**: No further winning combinations are formed after a series of cascading reels.
- **State**: `Completed`
- **Trigger**: No new wins detected.
- **Applies To**: **BaseGame**
- **Animation**: Trigger a celebratory animation indicating that all cascades have finished. This could include flashing lights, coin showers, or other congratulatory effects.

---

## Event Flow Example with Animation Events

1. **Player Presses Spin**
   - **State**: `Spinning`
   - **Trigger**: Spin button pressed.
   - **Applies To**: **BaseGame**, **Reels**
   - **Animation**: Trigger reel spin animations for all reels.

2. **Reel Stops**
   - **State**: `Stopped`
   - **Trigger**: Reel reaches stop position and outcome is determined.
   - **Applies To**: **Reels**
   - **Animation**: Play the reel stop animation with a focus on slowing down and highlighting important symbols.

3. **Cascade Initiates**
   - **Event**: Winning symbols are removed, and symbols cascade with new symbols replacing them.
   - **State**: `SpinStartedRollBack` or `SpinStartedRollForward`
   - **Trigger**: Space created after symbol removal.
   - **Applies To**: **Symbols**, **Reels**
   - **Animation**: Play symbol removal animations, followed by cascading animations where new symbols fall down to fill empty spaces.

4. **End of Cascade**
   - **State**: `Stopped`
   - **Trigger**: No more winning combinations.
   - **Applies To**: **BaseGame**, **Reels**
   - **Animation**: Symbols stop cascading and settle into their final positions with a soft bounce effect.

5. **Payout**
   - **Event**: Payouts are calculated after the cascade ends, and the game resets to idle.
   - **State**: `Completed`
   - **Applies To**: **BaseGame**
   - **Animation**: Trigger payout animations, which could include flashing lights, coin animations, and win celebration effects.

---

## Summary of Cascade-Specific Key States with Animation Events

### Reel
- **ReachedOutcomeStopIndex**: Reel stopped at a pre-determined outcome. Winning symbols are highlighted with a glowing or pulsing animation.
- **SpinStartedRollBack / SpinStartedRollForward**: Transitional animations during spinning and cascading. These include rolling animations to smooth the transition between symbol removal and cascading.
- **Stopped**: Reel has stopped spinning after cascades. Symbols stop with a small bounce or shake effect.
- **Completed**: All cascades and evaluations are complete, and the game is ready for payout. Trigger celebratory animations if the player has won.

---

This **EVENTS.md** file now includes both the state lists for **BaseGame** and **Reels**, along with the specific cascade events and animation events that occur at each stage. These animation events provide visual feedback to enhance player engagement during the game.
