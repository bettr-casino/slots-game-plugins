# ORCHESTRATION_EXAMPLE.md

## Orchestration Example for Nudging Reels Mechanic

This section provides an orchestration example, showing how different events are managed and triggered during a typical slot game utilizing the **Nudging Reels** mechanic.

### Step-by-Step Orchestration

### 1. **Initial Spin**

- **Event**: Player presses the "Spin" button.
- **Action**: The reels start spinning normally as part of the base game.
- **Outcome**: After spinning, the reels come to a stop and display symbols on the paylines.
  
  **Example**: The first two reels display matching symbols, but the third reel stops just one position away from a complete winning combination.

### 2. **Check for Near-Win Condition**

- **Event**: The game engine checks for any near-win scenarios on the reels.
- **Action**: The engine identifies that a high-value combination is one symbol away from being completed.
- **Outcome**: The game triggers the **Nudge Triggered** event based on the near-win condition.

  **Example**: The third reel is one position away from landing a wild symbol, which would complete the winning combination.

### 3. **Nudge Triggered**

- **Event**: **Nudge Triggered** event is activated.
- **Action**: The game nudges the third reel down by one position.
- **Outcome**: The nudge moves the wild symbol into place, completing the winning combination.

  **Example**: The wild symbol is now aligned with the other two matching symbols on the paylines, forming a high-paying combination.

### 4. **Nudge Completed**

- **Event**: **Nudge Completed** event is triggered after the reel has finished nudging.
- **Action**: The game recalculates the new symbol positions and checks for any winning combinations.
- **Outcome**: A winning combination is confirmed, and the game moves on to calculating the payout.

  **Example**: The player now has three matching symbols on the payline, with the wild completing the combination.

### 5. **Payout Calculation**

- **Event**: The game calculates the payout for the winning combination.
- **Action**: The win is displayed on the screen, and the corresponding credit is added to the player's balance.
- **Outcome**: The player receives their payout based on the game’s paytable.

  **Example**: The player is awarded a payout for landing three high-value symbols, with the wild contributing to the win.

### 6. **Bonus Feature Triggered (Optional)**

- **Event**: During this spin, the player has also landed scatter symbols that could trigger a bonus round.
- **Action**: The game checks for scatter symbols as part of the **Nudge Scatter Symbol** event. If scatters have nudged into position, the bonus feature is triggered.
- **Outcome**: The player enters the free spins round or another bonus feature.

  **Example**: The scatter symbol is nudged into place, triggering a free spins bonus round for the player.

### Orchestration Overview

The orchestration of the **Nudging Reels** mechanic involves several interconnected events:

1. **Initial Spin**: The reels spin and come to a stop, showing symbols on the screen.
2. **Nudge Triggered**: If a near-win scenario is identified, a nudge is triggered to improve the result.
3. **Nudge Completed**: After the nudge, the game checks if a winning combination is formed.
4. **Payout Calculation**: The player is awarded for the winning combination.
5. **Bonus Feature Triggered**: Optional nudge of scatter symbols may trigger a bonus round.

### Example Timeline

1. Spin -> Reels Stop -> Check for Near-Win
2. Near-Win Detected -> Nudge Triggered -> Reel Nudges
3. Nudge Completed -> Winning Combination Formed
4. Payout Calculated -> Credits Added to Player’s Balance
5. (Optional) Scatter Nudge -> Bonus Feature Triggered

### Conclusion

This example illustrates how the different events within the **Nudging Reels** mechanic work together in a seamless flow. From detecting near-wins and triggering nudges to calculating payouts and triggering bonus features, the orchestration ensures that the mechanic adds excitement and anticipation to the game.
