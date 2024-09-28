# EVENTS.md

## Events Related to Linked Reels Mechanic

### 1. **Event: Linked Reels Activation**
   - **Trigger**: This event occurs when the Linked Reels mechanic is activated.
   - **Description**: The game randomly selects two or more adjacent reels to link, causing them to display identical or nearly identical symbols when spinning. This event may trigger during regular gameplay or as part of a bonus round.
   - **Outcome**: 
     - Increased chances of forming winning combinations due to identical symbols across linked reels.
     - Potentially higher payouts depending on the symbols displayed.
   - **Properties**:
     - `linked_reels_count`: Number of reels that are linked (e.g., 2, 3, 4).
     - `activation_time`: Timestamp when the event is triggered.
     - `trigger_type`: Indicates if the event is triggered during regular gameplay (`base_game`) or a bonus feature (`bonus_round`).
   - **Example**: Reels 2 and 3 are linked during a random base game spin, displaying the same sequence of symbols.

### 2. **Event: Linked Reels Expansion**
   - **Trigger**: Occurs when more reels become linked, typically during a bonus round or free spins feature.
   - **Description**: Additional reels beyond the initial two become linked, increasing the number of identical symbols across the reels.
   - **Outcome**:
     - Greater chances of landing matching symbols across more reels.
     - Dramatically increased win potential.
   - **Properties**:
     - `additional_reels_linked`: Number of extra reels added to the linked set.
     - `total_linked_reels`: Total number of reels currently linked.
     - `expansion_time`: Timestamp when the additional reels are linked.
   - **Example**: Reels 2, 3, 4, and 5 are linked during a free spins round, greatly enhancing the chance of a large payout.

### 3. **Event: Bonus Round with Linked Reels**
   - **Trigger**: Occurs when the Linked Reels mechanic is activated as part of a special feature or bonus round (e.g., free spins or special modes).
   - **Description**: The linked reels become a persistent feature during the entire bonus round, with the potential for more reels to link as the round progresses.
   - **Outcome**:
     - Increased payout potential throughout the bonus round as linked reels continue to display the same symbols.
     - Higher chance of hitting full-screen wins if all reels become linked.
   - **Properties**:
     - `bonus_round_start`: Timestamp indicating the start of the bonus round.
     - `linked_reels_duration`: Duration in which the linked reels mechanic remains active during the bonus round.
     - `final_payout`: Total winnings earned during the linked reels in the bonus round.
   - **Example**: During free spins, reels 2, 3, 4, and 5 are linked, with the possibility of high-value symbols landing on multiple paylines for big payouts.

### 4. **Event: End of Linked Reels Mechanic**
   - **Trigger**: The Linked Reels mechanic deactivates after the spin or round ends.
   - **Description**: The linked reels return to their regular spinning behavior, and the mechanic resets for the next spin or game round.
   - **Outcome**:
     - Regular gameplay resumes without linked reels.
     - Any winnings from the linked reels are evaluated and awarded.
   - **Properties**:
     - `linked_reels_reset_time`: Timestamp when the linked reels mechanic stops.
     - `final_winnings`: Total payout from the linked reels feature.
   - **Example**: After the spin where reels 2 and 3 are linked, the mechanic deactivates, and the reels return to normal for the next spin.

### 5. **Event: Massive Win (Triggered by Linked Reels)**
   - **Trigger**: Occurs when a large payout is achieved due to the Linked Reels mechanic, typically from high-value symbols aligning across several paylines.
   - **Description**: A large win is triggered when linked reels show the same high-value or wild symbols across multiple paylines, leading to a significant payout.
   - **Outcome**:
     - A massive win is awarded to the player.
     - Visual and auditory celebration occurs in-game (e.g., animations, sound effects).
   - **Properties**:
     - `massive_win_value`: Total amount of the massive win.
     - `winning_symbol`: The symbol(s) responsible for the massive win.
     - `linked_reels_active`: Indicates whether linked reels were active during the win.
   - **Example**: Reels 2, 3, and 4 are linked, showing five matching wild symbols across multiple paylines, triggering a massive win of 10,000 credits.
