# Future User Stories

## ATH (HUD) Reactive Implementation
**Description:** As a player, I want to see my current resource counts on the screen (ATH/HUD) so that I know what I can afford to buy at the base.
**Acceptance Criteria:**
- The ATH correctly initializes with the current inventory count.
- The ATH listens to the `ResourceSpent` signal to decrease visually.
- The ATH listens to the `MaterialDestroyed` signal to increase visually.
- The base map or main scene should instantiate the ATH.

## Unlock New Island Visualization
**Description:** As a player, I want to see a visual change or a new level load when I purchase the new island.
**Acceptance Criteria:**
- In `InteractionScript.cs`, when `TryPurchaseIsland` is successful, emit a new signal `IslandPurchased`.
- A system listens to this signal to trigger the scene transition or map expansion.
