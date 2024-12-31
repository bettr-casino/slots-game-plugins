# EVENTS.md

## Events Related to Locked Reels Mechanic

### 1. **Event: Locked Reels Triggered by Special Symbol**
   - **Trigger**: This event occurs when the special symbol lands on Reel 1, activating the Locked Reels mechanic.
   - **Description**: When the special symbol appears on Reel 1, adjacent reels (starting from Reel 2) become locked together for the next spin, causing them to spin in sync and display identical symbols.
   - **Outcome**: 
     - Adjacent reels to Reel 1 are locked for the next spin.
     - Increased chance of forming winning combinations due to identical symbols across locked reels.
   - **Properties**:
     - `triggering_symbol`: The special symbol that lands on Reel 1, triggering the feature.
     - `trigger_time`: Timestamp when the event is triggered.
     - `locked_reels_count`: Number of reels that will be locked for the next spin.
   - **Example**: A special symbol lands on Reel 1, locking Reels 2 and 3 for the next spin. These reels will spin together and display identical symbols.

### 2. **Event: Locked Reels Spin**
   - **Trigger**: Occurs on the spin after the Locked Reels mechanic is triggered.
   - **Description**: The adjacent reels that were locked by the special symbol now spin in sync, displaying identical symbols when they stop.
   - **Outcome**: 
     - Locked reels display identical symbols, increasing the chances of forming winning combinations across paylines.
     - Potential for higher payouts depending on the symbols displayed.
   - **Properties**:
     - `locked_reels`: The reels that are currently locked and spinning together.
     - `spin_time`: Timestamp when the locked reels spin occurs.
     - `symbols_shown`: The symbols displayed on the locked reels after they stop spinning.
   - **Example**: Reels 2 and 3 are locked and spin together, displaying identical symbols (e.g., Wild, Wild, 7), leading to higher chances of forming winning combinations.

### 3. **Event: Locked Reels Payout After Spin**
   - **Trigger**: Occurs after the locked reels have spun and stopped, and the game evaluates the winning combinations.
   - **Description**: The game evaluates the symbols displayed on the locked reels after they stop spinning and calculates any resulting payouts.
   - **Outcome**:
     - Winning combinations are evaluated based on the identical symbols displayed on the locked reels.
     - Winnings are awarded to the player.
   - **Properties**:
     - `payout_value`: The total payout for the spin with locked reels.
     - `winning_combinations`: A list of the winning combinations formed by the locked reels.
     - `final_symbols`: The final symbols displayed on the locked reels when they stop.
   - **Example**: After Reels 2 and 3 are locked and spin together, the game evaluates the payout based on the matching symbols, and the player is awarded a payout of 1,000 credits.

### 4. **Event: Locked Reels Deactivation**
   - **Trigger**: The Locked Reels mechanic deactivates after the locked reels spin, and regular gameplay resumes.
   - **Description**: Once the locked reels spin and the payout is evaluated, the locked reels return to normal, independent behavior in subsequent spins.
   - **Outcome**:
     - The locked reels mechanic is reset, and all reels return to spinning independently.
     - The next spin will not have locked reels unless the mechanic is triggered again.
   - **Properties**:
     - `deactivation_time`: Timestamp when the locked reels mechanic deactivates.
     - `final_winnings`: Total payout earned during the locked reels feature.
   - **Example**: After Reels 2 and 3 are locked and spin together, the mechanic deactivates, and the reels return to normal for the next spin.

### 5. **Event: Massive Win (Triggered by Locked Reels)**
   - **Trigger**: Occurs when a large payout is achieved due to the Locked Reels mechanic, typically from high-value symbols aligning across several paylines.
   - **Description**: A large win is triggered when locked reels display the same high-value or wild symbols across multiple paylines, resulting in a significant payout.
   - **Outcome**:
     - A massive win is awarded to the player.
     - Visual and auditory celebration occurs in-game (e.g., animations, sound effects).
   - **Properties**:
     - `massive_win_value`: Total amount of the massive win.
     - `winning_symbol`: The symbol(s) responsible for the massive win.
     - `locked_reels_active`: Indicates whether locked reels were active during the win.
   - **Example**: Reels 2 and 3 are locked, showing five matching wild symbols across multiple paylines, triggering a massive win of 10,000 credits.
