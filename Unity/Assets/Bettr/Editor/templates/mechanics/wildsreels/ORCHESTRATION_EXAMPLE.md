# Orchestration Example for Wilds Reels Mechanic with Trigger Condition

This section provides an example of how the various events in the **Wilds Reels** mechanic are orchestrated during a slot game. It demonstrates the flow from the start of the spin to the final payout calculation, with Wilds Reels being activated through a trigger condition.

---

## Scenario:
- **Grid Configuration**: 5 reels, each with 3 rows.
- **Wild Reels**:
  - **Reels 3 and 5** are turned wild during the spin due to a trigger condition, dramatically increasing the chances of winning combinations.

---

## Orchestration for Wild Reels in a 5x3 Grid

| Step | Event                            | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | Function |
|------|----------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Initial Spin**                 | The player presses the spin button, and all reels start spinning as part of the base game.                                                                                                                                                        | All reels begin the normal spin animation.                                                                                                                               | `OnSpinButtonClicked()` + `SpinEngines()` |
| 2    | **Trigger Condition for Wild Reels Activation** | The game meets the condition (e.g., stacked wilds or landing a specific symbol) that triggers the Wild Reels feature during the spin.                                                                                                           | A special visual cue highlights reels 3 and 5 with glowing outlines and flickering wild symbols, signaling that these reels will turn wild.                                                                    | `CheckTriggerCondition()` + `ActivateWildReels(reels)` |
| 3    | **Wild Reels Transformation**     | Reels 3 and 5 are fully transformed into wild reels, substituting all symbols on those reels with wilds.                                                                                                                                          | A burst of lightning or energy sweeps down the reels, transforming them into fully wild reels with glowing, pulsing wild symbols.                                                                                | `TransformReelsToWild(reels)` |
| 4    | **Payout Calculation**           | The game calculates payouts based on the wild reels and any other winning paylines formed as a result of the wild transformation.                                                                                                                 | Payouts are calculated based on the new winning combinations formed by the wild reels. A celebratory sound or animation highlights the player’s winnings.                                                        | `CalculateAndDisplayPayout()` |
| 5    | **Bonus Feature Trigger (Optional)** | A bonus feature (e.g., free spins) may be triggered if scatter symbols or other triggers appear during the spin, possibly with guaranteed wild reels in future spins.                                                                               | If a bonus round is triggered, a special animation or transition effect highlights the move to the bonus round, which may feature guaranteed wild reels on every spin.                                            | `BonusRoundTriggered()` (if applicable) |

---

## Function Headers for New Functions

- `OnSpinButtonClicked()`: Handles the player's spin button click event, starting the reels.
- `SpinEngines()`: Spins all the reels as part of the base game.
- `CheckTriggerCondition()`: Evaluates the condition (such as stacked wild symbols or landing certain symbols) to activate the Wild Reels feature.
- `ActivateWildReels(reels)`: Highlights the reels that will turn wild, applying special visual effects and marking them for transformation.
- `TransformReelsToWild(reels)`: Transforms the designated reels into wild reels, replacing all symbols on those reels with wild symbols.
- `CalculateAndDisplayPayout()`: Calculates the final payout based on the new winning combinations and displays the result with celebratory animations.
- `BonusRoundTriggered()`: Handles transitions to a bonus round (if triggered), such as free spins, with possible guaranteed wild reels.

---

### Orchestration Overview

The orchestration of the **Wilds Reels** mechanic involves several interconnected events:

1. **Initial Spin**: The player initiates the spin, and all reels begin spinning.
2. **Trigger Condition Activation**: A specific condition (e.g., stacked wilds or a specific symbol) triggers the Wilds Reels feature.
3. **Wild Reels Transformation**: The selected reels are transformed into fully wild reels, enhancing the chances of forming winning combinations.
4. **Payout Calculation**: The game calculates the payouts based on the new wild reels and any other winning combinations.
5. **Bonus Feature Trigger (Optional)**: A bonus feature may be triggered, leading to additional spins with guaranteed wild reels.

---

### Example Timeline

1. **Spin -> Reels Start Spinning**
2. **Trigger Condition -> Reels 3 and 5 Highlighted for Wild Transformation**
3. **Reels Stop -> Reels 3 and 5 Turn Wild**
4. **Payout Calculation -> Winning Combinations Evaluated**
5. **(Optional) Bonus Feature Triggered -> Free Spins with Wild Reels**

---

### Conclusion

This example illustrates the orchestration of the **Wilds Reels** mechanic in a slot game, showing how wild reels are activated by a trigger condition and how they transform to increase the player’s chances of landing winning combinations. The potential for triggering bonus features adds further excitement and engagement to the gameplay.
