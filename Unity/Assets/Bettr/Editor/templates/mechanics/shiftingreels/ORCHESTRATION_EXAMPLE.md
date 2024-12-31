# ORCHESTRATION_EXAMPLE.md

## Orchestration Example for Shifting Reels Mechanic

This section provides a detailed orchestration example of how the **Shifting Reels** mechanic functions during a slot game. It demonstrates the flow of events from the initial spin to the final payout, emphasizing the reel shifts that happen in unison and the potential for consecutive wins.

### Step-by-Step Orchestration

### 1. **Initial Spin**

- **Event**: Player presses the "Spin" button to start the reels.
- **Action**: The reels spin normally, and the player awaits the outcome.
- **Outcome**: The reels stop, and a winning combination is formed.

  **Example**: The player lands a winning combination on reels 1, 2, and 3, forming a winning line of matching symbols.

### 2. **Shifting Reels Triggered**

- **Event**: The **Shifting Reels** mechanic is triggered by the winning combination.
- **Action**: The game prepares to shift the reels horizontally (or vertically) to allow for new symbols to appear.
- **Outcome**: The reels are set to move in unison, with all reels shifting one position to the right (for horizontal shifts) or downward (for vertical shifts).

  **Example**: The symbols on **Reel 4** move to **Reel 5**, **Reel 3** to **Reel 4**, and so on. **Reel 1** is populated with new symbols from the left.

### 3. **Reel Shift in Unison**

- **Event**: The reels shift **in unison**, moving one position horizontally or vertically.
- **Action**: All reels move simultaneously, ensuring a smooth transition. The leftmost reel (for horizontal shifts) or topmost row (for vertical shifts) is filled with new symbols.
- **Outcome**: New symbols enter **Reel 1**, replacing the vacated positions and creating the possibility for new winning combinations.

  **Example**: All reels shift to the right, with new symbols appearing on **Reel 1**. The new symbols now create a potential for additional wins.

### 4. **New Winning Combinations**

- **Event**: The game checks for new winning combinations after the shift.
- **Action**: The new symbol positions are evaluated, and if a winning combination is formed, the game triggers another shift.
- **Outcome**: If a new win is formed, the shifting process repeats, and the win multiplier increases if applicable.

  **Example**: The new symbols in **Reel 1** create another winning combination. The reels prepare to shift again, and the win multiplier increases to **2x** for the next win.

### 5. **Subsequent Shifts and Win Multiplier Increase**

- **Event**: The reels shift again if a new winning combination is formed.
- **Action**: The reels shift **in unison** once more, with new symbols filling the vacated positions, and the win multiplier increases with each shift.
- **Outcome**: The multiplier increases progressively as new winning combinations are formed from each shift.

  **Example**: The reels shift again, and the win multiplier increases to **3x**. The new symbols create yet another winning combination, continuing the sequence.

### 6. **End of Shifts**

- **Event**: The shifting reels mechanic ends when no further winning combinations are formed after a shift.
- **Action**: The reels stop shifting, and the game calculates the final payout.
- **Outcome**: The player is awarded the total payout from the initial spin, plus any winnings from consecutive shifts, including the win multiplier if applicable.

  **Example**: After the final shift, no new winning combination is formed. The game calculates the total payout, including the **3x** multiplier from consecutive shifts, and awards the final amount to the player.

### Orchestration Overview

The orchestration of the **Shifting Reels** mechanic follows a structured sequence of events:
1. **Initial Spin**: The player spins the reels and lands a winning combination, triggering the shifting reels feature.
2. **Shifting Reels Triggered**: The reels shift after a win, moving **in unison** to allow for new symbols to appear.
3. **New Symbols Enter**: The leftmost reel (or top row) is filled with new symbols, and the game checks for new winning combinations.
4. **New Wins and Shifts**: If a new win is created, the reels shift again, and the win multiplier increases progressively.
5. **End of Shifts**: The shifting process continues until no more wins are formed, and the player is awarded the total payout.

### Example Timeline

1. **Spin** → Reels Stop → **Winning Combination**
2. **Shift Triggered** → Reels Move in Unison
3. **New Symbols Enter** → Check for New Wins
4. **New Win** → Reels Shift Again → **Multiplier Increases**
5. **End of Shifts** → Total Payout Calculation

### Conclusion

This orchestration example demonstrates how the **Shifting Reels** mechanic functions smoothly during gameplay. The reels shift **in unison**, and new symbols enter the grid, allowing for multiple consecutive wins. With the potential for increasing win multipliers, the shifting reels mechanic can lead to extended win sequences and significantly enhanced payouts for the player.
