# Orchestration Example for Synced Reels Mechanic

This section provides an example of how the various events in the **Synced Reels** mechanic are orchestrated during a slot game. It demonstrates the flow from the start of the spin to the calculation of the final payout, with synced reels being activated randomly during the spin.

---

## Scenario:
- **Grid Configuration**: 5 reels, each with 3 rows.
- **Synced Reels**:
  - **Reels 2 and 4** are randomly synced during the spin and display identical symbols when they stop.

---

## Orchestration for Synced Reels in a 5x3 Grid

| Step | Event                            | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | Function |
|------|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Initial Spin**                 | The player presses the spin button, and all reels start spinning as part of the base game.                                                                                                                                                        | All reels begin the normal spin animation.                                                                                                                               | `OnSpinButtonClicked()` + `SpinEngines()` |
| 2    | **Random Synced Reels Activation During Spin** | The game randomly triggers the **Synced Reels Activation** event mid-spin. Reels 2 and 4 are selected to sync, showing identical symbols when they stop.                                                                                           | Reels 2 and 4 light up with a special glow effect to indicate that they will sync. These reels will stop together and show the same symbols due to identical reel strips.                                        | `TriggerSyncedReelsRandomly(reels)` + `ActivateSyncedReels(reels)` |
| 3    | **Reels Stop with Identical Symbols on Synced Reels** | Reels 2 and 4 stop at the same time and display identical symbols, increasing the chances of forming winning combinations.                                                                                                                          | Reels 2 and 4 stop spinning together, displaying matching symbols. The matching symbols are highlighted with a glowing or pulsing effect.                                                                        | `DisplaySyncedSymbols(reels)` |
| 4    | **Payout Calculation**           | The game calculates payouts based on matching symbols on the synced reels and any other winning paylines.                                                                                                                                         | Payouts are calculated based on the winning combinations formed with the synced reels. A celebratory sound or animation highlights the player’s winnings.                                                        | `CalculateAndDisplayPayout()` |
| 5    | **Bonus Feature Trigger (Optional)** | The player may trigger a bonus feature if scatter symbols or other bonus triggers land on the synced or other reels.                                                                                                                                  | If a bonus round is triggered (e.g., free spins), a special animation or effect highlights the transition to the bonus round, with guaranteed synced reels in the upcoming spins.                                | `BonusRoundTriggered()` (if applicable) |

---

## Function Headers for New Functions

- `OnSpinButtonClicked()`: Handles the player's spin button click event, starting the reels.
- `SpinEngines()`: Spins all the reels as part of the base game.
- `TriggerSyncedReelsRandomly(reels)`: Randomly selects reels during the spin to activate the Synced Reels feature.
- `ActivateSyncedReels(reels)`: Highlights synced reels during the spin, applies special visual effects, and ensures identical reel strips for those reels.
- `DisplaySyncedSymbols(reels)`: Displays identical symbols on the synced reels when they stop spinning.
- `CalculateAndDisplayPayout()`: Calculates the final payout and triggers a celebratory display for the player.
- `BonusRoundTriggered()`: Handles transitions to a bonus round, such as free spins (if triggered).

---

### Orchestration Overview

The orchestration of the **Synced Reels** mechanic involves several interconnected events:

1. **Initial Spin**: The player initiates the spin, and all reels begin spinning.
2. **Random Synced Reels Activation**: During the spin, two or more reels are randomly highlighted to sync, and they will display identical symbols when they stop.
3. **Reels Stop**: The synced reels stop at the same time and display matching symbols.
4. **Payout Calculation**: The game calculates the payouts based on the winning combinations formed by the synced reels and other reels.
5. **Bonus Feature Trigger (Optional)**: A bonus feature may be triggered, leading to additional spins with synced reels.

---

### Example Timeline

1. **Spin -> Reels Start Spinning**
2. **Mid-Spin Activation -> Reels 2 and 4 Sync**
3. **Reels Stop -> Synced Reels Show Matching Symbols**
4. **Payout Calculation -> Winning Combinations Evaluated**
5. **(Optional) Bonus Feature Triggered -> Free Spins with Synced Reels**

---

### Conclusion

This example illustrates the orchestration of the **Synced Reels** mechanic in a slot game, showing the random activation of synced reels during a spin and the display of identical symbols. The synced reels increase the chances of landing winning combinations, and the potential for triggering bonus features adds excitement to each spin.
