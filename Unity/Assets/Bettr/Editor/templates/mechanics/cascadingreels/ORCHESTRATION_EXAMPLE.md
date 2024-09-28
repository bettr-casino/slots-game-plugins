# Orchestration for Cascading Reels in a 5x3 Grid with 2 Payline Wins (with Corrected Group-Based Animation)

This document describes an example scenario for a **5x3 grid slot machine** with two payline wins:

- One win occurs **across Row 3** on Reels 1, 2, and 3.
- Another win occurs **across Row 2** on Reels 1, 2, and 3.

The orchestration details how symbols are animated, removed row by row, and how the remaining symbols cascade down and fill the empty spaces.

---

## Scenario:
- **Grid Configuration**: 5 reels, each with 3 rows.
- **Winning Paylines**:
  - **Row 3** (bottom row): Winning combination across **Reel 1**, **Reel 2**, and **Reel 3**.
  - **Row 2** (middle row): Another winning combination across **Reel 1**, **Reel 2**, and **Reel 3**.

---

## Corrected Orchestration for 5x3 Grid with 2 Payline Wins (with Animation)

| Step | Event                         | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        |
|------|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 1    | **Cascade Started**            | The reels have stopped, and winning combinations on **Row 3** and **Row 2** (for Reels 1, 2, and 3) are detected.                                                                                                                                      | **Highlight** the winning symbols in **Row 3** and **Row 2** with a **glowing or pulsing effect**. Accompany this with a sound effect indicating a win has been detected.                                        |
| 2    | **Winning Symbols Animation (Row 3)** | The winning symbols in **Row 3** play an animation to indicate success before removal.                                                                                                                                            | Play an **animation** (e.g., **enlarging, shaking, or glowing**) on the **Row 3** winning symbols. Sound effects such as a rising tone or celebratory sound play alongside.                                      |
| 3    | **Symbol Removal (Row 3)**     | Winning symbols from **Row 3** (the bottom-most row) on Reels 1, 2, and 3 are removed.                                                                                                                                                         | Play **symbol removal animation** (e.g., symbols **explode, shrink, or vanish** with a sparkle effect) as they are cleared from Row 3. Include a sound effect for symbol removal.                               |
| 4    | **Winning Symbols Animation (Row 2)** | After Row 3 is cleared, the winning symbols in **Row 2** play their success animation before removal.                                                                                                                                                      | Play an **enlarging, shaking, or glowing animation** on the **Row 2** winning symbols.                                                                                                                                                               |
| 5    | **Symbol Removal (Row 2)**     | Winning symbols from **Row 2** (middle row) on Reels 1, 2, and 3 are removed after their animation.                                                                                                                                            | Play **symbol removal animation** for Row 2, similar to Row 3. Symbols **vanish or explode** with a sound effect indicating they are cleared.                                                                  |
| 6    | **First Cascade**              | Symbols from **Row 1** (top row) cascade down to fill the empty spaces in **Row 2** and **Row 3** (since Row 2 and Row 3 have been cleared).                                                                                                      | Animate the symbols **falling or sliding down** from Row 1 to fill the spaces in Row 2 and Row 3. Use a **tumbling or rolling animation** with a sound effect for cascading.                                    |
| 7    | **Check for New Wins**         | The game checks if the newly cascaded symbols in Rows 2 and 3 form any additional winning combinations.                                                                                                                                        | **Re-evaluate** the grid for new winning combinations, and **highlight any new wins** with glow animations. Play a sound effect for new wins if detected.                                                       |
| 8    | **End of Cascade**             | Once no new winning combinations are found in Rows 2 and 3 after the cascades, the cascading process stops. The game now evaluates the final positions of all symbols.                                                                          | Play a **soft settling animation** for the final cascade. Symbols may **bounce or gently wobble** to indicate they have settled in their final positions.                                                        |
| 9    | **Payout Calculation**         | The game calculates the payout for the player, combining the results of both the **Row 3** and **Row 2** paylines.                                                                                                                             | Trigger a **celebratory animation**, such as **flashing lights, coin showers, or a payout display**. Accompany this with celebratory sound effects.                                                             |
| 10   | **Return to Idle**             | After the payout is calculated and displayed, the game resets to the **Waiting** state, ready for the next spin.                                                                                                                                | Show **idle animations** (e.g., **button highlights, background effects**) as the game returns to its idle state.                                                                                               |

---

## Summary of Group-Based Animation & Removal

- **Winning Symbols Animation (Row 3)**: Only the **Row 3** symbols (bottom-most row) play their winning animation first.
  
- **Winning Symbols Animation (Row 2)**: After the **Row 3** symbols are removed, **Row 2** winning symbols play their success animation before being removed.

- **Symbol Removal**: Winning symbols are removed in sequence (Row 3 first, then Row 2). After removal, **non-winning symbols** cascade down from Row 1.

- **First Cascade**: After both rows are cleared, the symbols from **Row 1** will cascade down to fill the spaces in **Row 2** and **Row 3**.

This **group-based animation and removal** approach ensures clarity in the cascading process and gives players a better visual experience of how winning symbols are handled.
