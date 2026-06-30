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
- [x] Online multiplayer UI flow

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
- [x] Online multiplayer gameplay
- [ ] Matchmaking


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
- [x] Hard bot strategy using depth-4 minimax with tactical heuristics
- [x] Genetic algorithm training project for evaluation weights
- [ ] AI analysis project for bot matchups, CSV export, and graph generation

## Implementation Details

- [x] Core rules are enforced in the domain layer through `Game` and `GameRules`
- [x] Controllers stay thin and delegate most logic to application services
- [x] Current game state is stored as serialized JSON and snapshots are stored incrementally for replay
- [x] Bot turns are processed asynchronously after human moves and the frontend polls while the bot is thinking
- [x] Email verification and password reset tokens are stored hashed rather than in plain text
- [x] Auth and game flows are covered by unit tests and integration tests
- [x] 80% test coverage 

---

# Project TODO

### High priority

* [ ] Implement UndoMove () (nu se mai aplica)
* [x] Adauga Transposition Table

### Medium

* [x] Move ordering
* [x] Caching simplu pe functii


## Login Features

* [x] Forgot password
* [x] Feedback UI pentru password match
* [ ] Password strength indicator (doar ca idee daca e timp)
* [x] Email verification
* [x] Remember me

---

## Player Stats and Rating System

---

## GameStats

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


