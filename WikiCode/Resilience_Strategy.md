# Client-Side Fallback & Resilience Strategy

## 1. Overview
As a Rogue-like game, player progression (inventory, unlocked stats) is critical. To ensure no data loss during network or database failures, a robust fallback mechanism is implemented between the Godot client and the .NET API.

## 2. API Resilience (Server-Side)
The `POST /api/player/sync` endpoint is protected by a try-catch block specifically targeting database connection issues (e.g., `SqlException`).
- **Behavior**: If the database is unreachable, the API returns a `503 Service Unavailable` status with a clear message.
- **Goal**: Inform the client that the request was received but the persistence layer failed, distinguishing it from a 404 (wrong endpoint) or 401 (unauthorized).

## 3. Client Fallback (Godot/Web)
When the client attempts to synchronize data via `ApiService`, it must follow this protocol:

### A. Sync Attempt
1. Serialize current state (Inventory, Stats, Config).
2. Send `POST /api/player/sync`.

### B. Error Handling
- **Success (200 OK)**: Clear the local "dirty" flag. The remote database is now the source of truth.
- **Database Failure (503 Service Unavailable)** or **Network Failure**:
    1. **Maintain Local Cache**: DO NOT overwrite or clear the local `profile_cache.json`.
    2. **Retry Logic**: Queue the sync operation for the next transition (e.g., next level, return to menu).
    3. **User Notification**: (Optional) Display a discreet "Offline Mode - Progress saved locally" indicator.

### C. Conflict Resolution (Session Start)
When the game starts:
1. Try to fetch the profile from `/api/player/profile`.
2. If failed, load from `profile_cache.json`.
3. If both exist, compare timestamps (if available) or prioritize the most advanced state (e.g., more items/stats).

## 4. Implementation Reference
The `ApiService.cs` in `Src/Core/Services/` implements this fallback logic by catching `HttpRequestException` and ensuring that the `ISaveService` (local persistence) is always called as a secondary safety measure.
