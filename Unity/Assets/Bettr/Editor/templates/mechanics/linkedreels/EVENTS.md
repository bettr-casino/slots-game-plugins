# EVENTS.md

## Events Related to Linked Reels Mechanic

### 1. **Event: Linked Reels Triggered by Wilds**
   - **Trigger**: This event occurs when two or more adjacent Wild symbols land on the reels, triggering the Linked Reels mechanic.
   - **Description**: When two or more adjacent Wild symbols appear on the reels during a base game spin, the affected reels become linked for the next spin. These reels will spin together, showing identical or nearly identical symbols, increasing the player's chance of landing winning combinations.
   - **Outcome**: 
     - The adjacent reels where Wilds landed become linked for the next spin, displaying identical symbols.
     - Increased chances of forming winning combinations on the next spin.
   - **Properties**:
     - `triggering_reels`: The reels on which Wilds landed (e.g., Reel 2 and Reel 3).
     - `trigger_time`: Timestamp when the event is triggered.
     - `trigger_symbol`: Symbol responsible for triggering the event (e.g., Wild).
     - `linked_reels_count`: Number of reels that will be linked (based on where the Wilds land).
   - **Example**: Two Wild symbols land on Reels 2 and 3 during a base game spin. After the payout, Reels 2 and 3 will be linked and spin together in the next spin, displaying identical symbols.

### 2. **Event: Linked Reels Spin**
   - **Trigger**: Occurs on the spin after the Linked Reels mechanic is activated.
   - **Description**: The reels that were linked by landing adjacent Wilds now spin together, displaying identical symbols when they stop.
   - **Outcome**: 
     - Linked reels display identical symbols, increasing the chances of forming winning combinations across paylines.
     - Potential for higher payouts depending on the symbols displayed.
   - **Properties**:
     - `linked_reels`: The reels that are currently linked.
     - `spin_time`: Timestamp when the linked reels spin occurs.
     - `symbols_shown`: The symbols displayed on the linked reels after they stop spinning.
   - **Example**: Reels 2 and 3 spin together after being linked and show identical symbols (e.g., Wild, Cherry, 7), leading to higher chances of forming winning combinations.

### 3. **Event: Linked Reels Payout After Spin**
   - **Trigger**: Occurs after the linked reels have spun and stopped, and the game evaluates the winning combinations.
   - **Description**: The game evaluates the symbols displayed on the linked reels after they stop spinning and calculates any resulting payouts.
   - **Outcome**:
     - Winning combinations are evaluated based on the identical symbols displayed on the linked reels.
     - Winnings are awarded to the player.
   - **Properties**:
     - `payout_value`: The total payout for the spin with linked reels.
     - `winning_combinations`: A list of the winning combinations formed by the linked reels.
     - `final_symbols`: The final symbols displayed on the linked reels when they stop.
   - **Example**: After Reels 2 and 3 are linked and spin together, the game evaluates the payout based on the matching symbols, and the player is awarded a payout of 500 credits.

### 4. **Event: Linked Reels Deactivation**
   - **Trigger**: The Linked Reels mechanic deactivates after the linked reels spin, and regular gameplay resumes.
   - **Description**: Once the linked reels spin and the payout is evaluated, the linked reels return to normal, independent behavior in subsequent spins.
   - **Outcome**:
     - The linked reels mechanic is reset, and all reels return to spinning independently.
     - The next spin will not have linked reels unless the mechanic is triggered again.
   - **Properties**:
     - `deactivation_time`: Timestamp when the linked reels mechanic deactivates.
     - `final_winnings`: Total payout earned during the linked reels feature.
   - **Example**: After Reels 2 and 3 are linked and spin together, the mechanic deactivates, and the reels return to normal for the next spin.

### 5. **Event: Linked Reels Expansion During Bonus**
   - **Trigger**: Occurs when more reels become linked, typically during a bonus round or free spins feature.
   - **Description**: Additional reels become linked during a bonus round, increasing the number of identical symbols across the reels.
   - **Outcome**:
     - Greater chances of landing matching symbols across more reels.
     - Increased win potential due to more linked reels.
   - **Properties**:
     - `additional_reels_linked`: Number of extra reels added to the linked set.
     - `total_linked_reels`: Total number of reels currently linked.
     - `expansion_time`: Timestamp when the additional reels are linked.
   - **Example**: During a free spins round, Reels 2, 3, and 4 become linked, greatly enhancing the chance of landing matching high-value symbols for a big payout.

### 6. **Event: Massive Win (Triggered by Linked Reels)**
   - **Trigger**: Occurs when a large payout is achieved due to the Linked Reels mechanic, typically from high-value symbols aligning across several paylines.
   - **Description**: A large win is triggered when linked reels display the same high-value or wild symbols across multiple paylines, resulting in a significant payout.
   - **Outcome**:
     - A massive win is awarded to the player.
     - Visual and auditory celebration occurs in-game (e.g., animations, sound effects).
   - **Properties**:
     - `massive_win_value`: Total amount of the massive win.
     - `winning_symbol`: The symbol(s) responsible for the massive win.
     - `linked_reels_active`: Indicates whether linked reels were active during the win.
   - **Example**: Reels 2 and 3 are linked, showing five matching wild symbols across multiple paylines, triggering a massive win of 10,000 credits.
