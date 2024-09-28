# ORCHESTRATION_EXAMPLE.md

## Orchestration Example for Synced Reels Mechanic

This section provides an example of how the various events in the **Synced Reels** mechanic are orchestrated during a slot game. It demonstrates the flow from the start of the spin to the calculation of the final payout, with synced reels being activated randomly during the spin.

### Step-by-Step Orchestration

### 1. **Initial Spin**

- **Event**: Player presses the "Spin" button to initiate the spin.
- **Action**: The reels begin spinning normally as part of the base game.
- **Outcome**: All reels are spinning, and the player awaits the result.

  **Example**: The player starts a new spin. Reels 1, 2, 3, 4, and 5 all begin spinning at the same time.

### 2. **Random Synced Reels Activation During Spin**

- **Event**: The game randomly triggers the **Synced Reels Activation** event during the spin.
- **Action**: Mid-spin, reels 2 and 4 light up or are highlighted with a special visual effect, indicating that these reels will sync together.
- **Outcome**: Reels 2 and 4 will stop on identical symbols when the reels finish spinning, as they are pulling symbols from an identical reel strip.

  **Example**: During the spin, reels 2 and 4 suddenly glow, signaling that they will sync. The player anticipates the outcome as the reels continue spinning.

### 3. **Reels Stop with Identical Symbols on Synced Reels**

- **Event**: The reels stop, and the **Synced Reels Stop** event is triggered.
- **Action**: Reels 2 and 4 stop at the same time, displaying identical symbols due to the use of identical reel strips.
- **Outcome**: The synced reels now show matching symbols, increasing the chances of forming winning combinations across multiple paylines.

  **Example**: When the spin ends, reels 2 and 4 both stop on a row of high-value symbols, such as wilds or matching icons. The synced symbols match those on other reels, forming a winning combination.

### 4. **Payout Calculation**

- **Event**: The game calculates the payout based on the matching symbols across the synced reels and any other winning paylines.
- **Action**: The game checks for all possible winning combinations involving the synced reels and other reels.
- **Outcome**: The player is awarded payouts based on the matching symbols from the synced reels and any additional winning combinations.

  **Example**: The synced reels create a winning combination across multiple paylines. The game calculates the total payout, including any extra bonuses for landing wild symbols on the synced reels.

### 5. **Bonus Feature Trigger (Optional)**

- **Event**: The player could also trigger a bonus feature during this spin if scatter symbols or other bonus triggers are present on the synced reels or other reels.
- **Action**: The bonus feature, such as free spins, is activated after the synced reels have completed.
- **Outcome**: The player enters a bonus round with guaranteed synced reels on every spin, increasing win potential further.

  **Example**: Along with the synced reels, the player lands three scatter symbols on reels 1, 3, and 5, triggering the free spins bonus round where synced reels are active for every spin.

### Orchestration Overview

The orchestration of the **Synced Reels** mechanic involves several interconnected events:

1. **Initial Spin**: The player initiates the spin, and all reels begin spinning.
2. **Random Synced Reels Activation**: While the reels are spinning, two or more reels are randomly highlighted to sync, and these reels will display identical symbols when they stop.
3. **Reels Stop**: The synced reels stop at the same time, displaying matching symbols.
4. **Payout Calculation**: The game calculates the payouts based on the winning combinations formed by the synced reels.
5. **Bonus Feature Trigger (Optional)**: The player may also trigger a bonus feature during the same spin, leading to additional spins with synced reels.

### Example Timeline

1. Spin -> Reels Start Spinning
2. Mid-Spin Activation -> Reels 2 and 4 Sync
3. Reels Stop -> Synced Reels Show Matching Symbols
4. Payout Calculation -> Winning Combinations Evaluated
5. (Optional) Bonus Feature Triggered -> Free Spins with Synced Reels

### Conclusion

This example illustrates the orchestration of the **Synced Reels** mechanic in a slot game, highlighting the random activation during a spin and the display of identical symbols from an identical reel strip. The synced reels increase the chances of landing winning combinations, and the potential for triggering additional bonus features adds an extra layer of excitement to each spin.
