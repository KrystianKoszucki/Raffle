# Raffle API

A simple raffle system API that allows creating raffle draws, adding participants, closing raffles, and picking winners.
The project is built with .NET 9 using ASP.NET Core Web API, Entity Framework Core, FluentValidation, and SQLite.

---

## Project Structure

Raffle.Api  
- Controllers/         # API controllers  
- Database/            # EF Core DbContext and DatabaseService  
- Services/            # Business logic (RaffleService)  
- Contracts/           # Request / response records (DTOs)  
- Middleware/          # Global exception handling middleware  
- Validation/          # FluentValidation validators  
- Models/              # EF Core entities  
- raffle.db            # Local SQLite database (ignored by git)  

Raffle.Tests  
- Services/            # Unit tests for services (RaffleService, DatabaseService)

---

## Requirements

- .NET 9
- SQLite (local file-based database)
- Visual Studio / Rider / VS Code
- Optional: Postman for manual API testing

---

## Setup Instructions

1. Clone the repository:

   git clone <repository-url>  
   cd Raffle  

2. Restore dependencies:

   dotnet restore  

3. Apply database migrations:

   dotnet ef database update --project Raffle.Api  

This will create a local SQLite database file named `raffle.db`.

The following files are ignored by git:
- raffle.db  
- raffle.db-shm  
- raffle.db-wal  

---

## Running the Application

Start the API:

dotnet run --project Raffle.Api  

The API will be available at:

https://localhost:44390

or as https: https://localhost:7165

Swagger (OpenAPI UI) is available at:

https://localhost:44390/swagger  

---

## Running Tests

All unit tests are located in the `Raffle.Tests` project.

Run tests from the solution root:

dotnet test  

The tests are pure unit tests:
- No real database access
- Service logic tested with mocks

---

## Assumptions & Design Decisions

- SQLite is used as a local database for simplicity.
- Entity Framework Core handles database operations.
- Business logic is separated into services.
- Database access is isolated in `DatabaseService`.
- Global exception handling middleware maps domain exceptions to HTTP responses:
  - RaffleNotFoundException → 404 Not Found
  - RaffleClosedException → 400 Bad Request
  - DuplicateRaffleMemberException → 400 Bad Request
  - NoRaffleMembersException → 400 Bad Request
  - Unhandled exceptions → 500 Internal Server Error
- Input validation is implemented using FluentValidation.
- Each raffle member must have a unique email within a raffle.
- Winner selection uses RandomNumberGenerator for secure randomness.
- Unit tests focus on service behavior.

---

## Notes

This project uses a local SQLite database and is intended for development and testing purposes.
No external infrastructure is required to run the application.