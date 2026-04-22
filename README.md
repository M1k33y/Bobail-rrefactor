# Project TODO / Idei & Next Steps

## AI Improvements (Hard Difficulty)

## Next Tasks (AI)

### High priority


* [x] Adauga Transposition Table


### Medium

* [x] Move ordering
* [x] Caching simplu pe funcții

---

## Login Features

* [x] Forgot password
* [x] Feedback UI pentru password match
* [ ] Password strength indicator (optional)
* [x] Email verification
* [x] Remember me

---

## Player Stats & Rating System
* [ ] Time Clock per game
* [ ] +10 for win -10 for Loss or Chess elo formula

---

## GameStats
* [x] Number of games
* [x] Loses/Wins
* [x] Loses/Wins per color

### Objective


## RatingHistory

### Objective

Păstrarea unui istoric complet al modificărilor de rating pentru analiză și audit.

### Table Structure:

* Id
* UserId
* OldRating
* NewRating
* GameId
* CreatedAt

---

## Update Flow (după un joc)

### 1. Update rating în Users

```sql
UPDATE Users SET Rating = 1230 WHERE Id = A;
UPDATE Users SET Rating = 1270 WHERE Id = B;
```

---

### 2. Insert în RatingHistory

```sql
INSERT INTO RatingHistory (UserId, OldRating, NewRating, GameId)
VALUES (A, 1200, 1230, 45);

INSERT INTO RatingHistory (UserId, OldRating, NewRating, GameId)
VALUES (B, 1300, 1270, 45);
```

---

## Design Decision

* `Users.Rating` → rating curent (folosit în aplicație în timp real)
* `RatingHistory` → istoric complet (folosit pentru analize, grafice, debugging)

---

## Beneficii

* Acces rapid la rating curent (fără calcule suplimentare)
* Istoric complet pentru:

  * evoluție rating
  * statistici
  * posibile feature-uri viitoare (grafice, rank progression)

---

## Posibile Extensii

* WinRate calculat automat
* Streak (win/lose streak)
* HighestRating
* Rating decay (pentru inactivitate)
* Grafice evoluție rating


# Multiplayer

## 🧩 Feature: Online Multiplayer via SignalR

### 🎯 Objective

Implement real-time online multiplayer functionality using SignalR, enabling two remote players to join the same game session, exchange moves in real time, and maintain a consistent game state across clients.

---

## 📦 Scope

This feature will extend the existing game system (which already supports local play and bot play) to support:

* Real-time player vs player matches
* Game session synchronization
* Server-authoritative move validation
* Basic reconnect support
* Foundation for future features (clock, rating)

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

* A user connects to SignalR hub
* Each connection is associated with:

  * `UserId`
  * `GameId`

**Requirements:**

* Maintain mapping:

  * `ConnectionId → UserId`
  * `GameId → List<ConnectionId>`
* On connection:

  * Authenticate user (if applicable)
  * Allow joining a game via `JoinGame(gameId)`

---

### 2. Join Game

**Method:**

```csharp
Task JoinGame(string gameId)
```

**Behavior:**

* Validate that user belongs to the game (`GamePlayers`)
* Add connection to SignalR group (`gameId`)
* Send current game state:

  * board state
  * current turn
  * game status

---

### 3. Make Move

**Method:**

```csharp
Task MakeMove(string gameId, MoveDto move)
```

**Flow:**

1. Validate:

   * user is part of game
   * game is active
   * it is user's turn
2. Apply move using existing game logic
3. Persist:

   * update `Games` (state, turn, status)
   * insert into `GameStates`
4. Broadcast:

```csharp
Clients.Group(gameId).SendAsync("MovePlayed", move)
```

5. If game ended:

   * update `WinnerUserId`
   * broadcast game result

---

### 4. Server Authority (Critical)

* Client must NOT be trusted
* All moves validated server-side
* Reject invalid moves silently or with error message

---

### 5. Reconnect Handling (Basic)

**On reconnect:**

* User calls `JoinGame(gameId)` again

Server:

* re-adds to group
* sends full latest game state

**Requirement:**

* Game state must always be reconstructable from DB

---

### 6. Disconnect Handling

* Remove connection from mappings
* If player disconnects:

  * mark timestamp (optional)
  * DO NOT immediately end game

(Future: timeout/forfeit logic)

---

### 7. Data Consistency

* `Games.CurrentTurn` must be enforced

Prevent:

* double moves
* moves out of turn

**Recommendation:**

```csharp
if (game.CurrentTurn != userId) reject
```

---

### 8. Concurrency Handling

* Ensure only one move is processed at a time per game

Options:

* DB-level check (simplest)
* per-game lock (in-memory dictionary)

---

## 🧠 Non-Functional Requirements

* Low latency (<200ms move propagation)
* No duplicate moves
* Idempotent operations (safe retries)

---

## 🧪 Testing Scenarios

### Core:

* Two players connect and play full game
* Moves sync correctly both ways

### Edge Cases:

* Player refresh → reconnect works
* Move out of turn → rejected
* Simultaneous moves → only one accepted
* Game ends → both clients notified

---

## 🚫 Out of Scope (for now)

* Matchmaking system
* Game clock
* Rating (ELO/Glicko)
* Spectators
* Chat

---

## 🔜 Future Extensions

* Add game clock (server-side)
* Add rating system
* Add matchmaking queue
* Add reconnection timeout logic

---

## ✅ Definition of Done

* Two remote players pot:

  * join same game
  * vedea același board
  * face mutări în timp real
* Game state:

  * este persistat
  * poate fi reconstruit după reconnect
* Serverul validează:

  * turn order
  * mutările
* Nu există desync între clienți

---

## 💡 Notes

* Serverul este single source of truth
* SignalR trebuie să rămână subțire (fără logică de business în hub)
* Refolosește game engine-ul existent pentru validare
