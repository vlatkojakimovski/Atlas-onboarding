# Atlas Onboarding Backend

A digital customer onboarding platform for a multi-market banking group, built with .NET 8, Clean Architecture, and Domain-Driven Design.

## 🚀 Quick Start (5 minutes)

### Prerequisites
- .NET 8 SDK ([download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Docker Desktop (for SQL Server and Seq)
- Git

### Running the Application

1. **Clone the repository**
  https://github.com/vlatkojakimovski/Atlas-onboarding

2. **Start infrastructure (SQL Server + Seq)**
   ```bash
   # In the solution root
   docker-compose up -d
   ```
   
   Wait ~30 seconds for SQL Server to initialize.

3. **Run database migrations**
   ```bash
   dotnet ef database update --project Atlas.Infrastructure --startup-project Atlas.Api
   ```

4. **Start the API**
   ```bash
   dotnet run --project Atlas.Api --launch-profile https
   ```
   
   Wait for the message: `Now listening on: https://localhost:7129`
   
   The API will be available on:
   - HTTPS: `https://localhost:7129`
   - HTTP: `http://localhost:5168`

5. **Access Swagger UI**
   ```
   https://localhost:7129/swagger
   ```
   
   Or in HTTP:
   ```
   http://localhost:5168/swagger
   ```

6. **Access Seq (logs)**
   ```
   http://localhost:5341
   ```

### Quick Test

Submit a test application via Swagger:

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-05-15",
  "market": "MA",
  "nationalId": "9005151234567",
  "email": "john.doe@example.com",
  "phone": "+1234567890",
  "documents": [
    {
      "type": "PASSPORT",
      "image": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
    },
    {
      "type": "SELFIE",
      "image": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=="
    }
  ],
  "termsAccepted": true
}
```

Expected: `201 Created` with application ID and status `Approved`

---

## 📦 What's Built

### Functional Coverage

✅ **Core Flow (End-to-End)**
- Application submission with validation
- Document upload (PASSPORT/ID_CARD + SELFIE)
- Parallel identity verification and sanctions screening
- Market-specific national ID validation (6 markets: MA-MF)
- Automated approval/rejection decision engine
- Account opening and card ordering for approved applications
- Complete audit trail

✅ **Market-Specific Rules** (per Annex B)
- MA/MB: 13-digit Personal Number
- MC: 9-character Unified Citizen ID (alphanumeric + letter)
- MD: 10-digit Citizen Number + branch activation requirement
- ME: 10-digit Civil Number
- MF: Letter + 7 digits (Passport for non-citizens)

✅ **Non-Functional Requirements**
- Structured logging (Serilog → Seq)
- Retry policies with exponential backoff (Polly)
- Global exception handling with RFC 9110 Problem Details
- Configuration-driven market rules (hot-reload)
- EF Core with retry on transient failures

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/applications` | Submit new application |
| GET | `/applications/{id}` | Retrieve application details |

---

## 🏗️ Architecture

### Design Philosophy: Modular Monolith

We implemented a **modular monolith** rather than microservices for these reasons:

1. **Time constraint** - 3 days focused on working software over infrastructure
2. **No actual load** - Microservices add complexity without benefit at this stage  
3. **Development speed** - In-process calls are faster to develop and debug
4. **Atomic transactions** - Database transactions work naturally
5. **Lower operational complexity** - Single deployment, single database

**However**, the architecture is **microservices-ready** with clear service boundaries.

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────┐
│                   Atlas.Api                         │
│         (Controllers, Middleware, Contracts)        │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│              Atlas.Application                      │
│     (Use Cases, Commands, Handlers, Validation)     │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│                Atlas.Domain                         │
│    (Entities, Value Objects, Business Logic)        │
└─────────────────────────────────────────────────────┘
                         ↓
┌─────────────────────────────────────────────────────┐
│            Atlas.Infrastructure                     │
│  (EF Core, External Services, Audit, Repository)    │
└─────────────────────────────────────────────────────┘
```

### Identified Service Boundaries

The code is structured around **five logical services** that could be extracted as microservices when needed:

#### 1️⃣ **Application API** (Gateway)
- **Current:** `Atlas.Api` + `Atlas.Application`
- **Responsibilities:** Request handling, orchestration, workflow coordination
- **Key Components:** `SubmitApplicationCommandHandler`, Controllers, Validation

#### 2️⃣ **Verification Service**
- **Current:** `IIdentityVerificationService`, `ISanctionsScreeningService` 
- **Responsibilities:** Document verification, face matching, sanctions screening
- **Key Components:** `MockIdentityVerificationService`, `MockSanctionsScreeningService`
- **External Dependencies:** IDNow, World-Check (mocked)

#### 3️⃣ **Decision Service**  
- **Current:** `IApplicationDecisionEngine`, `INationalIdValidator`
- **Responsibilities:** Business rules, approval logic, market-specific validation
- **Key Components:** `ApplicationDecisionEngine`, `NationalIdValidator`, `MarketConfigurationProvider`

#### 4️⃣ **Banking Service**
- **Current:** `ICoreBankingService`, `ICardOrderingService`
- **Responsibilities:** Account opening, card ordering
- **Key Components:** `MockCoreBankingService`, `MockCardOrderingService`
- **External Dependencies:** Core banking system, card processor (mocked)

#### 5️⃣ **Audit Service**
- **Current:** `IAuditLogger`
- **Responsibilities:** Compliance logging, audit trail
- **Key Components:** `AuditLogger` (dual logging: Serilog + Database)

### Migration Path to Microservices

When scale or team boundaries require it:

1. **Extract services** - Each interface becomes its own API project
2. **Add HTTP communication** - Replace in-process calls with HTTP clients (Polly already configured for retries)
3. **Split database** - Use event sourcing or eventual consistency
4. **Add service mesh** - For observability and resilience
5. **Implement sagas** - For distributed transactions

---

## 🗄️ Data Model

### Database Schema

```
Applications (aggregate root)
├── Documents (1:many)
├── IdentityVerifications (1:1)
├── SanctionsScreenings (1:1)
│   └── SanctionMatches (1:many)
├── AccountDetails (1:1)
├── CardOrders (1:1)
└── ApplicationDecisions (1:1)
    └── RejectionReasons (1:many)

AuditLogs (separate, compliance-focused)
```

### Domain Model

- **Application** (aggregate root) - Encapsulates entire application lifecycle
- **Value Objects** - `IdentityVerificationResult`, `SanctionsScreeningResult`, `ApplicationDecision`
- **Enumerations** - `ApplicationStatus`, `DocumentType`, `RejectionReason`, etc.

Domain entities use **private setters** and **business methods** to enforce invariants.

---

## 🧪 Testing

At this point Unit tests and Integration tests are not implemented yet !!!
<!-- ### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/Atlas.UnitTests
dotnet test Tests/Atlas.IntegrationTests
```

### Test Coverage

**Unit Tests** (domain logic, validation):
- ✅ `ApplicationValidatorTests` - All validation rules
- ✅ `ApplicationDecisionEngineTests` - Approval/rejection logic
- ✅ `NationalIdValidatorTests` - Market-specific formats

**Integration Tests** (end-to-end flows):
- ✅ `ApplicationSubmissionTests` - Happy path and rejection scenarios
- ✅ `DatabaseTests` - Repository operations
- ✅ `ExternalServiceTests` - Mock service responses -->

---

## 📁 Project Structure

```
Atlas Task Solution/
├── Atlas.Api/                    # API layer (controllers, middleware)
├── Atlas.Application/            # Use cases, commands, handlers
├── Atlas.Domain/                 # Business logic, entities, value objects
├── Atlas.Infrastructure/         # EF Core, external services, audit
├── Tests/
│   ├── Atlas.UnitTests/         # Unit tests (fast, isolated)
│   └── Atlas.IntegrationTests/  # Integration tests (database, API)
├── docker-compose.yml           # SQL Server + Seq
├── README.md                    # This file
├── WHERE-TO-LOOK.md            # Key files to review
└── ARCHITECTURE.md             # Detailed design decisions
```

---

## 🔧 Configuration

### appsettings.json

Key configurations:
- **Markets** - National ID patterns, branch activation rules (hot-reloadable)
- **ConnectionStrings** - SQL Server connection
- **ExternalServices** - Timeout and retry settings
- **Logging** - Serilog levels and targets


## 🚨 Error Handling

### Validation Errors (400 Bad Request)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Validation Failed",
  "status": 400,
  "errors": {
    "DateOfBirth": ["Applicant must be at least 18 years old"],
    "Documents": ["A SELFIE document is required"]
  },
  "traceId": "0HN9F4FHTFHKA7:00000001"
}
```

### Application Errors (500 Internal Server Error)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "traceId": "0HN9F4FHTFHKA7:00000002"
}
```

All errors are logged to Seq with full context.

---

## 📊 Observability

### Structured Logging

Logs are written to:
1. **Console** (for Docker/K8s)
2. **Seq** (http://localhost:5341) - Searchable, queryable logs

### Log Levels

- **Debug** - Technical details (disabled in production)
- **Information** - Business events (application submitted, approved)
- **Warning** - Unexpected but handled (validation failures)
- **Error** - Failures requiring attention
- **Critical** - System-level failures

### Audit Trail

All application events are logged to the `AuditLogs` table for compliance:
- ApplicationCreated
- VerificationCompleted
- ScreeningCompleted  
- DecisionMade
- ApplicationStatusChanged
- AccountOpened
- CardOrdered

---

## 🛠️ Development

### Adding a New Market

1. Add market configuration to `appsettings.json`:
   ```json
   "MG": {
     "CountryName": "Market G",
     "NationalIdPattern": "^\\d{12}$",
     "NationalIdDescription": "12 digits",
     "NationalIdExample": "123456789012",
     "RequiresBranchActivation": false,
     "IsActive": true
   }
   ```

2. No code changes required - configuration is hot-reloadable

### Adding a New Rejection Reason

1. Add enum value to `Atlas.Domain/Enums/RejectionReason.cs`
2. Update decision logic in `ApplicationDecisionEngine.cs`
3. Add migration if storing in database

### Changing Verification Logic

Modify `ApplicationDecisionEngine.MakeDecision()` in `Atlas.Domain/BusinessLogic/`

---

## 📝 What's Not Built

### Intentionally Mocked

- **Identity verification** - Real IDNow integration (commercial service)
- **Sanctions screening** - Real World-Check integration (requires credentials)
- **Core banking** - Real account opening
- **Card ordering** - Real card processor integration

## 🤔 Design Decisions

### Why Single Database?

**Decision:** All entities in one database with clear schema boundaries.

**Reasoning:**
- Atomic transactions for consistency
- Simpler to develop and debug
- No distributed transaction complexity
- Schema designed for eventual split (foreign keys could become correlation IDs)


---

## 📖 Additional Documentation

- **`WHERE-TO-LOOK.md`** - Key files and decisions to review
