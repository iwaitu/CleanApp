# Copilot Instructions for CleanApp

This repo is an ASP.NET Core 10 **Clean Architecture** backend template. These instructions describe how AI agents should work within this specific solution.

## Solution & Architecture
- Core layout (all under `src/` unless noted):
  - `CleanApp` – ASP.NET Core Web API entry (controllers, Program.cs, DI, Swagger, JWT, Redis, migrations bootstrapping).
  - `CleanApp.Domain` – pure domain entities (e.g., `AppUser`, `AppFile`, `BaseEntity`), no infrastructure or UI dependencies.
  - `CleanApp.Core` – business interfaces and services, depends on Domain/Infrastructure/Contracts (e.g., `IFileService`, `IMongoFileService`, `FileService`).
  - `CleanApp.Infrastructure` – EF Core `AppDbContext`, migrations, `IUnitOfWork<TContext>`/`UnitOfWork<TContext>`, `SeedData`.
  - `CleanApp.Contracts` – cross-layer DTOs and helpers (e.g., `PageOf<T>` for pagination).
  - `CleanApp.AppHost` & `CleanApp.ServiceDefaults` – hosting & common service defaults (`AddServiceDefaults`, `MapDefaultEndpoints`).
- Respect boundaries:
  - Domain must stay persistence-agnostic (no EF, no Redis, no HTTP).
  - Core services can depend on Infrastructure abstractions like `IUnitOfWork<AppDbContext>` but not on Web controllers.
  - Web controllers use Core interfaces (e.g., `IFileService`) and Identity, not `DbContext` directly.

## Key Runtime & Patterns
- **Startup & DI**: see `src/CleanApp/Program.cs`.
  - Postgres via `UseNpgsql` with migrations assembly `CleanApp.Infrastructure`.
  - Identity: `AddIdentityCore<AppUser>().AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>()`.
  - JWT bearer auth using `Token:*` settings from configuration.
  - Redis configured with `StackExchange.Redis` and `AddStackExchangeRedisCache`.
  - Mongo integration via singleton `IMongoFileService` constructed from `IConfiguration` (see `MongoFileService` in Core/Services).
  - Default endpoints & service defaults wired via `builder.AddServiceDefaults()` and `app.MapDefaultEndpoints()` from `CleanApp.ServiceDefaults`.
- **Unit of Work**: use `IUnitOfWork<AppDbContext>` from Infrastructure when persisting domain entities.
  - Use `AddEntity`, `UpdateEntity`, `RemoveEntity`, `CommitAsync` instead of calling `SaveChanges` directly.
  - `RemoveEntity` soft-deletes any `BaseEntity` by setting `IsDeleted = true` (see `UnitOfWork<TContext>` and tests); new code should respect this behavior.
- **Pagination**: use `PageOf<T>` in `CleanApp.Contracts` as the standard paged result shape (properties: `List`, `Page`, `PageSize`, `Total`).

## Auth & Controllers
- Controllers are in `src/CleanApp/Controllers`.
  - `AuthController` exposes `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/auth/me`.
    - JWT tokens are generated using `Token:*` configuration section; follow the existing `GenerateJwtToken` pattern when adding claims.
  - `HomeController` is a simple authenticated endpoint using `[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]`.
- When adding new controllers:
  - Use `[ApiController]` and explicit `[Route("api/[controller]")]` (or consistent routing with existing controllers).
  - Prefer injecting Core services (`IFileService`, future business services) rather than `AppDbContext`.
  - Use `[Authorize]` with roles where appropriate, reusing Identity roles (e.g., `Admin`) seeded in `SeedData`.

## File & Storage Services
- Core file abstractions live in `src/CleanApp.Core/Interfaces` and are implemented in `Core/Services`:
  - `IFileService`: high-level domain file operations returning/using `AppFile` and `PageOf<AppFile>`.
  - `IMongoFileService`: low-level Mongo-backed blob storage (`UploadFileAsync`, `DownloadFileAsync`, `DeleteFileAsync`).
- Existing behavior is tested in `test/CleanAppCoreTests/FileServiceTests.cs`:
  - `UploadAsync` stores bytes via `IMongoFileService` and persists `AppFile` metadata through `UnitOfWork<AppDbContext>`.
  - `DeleteAsync` calls `IMongoFileService.DeleteFileAsync` and soft-deletes metadata (expects `IsDeleted = true`).
  - `ListAsync` filters by file name (substring match) and returns a paginated `PageOf<AppFile>`.
- When extending storage features (e.g., adding folders, tags, or different backends):
  - Keep `IMongoFileService` focused on blob storage concerns; model metadata as domain entities in `CleanApp.Domain`.
  - Preserve existing test-backed semantics (soft delete; `Id` as ULID string for `AppFile`).

## Data Seeding & Migrations
- `SeedData.Initialize` in Infrastructure seeds an `Admin` role and a default admin user using Identity.
  - If you change admin defaults (email/password), update both `SeedData` and any related docs.
- At startup, `Program.cs` runs `db.Database.Migrate()` and then `SeedData.Initialize(services)` inside a scoped service provider.
  - New migrations should live in `src/CleanApp.Infrastructure/Migrations` and target `AppDbContext`.

## Build, Run, and Tests
- Default workflow (from repo root):
  - Restore & run API: `cd src && dotnet restore && dotnet run --project CleanApp/CleanApp.csproj`.
  - Run tests: from solution root (same level as `src/`), use `dotnet test` (tests live in `test/CleanAppCoreTests`).
- When adding new tests:
  - Prefer putting Core/service logic tests in `test/CleanAppCoreTests`.
  - Use the existing pattern of in-memory `AppDbContext` + `UnitOfWork<AppDbContext>` + mocks (see `FileServiceTests`).

## How AI Agents Should Modify Code
- Maintain the Clean Architecture boundaries and reuse existing patterns (UnitOfWork, `PageOf<T>`, soft deletes, JWT setup, DI registration style).
- When adding new services:
  - Define interfaces under `CleanApp.Core/Interfaces`, implementations under `CleanApp.Core/Services`, and register them in `Program.cs` similarly to `FileService`.
- When touching infrastructure (DbContext, migrations, Mongo, Redis):
  - Keep `AppDbContext` focused on EF mappings; avoid leaking infrastructure concerns back into Domain/Core contracts.
- Before introducing new libraries or patterns, check `README.md` and existing code for an established approach and follow it for consistency.
