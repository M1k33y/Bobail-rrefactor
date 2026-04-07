Project TODO / Idei & Next Steps
AI Improvements (Hard Difficulty)
Probleme actuale
Hard AI se mișcă lent
Se fac foarte multe clone de game state
Minimax recalculează aceleași poziții
Nu există move ordering eficient
Soluții propuse
1. Caching pentru funcții
Cache pe baza:
nume funcție + parametri

Exemplu cheie:

functionName + serializedParams
Se poate salva:
scorul evaluării
mutarea optimă

Beneficiu:

Elimină calcule duplicate
2. Transposition Table
Cache special pentru Minimax
Stochează:
hash-ul poziției
scor
depth
best move

Implementare:

Dictionary<Hash, TranspositionEntry>

Ideal:

Zobrist hashing pentru board

Beneficiu:

Evită recalcularea acelorași poziții
3. Time-Based Search

În loc de depth fix:

caut până la depth X

Folosim:

rulează 1 secundă și caută cât de mult se poate

Implementare:

Iterative deepening:
depth = 1 → 2 → 3 → ... până expiră timpul

Beneficiu:

AI mai stabil
Evită blocaje
4. UndoMove în loc de Clone

Problema:

Clonăm game state foarte des

Soluție:

ApplyMove(move)
UndoMove(move)

Workflow:

ApplyMove
→ Minimax
→ UndoMove

Beneficii:

Performanță mult mai bună
Mai puțin memory overhead
5. Move Ordering

Ordinea mutărilor influențează mult Alpha-Beta pruning

Idei:

mutări de captură mai întâi
best move din transposition table
killer moves
history heuristic

Beneficiu:

Search mai rapid
6. Alte optimizări
reducere alocări
evitarea clonărilor inutile
refolosire structuri de date
Next Tasks (AI)

High priority:

 Implement UndoMove
 Adaugă Transposition Table
 Time-based search (iterative deepening)

Medium:

 Move ordering
 Caching simplu pe funcții

Nice to have:

 Killer moves
 History heuristic
Login Features
 Forgot password
 Feedback UI pentru password match
 Password strength indicator (optional)
 Email verification
 Remember me

 ## 🧩 Feature: Online Multiplayer via SignalR

### 🎯 Objective

Implement real-time online multiplayer functionality using SignalR, enabling two remote players to join the same game session, exchange moves in real time, and maintain a consistent game state across clients.

---

## 📦 Scope

This feature will extend the existing game system (which already supports local play and bot play) to support:

- Real-time player vs player matches
- Game session synchronization
- Server-authoritative move validation
- Basic reconnect support
- Foundation for future features (clock, rating)

---

## 🏗️ Architecture Overview

### Components:

1. **SignalR Hub (`GameHub`)**
2. **Game Session Management (in-memory + DB)**
3. **Existing Game Logic (reused)**
4. **Database (Games, GamePlayers, GameStates)**

---

## 🔌 Functional Requirements

### 1. Connection & Session Management

- A user connects to SignalR hub.
- Each connection is associated with:
    - `UserId`
    - `GameId`

### Requirements:

- Maintain mapping:
    - `ConnectionId → UserId`
    - `GameId → List<ConnectionId>`
- On connection:
    - Authenticate user (if applicable)
    - Allow joining a game via `JoinGame(gameId)`

---

### 2. Join Game

### Method:

```csharp
Task JoinGame(string gameId)
```

### Behavior:

- Validate that user belongs to the game (`GamePlayers`)
- Add connection to SignalR group (`gameId`)
- Send current game state to the user:
    - board state
    - current turn
    - game status

---

### 3. Make Move

### Method:

```csharp
Task MakeMove(string gameId, MoveDto move)
```

### Flow:

1. Validate:
    - user is part of game
    - game is active
    - it is user's turn
2. Apply move using existing game logic
3. Persist:
    - update `Games` (state, turn, status)
    - insert into `GameStates`
4. Broadcast:

```csharp
Clients.Group(gameId).SendAsync("MovePlayed", move)
```

1. If game ended:
    - update `WinnerUserId`
    - broadcast game result

---

### 4. Server Authority (Critical)

- Client must NOT be trusted
- All moves validated server-side
- Reject invalid moves silently or with error message

---

### 5. Reconnect Handling (Basic)

### On reconnect:

- User calls `JoinGame(gameId)` again
- Server:
    - re-adds to group
    - sends full latest game state

### Requirement:

- Game state must always be reconstructable from DB

---

### 6. Disconnect Handling

### On disconnect:

- Remove connection from mappings
- If player disconnects:
    - mark timestamp (optional)
    - DO NOT immediately end game

(Future: timeout/forfeit logic)

---

### 7. Data Consistency

- `Games.CurrentTurn` must be enforced
- Prevent:
    - double moves
    - moves out of turn

### Recommendation:

- Use optimistic check:

```csharp
if (game.CurrentTurn != userId) reject
```

---

### 8. Concurrency Handling

- Ensure only one move is processed at a time per game

Options:

- DB-level check (simplest)
- or per-game lock (in-memory dictionary)

---

## 🧠 Non-Functional Requirements

- Low latency (<200ms move propagation)
- No duplicate moves
- Idempotent operations (safe retries)

---

## 🧪 Testing Scenarios

### Core:

- Two players connect and play full game
- Moves sync correctly both ways

### Edge Cases:

- Player refreshes page → reconnect works
- Player tries to move out of turn → rejected
- Two moves sent simultaneously → only one accepted
- Game ends → both clients notified

---

## 🚫 Out of Scope (for now)

- Matchmaking system
- Game clock
- Rating (ELO/Glicko)
- Spectators
- Chat

---

## 🔜 Future Extensions

- Add game clock (server-side time tracking)
- Add rating system (update after game ends)
- Add matchmaking queue
- Add reconnection timeout logic

---

## ✅ Definition of Done

- Two remote players can:
    - join same game
    - see same board state
    - make moves in real time
- Game state is:
    - persisted
    - recoverable after reconnect
- Server enforces:
    - turn order
    - move validity
- No desync between clients

---

## 💡 Notes

- Prefer server as single source of truth
- Keep SignalR layer thin (no business logic inside hub if possible)
- Reuse existing game engine for validation
