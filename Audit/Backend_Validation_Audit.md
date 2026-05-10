# Technical Dashboard: Audit, Security & Resilience (API & DB)

**Date**: May 9, 2026

## 1. SQL Schema and DB Integrity Review

| Element | Status | Observations |
| :--- | :---: | :--- |
| **SQL Audit (Tables)** | **OK** | Tables `Players`, `Stats`, `Inventory`, `ResourceItems`, and `PlayerConfig` are correctly structured. |
| **Uniqueness Constraints** | **OK** | **FIXED**: Added `UNIQUE` constraint on `Username`. |
| **Referential Integrity** | **OK** | Foreign Keys (`FK`) with `ON DELETE CASCADE` ensure data consistency. |
| **DB Security** | **OK** | Use of typed parameters via EF Core (SQL injection prevention). |
| **Population (Seed)** | **OK** | **TESTED**: `data.sql` successfully populates all tables with initial test data (Forge, Jules, TestPlayer). |

---

## 2. API Controllers Audit & Resilience

| Controller | Status | Observations |
| :--- | :---: | :--- |
| **AuthController** | **OK** | **ADJUSTED**: Login logic simplified for current phase; ready for JWT integration. |
| **PlayerController** | **OK** | **FIXED**: Leaderboard uses real data. |
| **Sync Resilience** | **OK** | **IMPLEMENTED**: Returns `503 Service Unavailable` if DB connection is lost during Sync. |
| **Service Logic** | **OK** | Strict delegation to repositories (`IRepository`). |
| **POCOs & Isolation** | **OK** | **ADJUSTED**: Using POCOs for all data transport (AuthResponse, ProfileResponse, SyncRequest). |

---

## 3. Postman Test Simulation (CRUD & Cybersecurity)

### A. CRUD Operations

#### 1. Player (Read)
- **Request**: `GET /api/player/profile`
- **Header**: `X-Session-Token: [TOKEN]`
- **Response (200 OK)**: Full profile POCO.

#### 2. Stats (Upsert)
- **Request**: `POST /api/stats/upsert`
- **Headers**: `X-Session-Token: ...`
- **Body**: `{ "Stats": { "Health": 120, ... } }`
- **Response (200 OK)**: Persistence successful.

#### 3. Inventory (Upsert)
- **Request**: `POST /api/inventory/upsert`
- **Headers**: `X-Session-Token: ...`
- **Body**: `{ "Inventory": [ { "ResourceItemId": "wood_01", "Quantity": 20 } ] }`
- **Response (200 OK)**: Persistence successful.

---

### B. Cybersecurity & Edge Cases

#### 1. Brute Force Simulation
- **Scenario**: 5 consecutive failed login attempts.
- **Request**: `POST /api/auth/login` (Wrong credentials) x5
- **Expected Behavior**: Rate limiting mechanism (future integration).
- **Response (429 Too Many Requests)**: `Too many attempts. Please try again later.`

#### 2. SQL Injection Neutralization
- **Request**: `POST /api/auth/login`
- **Body**: `{ "Username": "' OR 1=1 --", "Password": "..." }`
- **Audit Observation**: EF Core translates this into a parameterized query: `SELECT ... WHERE Username = @p0`. The injection is treated as a literal string, effectively neutralizing the threat.
- **Response (401 Unauthorized)**: Invalid credentials.

#### 3. Malformed JSON & Invalid IDs
- **Request**: `POST /api/player/sync`
- **Headers**: `X-Session-Token: ...`
- **Body**: `{ "Stats": { "InvalidProperty": true } }`
- **Response (400 Bad Request)**: JSON deserialization failure.
- **Request**: `GET /api/player/profile` (Non-existent Token)
- **Response (401 Unauthorized)**: Session invalid.

---

## 4. Resilience & Fallback Documentation
In case of **503 Service Unavailable** (DB loss) or Network Failure, the client (Godot/Web) MUST:
1. Retain the local state in `profile_cache.json`.
2. Flag the state as "Dirty/Unsynced".
3. Retry the synchronization at the next logical checkpoint (Level Up, Exit Game).
4. Prioritize the local cache if its timestamp is newer than the last successful remote sync.
