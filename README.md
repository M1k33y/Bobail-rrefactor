# Bobail

Bobail is a full-stack web application for playing the Bobail strategy board game. It supports local two-player matches, games against an AI opponent, and real-time online multiplayer with authenticated accounts, persistent history, and replay review.

## Key Features

- Play Bobail on a 5x5 board with enforced movement rules, turn phases, legal move highlighting, and win detection.
- Start local two-player matches on the same device.
- Play against an AI opponent with Easy, Medium, and Hard difficulty levels and selectable player color.
- Create and join online multiplayer games by game ID, with real-time SignalR updates, active-game detection, invite ID copying, and turn validation.
- Use online game controls including 3-minute player clocks, timeout handling, resignation, and forced forfeits when a banned user is in an active match.
- Register and sign in with JWT authentication, remember-me sessions, email verification, resend verification, and forgot/reset password flows.
- Review completed matches through paginated game history, result labels, end reasons, and move-by-move replay timelines.
- Track personal game statistics including total games, wins, losses, member-since date, and results by color.
- Manage users from an admin panel with search, pagination, ban/unban actions, and real-time forced logout for banned accounts.
- Learn the rules through a dedicated rules page with animated board examples and customize the app, board, and piece appearance.

## Technologies Used

- **Backend:** .NET 8, ASP.NET Core Web API, SignalR, Entity Framework Core, SQL Server
- **Frontend:** React 19, Vite, React Router, SignalR JavaScript client, Lucide React
- **Authentication and validation:** JWT bearer authentication, BCrypt, FluentValidation
- **Testing:** xUnit, FluentAssertions, Moq, ASP.NET Core integration testing, EF Core InMemory/SQLite test stores
- **AI tooling:** GeneticSharp for bot weight training and ScottPlot for analysis visualizations

## Project Architecture

The backend follows a layered structure. `Bobail.Domain` contains the game model and rule enforcement, `Bobail.Application` contains services, DTOs, validation, bot strategies, and repository contracts, `Bobail.Infrastructure` contains SQL persistence and email delivery, and `Bobail.API` exposes REST endpoints, SignalR hubs, middleware, and background services.

The React frontend is organized by feature areas such as authentication, gameplay, game history, statistics, rules, settings, and admin management. Separate console projects support AI training and bot profile analysis.

## Installation

Prerequisites:

- .NET 8 SDK
- Node.js and npm
- SQL Server LocalDB or another SQL Server instance

Install backend dependencies:

```bash
dotnet restore Bobail2.sln
```

Install frontend dependencies:

```bash
cd frontend2/bobail-frontend
npm install
```

Configure the API in `Bobail.API/appsettings.json` or with user secrets/environment variables:

- `ConnectionStrings:Default`
- `Jwt:Key`
- `Frontend:BaseUrl`
- optional `Email:Smtp` settings for real email delivery

Apply database migrations:

```bash
dotnet ef database update --project Bobail.Infrastructure --startup-project Bobail.API
```

## Running the Application

Start the API:

```bash
dotnet run --project Bobail.API --launch-profile https
```

The backend runs on `https://localhost:7006` by default and exposes Swagger in development.

Start the frontend:

```bash
cd frontend2/bobail-frontend
npm run dev
```

The frontend runs on `http://localhost:5173` and is configured to call the local HTTPS API.

Run tests:

```bash
dotnet test Bobail2.sln
```

## Project Structure

- `Bobail.Domain` - core board, pieces, game lifecycle, clocks, and rules
- `Bobail.Application` - use cases, DTOs, validators, bot strategies, and interfaces
- `Bobail.Infrastructure` - EF Core persistence, repositories, migrations, and email senders
- `Bobail.API` - controllers, SignalR hubs, middleware, Swagger, and hosted services
- `frontend2/bobail-frontend` - React frontend application
- `Bobail.Training` - bot evaluation weight training console app
- `Bobail.AI.Analysis` - bot matchup analysis and chart generation console app
- `*.Tests` and `Bobail.IntegrationTests` - automated test projects

## Future Improvements

- Add automated online matchmaking beyond manual game-ID invites.

