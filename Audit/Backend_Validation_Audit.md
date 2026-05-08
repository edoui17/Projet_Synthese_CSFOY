# Technical Dashboard: Audit and Validation (API & DB)

## 1. SQL Schema and DB Integrity Review

| Element | Status | Observations |
| :--- | :---: | :--- |
| **SQL Audit (Tables)** | **OK** | Tables `Players`, `Stats`, `Inventory`, `ResourceItems`, and `PlayerConfig` are correctly structured. Proper use of `UNIQUEIDENTIFIER` for IDs. |
| **Uniqueness Constraints** | **OK** | **FIXED**: Added `UNIQUE` constraint on `Username` in `schema.sql` and `AppDbContext` to prevent duplicates. |
| **Referential Integrity** | **OK** | Foreign Keys (`FK`) with `ON DELETE CASCADE` ensure data consistency when a player is deleted. |
| **DB Security** | **OK** | Use of typed parameters via EF Core (SQL injection prevention). |
| **Schema Sync** | **OK** | **FIXED**: Updated `schema.sql` to include `PasswordHash` and `SessionToken` to match C# entities. |

---

## 2. API Controllers Audit

| Controller | Status | Observations |
| :--- | :---: | :--- |
| **AuthController** | **OK** | **FIXED**: Use of `AuthResponse` DTO to isolate the `Player` entity and never expose the `PasswordHash`. |
| **PlayerController** | **OK** | **FIXED**: Cleaned up the `leaderboard` endpoint (real data instead of simulated). Use of `ProfileResponse`. |
| **Stats/Inventory** | **OK** | `upsert` endpoints correctly use `SessionToken` to securely identify the player. |
| **Service Logic** | **OK** | Strict delegation to repositories (`IRepository`). Controllers contain no complex business logic. |
| **DTOs & Isolation** | **OK** | **FIXED**: Created `AuthResponse` and `ProfileResponse`. DB entities are strictly confined to the Infrastructure layer. |

---

## 3. Postman Test Simulation (Integration Testing)

### A. Authentication (Login)
- **Request**: `POST /api/auth/login`
- **Body**: `{ "Username": "Forge", "Password": "password123" }`
- **Response (Success - 200 OK)**:
  ```json
  {
    "sessionToken": "d47e8b6b-...",
    "username": "Forge"
  }
  ```
- **Response (Failure - 401 Unauthorized)**: `Invalid username or password.` (No sensitive info leak).

### B. Profile Retrieval
- **Request**: `GET /api/player/profile`
- **Header**: `X-Session-Token: d47e8b6b-...`
- **Response (Success - 200 OK)**:
  ```json
  {
    "username": "Forge",
    "stats": { "health": 100, "attack": 10, ... },
    "config": { "masterVolume": 0.8, ... },
    "inventory": [ { "resourceItemId": "wood_01", "quantity": 10 } ]
  }
  ```

### C. Global Synchronization (Sync)
- **Request**: `POST /api/player/sync`
- **Body**:
  ```json
  {
    "sessionToken": "d47e8b6b-...",
    "stats": { "health": 95, "attack": 12 },
    "inventory": [ { "resourceItemId": "wood_01", "quantity": 15 } ]
  }
  ```
- **Response (Success - 200 OK)**: (Empty status)

### D. Failure Cases (Validation)
- **Request**: `POST /api/stats/upsert` (Without token)
- **Response (Failure - 401 Unauthorized)**: (Empty)
- **Request**: `POST /api/auth/login` (Empty body)
- **Response (Failure - 400 Bad Request)**: `Username and password are required.`
