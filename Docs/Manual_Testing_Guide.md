# Manual Testing Guide (IslandSurvivor)

**Date**: May 9, 2026

## 1. API & Database Testing (Postman)
Use these instructions to manually verify CRUD operations for the backend.

### Prerequisites (Configuration)
Before testing, each developer must configure their own connection string in `Src/API/appsettings.json`.
- **Server Instance**: Ensure the `Server=` parameter matches your local SQL Server instance (e.g., `Server=MSI`, `Server=.`, or `Server=(localdb)\mssqllocaldb`).
- **Connection Example**: `"DefaultConnection": "Server=MSI;Database=DBIslandSurvivor;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"`

### A. Authentication
- **Endpoint**: `POST /api/auth/login`
- **Headers**: `X-API-KEY: IslandSurvivor-Dev-2026`
- **Body (JSON)**: `{ "Username": "Forge", "Password": "password123" }`
- **Goal**: Retrieve the `sessionToken` for subsequent requests.

### B. Player Profile (Read)
- **Endpoint**: `GET /api/player/profile`
- **Headers**:
    - `X-API-KEY: IslandSurvivor-Dev-2026`
    - `X-Session-Token: [TOKEN]`
- **Goal**: Verify that all linked data (Stats, Inventory, Config) is returned as a single POCO.

### C. Stats Update (Upsert)
- **Endpoint**: `POST /api/stats/upsert`
- **Headers**:
    - `X-API-KEY: IslandSurvivor-Dev-2026`
    - `X-Session-Token: [TOKEN]`
- **Body (JSON)**:
  ```json
  {
    "Stats": { "Health": 200, "Attack": 25, "Speed": 2, "Luck": 5 }
  }
  ```
- **Goal**: Update persistent player stats.

### D. Inventory Update (Upsert)
- **Endpoint**: `POST /api/inventory/upsert`
- **Headers**:
    - `X-API-KEY: IslandSurvivor-Dev-2026`
    - `X-Session-Token: [TOKEN]`
- **Body (JSON)**:
  ```json
  {
    "Inventory": [ { "ResourceItemId": "gold_01", "Quantity": 50 } ]
  }
  ```
- **Goal**: Synchronize inventory state.

---

## 2. End-to-End (E2E) Test Scenario
Follow this step-by-step scenario to validate the full integration between the Web/Client, API, and Database.

### Scenario: "Persistent Progression Sync"
1. **Action**: Open the Blazor Web Dashboard or Godot Client and modify a player statistic (e.g., increase Attack to 50).
2. **Observation**: Trigger a "Save" or "Sync" action.
3. **API Validation**: In the API logs or Postman, verify that the `POST /api/player/sync` or `POST /api/stats/upsert` returns a **200 OK**.
4. **Database Validation**: Open SSMS and run the following query:
   ```sql
   SELECT Health, Attack FROM Stats WHERE PlayerId = (SELECT Id FROM Players WHERE Username = 'YourUsername');
   ```
5. **Confirmation**: Ensure the value in the database exactly matches the value entered in the UI.
6. **Resilience Test**: Simulate a database disconnect. Attempt another sync. Verify that the API returns **503 Service Unavailable** and that the UI retains the data locally for a future retry.
