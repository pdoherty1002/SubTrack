# SubTrack

A subscription and renewal tracker — keep every recurring subscription in one place, see what's renewing soon, and get reminded before you're charged.

## Features

- Register/login with email + password, secured with JWT bearer auth
- Add, view, and remove subscriptions (name, cost, billing cycle, renewal date, category)
- Dashboard with colour-coded renewal urgency: red (1–2 days), amber (3–7 days), green (8+ days)
- Per-subscription reminder toggle
- Email reminders sent ahead of renewal (via AWS SES in production)
- Categorised subscriptions (Streaming, Fitness, Gaming, News, Utilities, Other)

## Tech stack

**Backend** — ASP.NET Core Web API (.NET 10), EF Core + Npgsql (PostgreSQL), JWT bearer authentication, BCrypt password hashing, Swagger/OpenAPI, AWS SES for email.

**Frontend** — Blazor WebAssembly (.NET 10), MudBlazor component library, an NSwag-generated typed client against the API's OpenAPI spec.

**Infrastructure** — PostgreSQL on Amazon RDS, both services containerised and deployed via Amazon ECS Express Mode (Fargate), images in Amazon ECR, CI/CD via GitHub Actions using OIDC (no long-lived AWS credentials).

**Testing** — xUnit + Moq for the API, bUnit for Blazor components.

## Project structure

```
SubTrack/
├── src/
│   ├── SubTrack.Api/              ASP.NET Core Web API
│   ├── SubTrack.Domain/           Entities and enums
│   ├── SubTrack.Infrastructure/   EF Core DbContext, migrations, repositories
│   └── SubTrack.Web/              Blazor WebAssembly frontend
├── tests/
│   ├── SubTrack.Tests/            API unit tests
│   └── SubTrack.Web.Tests/        Frontend component tests
└── .github/workflows/             CI/CD pipeline
```

## Architecture

```
Browser ──HTTPS──> ECS (subtrack-web)      nginx serving the Blazor WASM static build
             │
             └──HTTPS/CORS──> ECS (subtrack-api)    ASP.NET Core Web API
                                     │
                                     └──SSL (verify-full)──> RDS PostgreSQL
```

Both services sit behind Application Load Balancers provisioned automatically by ECS Express Mode, each with its own AWS-generated HTTPS URL. The API only accepts cross-origin requests from an explicitly configured frontend origin.

## Live demo

**[su-ec7a765b2d944a8cb9f270f15374d662.ecs.us-east-2.on.aws](https://su-ec7a765b2d944a8cb9f270f15374d662.ecs.us-east-2.on.aws/)**

Register an account to get started.

## Running the tests

```
dotnet test
```
Runs both the API's xUnit suite and the frontend's bUnit suite.

## API overview

| Method | Route | Auth required | Description |
|---|---|---|---|
| POST | `/api/Auth/register` | No | Create a new account |
| POST | `/api/Auth/login` | No | Log in, returns a JWT |
| GET | `/api/Subscriptions` | Yes | List the caller's subscriptions |
| POST | `/api/Subscriptions` | Yes | Create a subscription |
| DELETE | `/api/Subscriptions/{id}` | Yes | Delete a subscription (owner-only) |
| GET | `/health` | No | Health check |

Full interactive documentation is available at `/swagger` when running in Development.

## Deployment

Deployment is automated via GitHub Actions on every push to `main`:
1. Restore, build, and run the full test suite
2. Build and push Docker images for both the API and frontend to Amazon ECR
3. Trigger a new deployment on each ECS Express Mode service

The pipeline authenticates to AWS via OpenID Connect — no static AWS credentials are stored in GitHub. Its IAM role is scoped to exactly two permissions: pushing images to ECR and updating these two specific ECS services.

**Configuration in production** is supplied via ECS task definition environment variables (connection string, JWT signing key, SES sender address, allowed frontend origin) rather than being baked into the image or committed to source control.

**Note on the frontend specifically:** Blazor WebAssembly config is baked into the static build at publish time, not read from the environment at container startup. `wwwroot/appsettings.json` (committed) holds the real production API URL for this reason; local development instead uses `wwwroot/appsettings.Development.json`, which is not deployed.

## Future work

- Logout functionality
- Password reset flow
- General UI polish and fixes
