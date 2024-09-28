# EVENTS for Cascading Reels Mechanic

This document lists the key events that occur during the execution of the Cascading Reels Mechanic in a slot machine game, focusing on cascade-specific events. The common **Base Game** and **Reel** events are excluded.

---

## Cascade Events

### 1. **Cascade Started**
- **Event**: After the reels stop spinning and a winning combination is formed, the game enters the cascading phase.
- **State**: `Spinning`
- **Trigger**: Winning combination detected on the reels.

### 2. **Symbol Removal**
- **Event**: Winning symbols are removed from the reels to make space for new symbols to cascade down.
- **State**: `ReachedOutcomeStopIndex`
- **Trigger**: Winning combination detected.

### 3. **Symbol Cascade**
- **Event**: Symbols cascade down to fill empty spaces created by the removal of winning symbols.
- **State**: `SpinStartedRollForward` or `SpinStartedRollBack`
- **Trigger**: Space created on the reel after symbol removal.

### 4. **End of Cascade**
- **Event**: No further cascades occur, and the reels return to a stopped state.
- **State**: `Stopped`
- **Trigger**: No new winning combinations are formed.

### 5. **Cascade Completed**
- **Event**: No further winning combinations are formed after a series of cascading reels.
- **State**: `Completed`
- **Trigger**: No new wins detected.

---

## Event Flow Example

1. **Player Presses Spin**: 
   - Base Game enters `Spinning`, Reels enter `Spinning` and use animation states like `SpinStartedRollForward`.

2. **Reel Stops**:
   - Reels move to `Stopped` and then `ReachedOutcomeStopIndex` for symbol evaluation.

3. **Cascade Initiates**:
   - Winning symbols are removed, reels cascade with new symbols using `SpinStartedRollBack` or `SpinStartedRollForward`.

4. **End of Cascade**:
   - Reels enter `Stopped`, Base Game enters `Completed` after all cascades are finished.

5. **Payout**:
   - Payouts are calculated, and the Base Game returns to `Waiting`.

---

## Summary of Cascade-Specific Key States

### Reel
- **ReachedOutcomeStopIndex**: Reel stopped at a pre-determined outcome.
- **SpinStartedRollBack / SpinStartedRollForward**: Transitional animations during spinning and cascading.
- **Stopped**: Reel has stopped spinning after cascades.
- **Completed**: All cascades and evaluations are complete, and the game is ready for payout.

---

This **EVENTS.md** file focuses on the events and state transitions that are specific to the Cascading Reels Mechanic in the slot machine game, excluding common base game and reel spin mechanics.
