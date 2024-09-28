# EVENTS for Cascading Reels Mechanic

This document lists the key events that occur during the execution of the Cascading Reels Mechanic in a slot machine game, focusing on cascade-specific events. The common **BaseGame** and **Reel** events are also included along with their state lists.

---

## BaseGame States

1. **Waiting**: The game is in an idle state, waiting for player input.
2. **Spinning**: The game has entered the spinning state, and the reels are actively spinning.
3. **Completed**: The game has finished all spins, cascades, and evaluations, and payouts are being calculated.

---

## Reel States

1. **Waiting**: The reel is idle, waiting to be triggered for a spin.
2. **Spinning**: The reel is actively spinning after being triggered by the base game.
3. **Stopped**: The reel has stopped spinning and awaits evaluation.
4. **ReachedOutcomeStopIndex**: The reel has stopped at a specific pre-determined outcome.
5. **SpinStartedRollBack**: The reel performs a backward animation roll, often for visual effects during or after a spin.
6. **SpinStartedRollForward**: The reel performs a forward animation roll, often used for smooth transitions during or after a spin.
7. **SpinEndingRollBack**: A final backward roll effect when the reel is ending its spin.
8. **SpinEndingRollForward**: A final forward roll effect when the reel is ending its spin.

---

## Cascade Events

### 1. **Cascade Started**
- **Event**: After the reels stop spinning and a winning combination is formed, the game enters the cascading phase.
- **State**: `Spinning`
- **Trigger**: Winning combination detected on the reels.
- **Applies To**: **BaseGame**

### 2. **Symbol Removal**
- **Event**: Winning symbols are removed from the reels to make space for new symbols to cascade down.
- **State**: `ReachedOutcomeStopIndex`
- **Trigger**: Winning combination detected.
- **Applies To**: **Symbols**, **Reels**

### 3. **Symbol Cascade**
- **Event**: Symbols cascade down to fill empty spaces created by the removal of winning symbols.
- **State**: `SpinStartedRollForward` or `SpinStartedRollBack`
- **Trigger**: Space created on the reel after symbol removal.
- **Applies To**: **Symbols**, **Reels**

### 4. **End of Cascade**
- **Event**: No further cascades occur, and the reels return to a stopped state.
- **State**: `Stopped`
- **Trigger**: No new winning combinations are formed.
- **Applies To**: **BaseGame**, **Reels**

### 5. **Cascade Completed**
- **Event**: No further winning combinations are formed after a series of cascading reels.
- **State**: `Completed`
- **Trigger**: No new wins detected.
- **Applies To**: **BaseGame**

---

## Event Flow Example

1. **Player Presses Spin**
   - **State**: `Spinning`
   - **Trigger**: Spin button pressed.
   - **Applies To**: **BaseGame**, **Reels**

2. **Reel Stops**
   - **State**: `Stopped`
   - **Trigger**: Reel reaches stop position and outcome is determined.
   - **Applies To**: **Reels**

3. **Cascade Initiates**
   - **Event**: Winning symbols are removed, and symbols cascade with new symbols replacing them.
   - **State**: `SpinStartedRollBack` or `SpinStartedRollForward`
   - **Trigger**: Space created after symbol removal.
   - **Applies To**: **Symbols**, **Reels**

4. **End of Cascade**
   - **State**: `Stopped`
   - **Trigger**: No more winning combinations.
   - **Applies To**: **BaseGame**, **Reels**

5. **Payout**
   - **Event**: Payouts are calculated after the cascade ends, and the game resets to idle.
   - **State**: `Completed`
   - **Applies To**: **BaseGame**

---

## Summary of Cascade-Specific Key States

### Reel
- **ReachedOutcomeStopIndex**: Reel stopped at a pre-determined outcome.
- **SpinStartedRollBack / SpinStartedRollForward**: Transitional animations during spinning and cascading.
- **Stopped**: Reel has stopped spinning after cascades.
- **Completed**: All cascades and evaluations are complete, and the game is ready for payout.

---

This **EVENTS.md** file now includes both the state lists for **BaseGame** and **Reels** as well as the specific events for the Cascading Reels Mechanic.
