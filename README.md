# Implemented Features

## Backend

- [x] ASP.NET Core Web API with dedicated controllers for authentication and game operations
- [x] JWT-protected endpoints for game creation, move execution, abandon, history, stats, and replay access
- [x] Swagger/OpenAPI enabled in development with Bearer token support
- [x] Global exception-handling middleware returning JSON error payloads
- [x] SQL Server persistence configured through Entity Framework Core
- [x] Fixed-window rate limiting applied to authentication-sensitive endpoints
- [x] Email delivery abstraction with SMTP sender and in-memory fallback based on configuration
- [x] Background bot execution triggered automatically after player actions in bot games

## Frontend

- [x] React frontend with route-based navigation using `react-router-dom`
- [x] Protected routes for starting games, playing games, viewing history, stats, and replay review
- [x] Home page with login/logout actions and navigation to play flows
- [x] Rules page describing board setup, movement, turn flow, and victory conditions
- [x] Interactive game page with board rendering, turn state, and winner modal
- [x] Local game start page that creates a game and redirects into the match
- [x] Bot game setup page with selectable difficulty and player color
- [x] Game history page with pagination and per-game review links
- [x] Replay review page with timeline navigation across saved board states
- [x] Player stats page with totals, win/loss breakdown, and account age
- [x] Settings page for app theme, board theme, and piece style selection
- [x] Theme preferences persisted in `localStorage`
- [ ] Online multiplayer UI flow

## Authentication

- [x] User registration with email, password, and nickname
- [x] Backend validation for email format, password policy, and nickname length
- [x] Password hashing using BCrypt
- [x] Email verification required before login
- [x] Email verification flow with generated token, hashed token storage, expiration, and verification page
- [x] Resend verification email flow
- [x] JWT login response with token, expiry, remember-me flag, and nickname
- [x] Remember-me session persistence using `localStorage` or `sessionStorage`
- [x] Forgot password flow with reset token generation and email delivery
- [x] Reset password flow with token validation, password update, and token invalidation
- [x] Login blocked for inactive accounts
- [x] Login blocked for unverified accounts

## Game Logic

- [x] 5x5 board initialization with five red pieces, five green pieces, and Bobail in the center
- [x] Two-phase turn system: player move and Bobail move
- [x] First-turn rule where only the player-piece move is performed
- [x] Player pieces can move horizontally, vertically, or diagonally
- [x] Player pieces must move as far as possible in the chosen direction
- [x] Bobail moves exactly one square to an empty adjacent cell
- [x] Turn ownership validation for both player pieces and Bobail
- [x] Valid-move endpoints for player pieces and Bobail
- [x] Victory when Bobail reaches a home row
- [x] Victory when Bobail is fully surrounded
- [x] Game abandon state
- [x] Replay support through per-move game state snapshots

## Multiplayer

- [x] Local multiplayer on a single device
- [x] Per-game player mapping persisted for local and bot games
- [ ] Online multiplayer gameplay
- [ ] Matchmaking
- [ ] Real-time synchronization with SignalR or equivalent

## Database / Persistence

- [x] Users table with email, password hash, nickname, role, active flag, and email verification fields
- [x] Games table storing serialized game state plus status, turn, mode, timestamps, and winner user id
- [x] Game player records for color assignment and bot participation
- [x] Game state snapshot history stored for replay and review
- [x] Email verification token persistence with hashed tokens and expiration
- [x] Password reset token persistence with hashed tokens, expiration, and used flag
- [x] Unique constraints for email, token hashes, game/color, game/user, and game/move number
- [x] Finished-game history queries scoped to the authenticated user
- [x] Replay queries restricted to users who participated in the finished game

## Bot / AI

- [x] Player-vs-bot mode
- [x] Easy bot difficulty
- [x] Medium bot difficulty
- [x] Hard bot difficulty
- [x] Easy bot strategy using random legal moves with Bobail row preference
- [x] Medium bot strategy using depth-2 minimax with alpha-beta pruning and transposition table caching
- [x] Hard bot strategy using depth-3 minimax with tactical heuristics
- [x] Genetic algorithm training project for evaluation weights
- [ ] AI analysis project for bot matchups, CSV export, and graph generation

## Implementation Details

- [x] Core rules are enforced in the domain layer through `Game` and `GameRules`
- [x] Controllers stay thin and delegate most logic to application services
- [x] Current game state is stored as serialized JSON and snapshots are stored incrementally for replay
- [x] Bot turns are processed asynchronously after human moves and the frontend polls while the bot is thinking
- [x] Email verification and password reset tokens are stored hashed rather than in plain text
- [x] Auth and game flows are covered by unit tests and integration tests
- [ ] 80% test coverage 

---

# Project TODO

## AI Improvements (Hard Difficulty)

### Probleme actuale

* Hard AI se misca lent
* Se fac foarte multe clone de game state
* Minimax recalculeaza aceleasi pozitii

---

Solutie:
### 1. UndoMove in loc de Clone

Problema:

* Clonam game state foarte des

Solutie:

```text
ApplyMove(move)
UndoMove(move)
```

Workflow:

```text
ApplyMove
-> Minimax
-> UndoMove
```

**Beneficii:**

* Performanta mult mai buna
* Mai putin memory overhead

---



## Next Tasks (AI)

### High priority

* [ ] Implement UndoMove
* [x] Adauga Transposition Table

### Medium

* [x] Move ordering
* [x] Caching simplu pe functii


## Login Features

* [x] Forgot password
* [x] Feedback UI pentru password match
* [ ] Password strength indicator (optional)
* [x] Email verification
* [x] Remember me

---

## Player Stats and Rating System

---

## GameStats

### Objective

Sa avem statistici agregate pentru fiecare utilizator, usor de accesat si afisat.

### Fields:

* TotalGamesPlayed
* TotalWins
* TotalLosses
* MemberSince (data crearii contului)
* CurrentRating


## RatingHistory


### Table Structure:

* Id
* UserId
* OldRating
* NewRating
* GameId
* CreatedAt

---


## Posibile Extensii

* WinRate calculat automat
* Streak (win/lose streak)
* HighestRating
* Rating decay (pentru inactivitate)
* Grafice evolutie rating

# Multiplayer

## Feature: Online Multiplayer via SignalR

### Objective

Implement real-time online multiplayer functionality using SignalR, enabling two remote players to join the same game session, exchange moves in real time, and maintain a consistent game state across clients.

---

## Scope

This feature will extend the existing game system (which already supports local play and bot play) to support:

* Real-time player vs player matches
* Game session synchronization
* Server-authoritative move validation
* Basic reconnect support
* Foundation for future features (clock, rating)

---

## Architecture Overview

### Components:

1. **SignalR Hub (`GameHub`)**
2. **Game Session Management (in-memory + DB)**
3. **Existing Game Logic (reused)**
4. **Database (Games, GamePlayers, GameStates)**

---

## Functional Requirements

### 1. Connection and Session Management

* A user connects to SignalR hub
* Each connection is associated with:
* `UserId`
* `GameId`

**Requirements:**

* Maintain mapping:
* `ConnectionId -> UserId`
* `GameId -> List<ConnectionId>`
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

## Non-Functional Requirements

* Low latency (<200ms move propagation)
* No duplicate moves
* Idempotent operations (safe retries)

---

## Testing Scenarios

### Core:

* Two players connect and play full game
* Moves sync correctly both ways

### Edge Cases:

* Player refresh -> reconnect works
* Move out of turn -> rejected
* Simultaneous moves -> only one accepted
* Game ends -> both clients notified

---

## Out of Scope (for now)

* Matchmaking system
* Game clock
* Rating (ELO/Glicko)
* Spectators
* Chat

---

## Future Extensions

* Add game clock (server-side)
* Add rating system
* Add matchmaking queue
* Add reconnection timeout logic

---

## Definition of Done

* Two remote players pot:
* join same game
* vedea acelasi board
* face mutari in timp real
* Game state:
* este persistat
* poate fi reconstruit dupa reconnect
* Serverul valideaza:
* turn order
* mutarile
* Nu exista desync intre clienti

---

## Notes

* Serverul este single source of truth
* SignalR trebuie sa ramana subtire (fara logica de business in hub)
* Refoloseste game engine-ul existent pentru validare
