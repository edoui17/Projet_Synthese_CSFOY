1. **Fix ProgressionManager to correctly award XP on resource harvest**
   - In `ProgressionManager.cs`, subscribe to `MaterialDestroyedEvent` instead of `ResourceHarvestedEvent`.
   - Implement `OnMaterialDestroyedEvent(MaterialDestroyedEvent p_event)` to call `StatTracker.AddExperience(XpFromHarvesting)`.

2. **Fix ProgressionManager to correctly award XP on island exploration (Portal Use)**
   - In `ProgressionManager.cs`, subscribe to `TeleportRequestedEvent` instead of `NavigationRequestedEvent`.
   - Implement `OnTeleportRequestedEvent(TeleportRequestedEvent p_event)` to call `StatTracker.AddExperience(XpFromNewIsland)` if it's not the home island.

3. **Fix AttributeStat logic for tracking Experience and Level values**
   - The reason level progression isn't saving/updating properly is that `StatTracker` relies on `SetCurrentValue` to update XP and Level, but `AttributeStat.SetCurrentValue` is currently a no-op empty method.
   - Update `AttributeStat.SetCurrentValue(float p_value)` in `Src/Core/Managers/Stats/AttributeStat.cs` to set `m_baseValue = p_value` and call `NotifyStatChanged()`.

4. **Update `Player.cs` / `HealthBarLvl.cs` connections if needed**
   - I will check `Player.cs` and potentially `HealthBarLvl.cs` to ensure that `LevelChangedEvent` updates the level UI correctly. Player already listens to `LevelChangedEvent`, so fixing the above logic in `AttributeStat` will naturally make the UI update and trigger the "LEVEL UP" label!

5. **Pre Commit Steps**
   - Call `pre_commit_instructions` tool to make sure proper testing, verifications, reviews, and reflections are done.
