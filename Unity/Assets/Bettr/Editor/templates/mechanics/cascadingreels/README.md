# Cascading Reels Mechanic with Game and Reel States

## Cascading Reels Overview

Cascading Reels is a slot machine mechanic where winning symbols disappear from the reels, allowing new symbols to fall into place. This creates the possibility of multiple wins on a single spin as new combinations are formed. Cascading Reels is also known as Avalanche, Tumbling, or Rolling Reels, depending on the game developer.

## Key Features

- **Symbol Removal**: After a winning combination is formed, the winning symbols are removed from the reels, leaving empty spaces for new symbols to cascade down and fill the gaps.
- **Chain Reactions**: With each cascade, new symbols fall into place, potentially creating new winning combinations. This process can continue as long as new wins are formed, allowing for multiple consecutive wins from a single spin.
- **No New Spin Required**: Cascading Reels operate within a single spin. As long as new winning combinations keep forming, the cascades will continue without requiring another spin or bet.
- **Multiplier Feature**: Some Cascading Reels games include a multiplier feature, where each successive cascade increases the multiplier, enhancing the payout for any subsequent wins in the chain reaction.

## How It Works

### 1. **Initial Base Game States**

The base game transitions between the following states:

- **Waiting**: The game waits for player input, such as pressing the spin button.
- **Spinning**: The game transitions to this state after the player initiates the spin, triggering the reels to start spinning.
- **Completed**: Once all cascades and evaluations are done, the base game enters the completed state, finalizing the spin.

### 2. **Reel Spin States**

Each reel transitions between the following states:

- **Waiting**: The reel is idle, waiting to be triggered for spinning.
- **Spinning**: The reel is actively spinning after being triggered by the base game.
- **Stopped**: The reel has stopped spinning and is ready for the next evaluation.
- **ReachedOutcomeStopIndex**: The reel has reached its pre-defined outcome, indicating the stop position for the spin.
- **SpinStartedRollBack**: A transitional state where the reel starts spinning in reverse to add visual polish to the animation.
- **SpinStartedRollForward**: A transitional state where the reel starts spinning forward to add visual polish.
- **SpinEndingRollForward**: The reel ends its spin with a forward roll effect.
- **SpinEndingRollBack**: The reel ends its spin with a backward roll effect.

### 3. **Cascading Reels Process**

The cascading reels mechanic integrates with the base game and reel spin states as follows:

#### Initial Spin

1. The player initiates the spin while the base game is in the **Waiting** state. This triggers a transition to the **Spinning** state.
   
2. Each reel moves from **Waiting** to **Spinning**, where the reels spin using intermediate states like **SpinStartedRollBack** or **SpinStartedRollForward** for smooth animations.

3. Once the reels stop, each reel enters the **Stopped** state and then the **ReachedOutcomeStopIndex** state to confirm its outcome.

#### Winning Combination

4. After the reels stop, the base game evaluates the symbols. If a winning combination is found, the base game transitions to the cascading phase while remaining in the **Spinning** state. 

5. Winning symbols are removed at the symbol level, leaving empty spaces. The reels may transition to **SpinStartedRollForward** or **SpinStartedRollBack** to handle the cascading animation.

#### Cascading Mechanic

6. New symbols fall into the empty spaces left by the removed winning symbols. This action can happen multiple times as long as new winning combinations are formed.

7. Each time the symbols cascade, the reels may briefly return to the **Spinning** state as they "simulate" additional spins, moving through **SpinStartedRollBack/Forward** and **SpinEndingRollBack/Forward** states during these transitions.

8. If no new winning combinations are formed after a cascade, the reels enter the **Stopped** state, finalizing their actions.

#### End of Cascade

9. Once all cascades are completed and no further wins are detected, the base game transitions from **Spinning** to **Completed**, signaling the end of the spin cycle.

#### Payout and Reset

10. After entering the **Completed** state, the game calculates and awards payouts based on the winning combinations formed during the spin and cascade cycles. The base game then returns to the **Waiting** state, ready for the next spin.

## Example Flow with States

1. **Initial State**: 
   - Base Game in **Waiting**
   - Reels in **Waiting**

2. **Spin Begins**: 
   - Base Game transitions to **Spinning**
   - Reels transition to **Spinning** and animate using **SpinStartedRollForward**

3. **Reels Stop**:
   - Reels enter **Stopped** and then **ReachedOutcomeStopIndex** for outcome evaluation

4. **Cascading**: 
   - Winning symbols are removed at the symbol level
   - Reels simulate cascading symbols using **SpinStartedRollBack** or **SpinStartedRollForward**

5. **End of Cascade**: 
   - Reels enter **Stopped**, Base Game moves to **Completed**, and payouts are calculated

6. **Return to Idle**: 
   - Base Game returns to **Waiting** for the next spin

## Conclusion

This updated version of the **Cascading Reels Mechanic** integrates the base game and reel state machines, allowing for precise control over each phase of the game. The dynamic reel states like **SpinStartedRollBack** and **SpinEndingRollForward** allow for smooth transitions and animations during cascades, creating an engaging and visually polished experience. With the inclusion of cascading mechanics, the game remains in the **Spinning** state until no further wins are detected, ensuring multiple chances for consecutive wins within a single spin cycle.
