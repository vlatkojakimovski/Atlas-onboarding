# Implementation Plan: Atlas Onboarding Backend System

## Overview

This implementation plan breaks down the Atlas Onboarding backend system into discrete, manageable tasks following the 3-day timeline constraint. The system implements a synchronous, fully automated customer onboarding flow with identity verification, sanctions screening, and downstream account/card provisioning.

**Implementation Strategy:** Phase 1/v1 with auto-rejection of POSSIBLE_MATCH results, mock external providers, seam-only for core banking and card service, and architecture designed for future async manual review upgrade.

**Technology Stack:** .NET 8, ASP.NET Core, Entity Framework Core, SQL Server, Seq, FsCheck (property-based testing)

**Day 1 Focus:** Foundation, database, domain core, happy path skeleton  
**Day 2 Focus:** Complete flow, provider integrations, decision engine, adapters, audit  
**Day 3 Focus:** Testing (unit, property-based, integration), error handling, documentation

---

## Tasks

### 1. Foundation & Setup

- [ ] 1.1 Create solution structure and projects
  - Create `Atlas.sln` solution file
  - Create `Atlas.Api` project (ASP.NET Core Web API, .NET 8)
  - Create `Atlas.Application` project (Class Library, .NET 8)
  - Create `Atlas.Domain` project (Class Library, .NET 8)
  - Create `Atlas.Infrastructure` project (Class Library, .NET 8)
  - Create `Atlas.Tests.Unit` project (xUnit, .NET 8)
  - Create `Atlas.Tests.Integration` project (xUnit, .NET 8)
  - Add project references following clean architecture dependencies
  - _Requirements: All functional requirements (project structure foundation)_

- [ ] 1.2 Install required NuGet packages
  - **Atlas.Api:** Microsoft.AspNetCore.OpenApi, Swashbuckle.AspNetCore, Serilog.AspNetCore, Serilog.Sinks.Seq, Serilog.Enrichers.Environment
  - **Atlas.Application:** FluentValidation, Microsoft.Extensions.DependencyInjection.Abstractions
  - **Atlas.Infrastructure:** Microsoft.EntityFrameworkCore.SqlServer, Microsoft.EntityFrameworkCore.Design, Polly, Polly.Extensions.Http
  - **Atlas.Tests.Unit:** xUnit, xUnit.runner.visualstudio, Moq, FluentAssertions, FsCheck, FsCheck.Xunit
  - **Atlas.Tests.Integration:** Microsoft.AspNetCore.Mvc.Testing, Microsoft.EntityFrameworkCore.InMemory
  - _Requirements: 16, 17, 18 (configuration, performance, logging dependencies)_

- [ ] 1.3 Configure appsettings.json with market configurations
  - Add Markets configuration section with all 6 markets (MA-MF)
  - Configure national ID patterns from Annex B (regex patterns)
  - Add branch activation flags (MD market: true, others: false)
  - Add ConnectionStrings section for SQL Server
  - Add Seq URL configuration
  - Add timeout configurations (10s for providers)
  - _Requirements: 2.1-2.7, 14.4, 16.1-16.5, 17.1-17.2_

- [ ] 1.4 Set up Serilog structured logging
  - Configure Serilog in Program.cs with JSON formatter
  - Add Seq sink with configured URL
  - Add enrichers for correlation ID, user context, market context
  - Configure log levels (Information, Warning, Error)
  - Test log output to Seq
  - _Requirements: 10.9, 18.1-18.8_

---

### 2. Database Layer

- [ ] 2.1 Create EF Core DbContext and entity configurations
  - Create `ApplicationDbContext` class
  - Define `Applications` DbSet
  - Define `Documents` DbSet
  - Define `IdentityVerifications` DbSet
  - Define `SanctionsScreenings` DbSet
  - Define `SanctionMatches` DbSet
  - Define `AccountDetails` DbSet
  - Define `CardOrders` DbSet
  - Define `ApplicationDecisions` DbSet
  - Define `RejectionReasons` DbSet
  - Define `AuditLogs` DbSet
  - Configure relationships and constraints from schema
  - _Requirements: 12.1-12.10_

- [ ] 2.2 Create entity classes matching database schema
  - Create `ApplicationEntity` with all fields from Applications table
  - Create `DocumentEntity` with foreign key to ApplicationEntity
  - Create `IdentityVerificationEntity` with one-to-one relationship
  - Create `SanctionsScreeningEntity` with one-to-one relationship
  - Create `SanctionMatchEntity` with foreign key to SanctionsScreeningEntity
  - Create `AccountDetailsEntity` with one-to-one relationship
  - Create `CardOrderEntity` with one-to-one relationship
  - Create `ApplicationDecisionEntity` with one-to-one relationship
  - Create `RejectionReasonEntity` with foreign key to ApplicationDecisionEntity
  - Create `AuditLogEntity` with foreign key to ApplicationEntity
  - _Requirements: 12.1-12.10_

- [ ] 2.3 Configure EF Core fluent API for constraints
  - Add CHECK constraints for Status enumeration
  - Add CHECK constraints for MarketCode enumeration
  - Add CHECK constraints for DocumentType enumeration
  - Add CHECK constraints for DocumentStatus, ScreeningStatus, MatchType
  - Configure indexes: IX_Applications_MarketCode_Status, IX_Applications_CreatedAt, IX_Applications_Email
  - Configure indexes: IX_Documents_ApplicationId, IX_AuditLogs_Timestamp, IX_AuditLogs_ApplicationId, IX_AuditLogs_UserId, IX_AuditLogs_MarketCode
  - Configure foreign key relationships with cascading behavior
  - _Requirements: 12.10, 14.3_

- [ ] 2.4 Create and apply EF Core migrations
  - Run `dotnet ef migrations add InitialCreate`
  - Review generated migration SQL
  - Update connection string in appsettings.json for local SQL Server
  - Run `dotnet ef database update` to create database
  - Verify tables, constraints, and indexes created correctly
  - _Requirements: 12.1-12.10_

- [ ] 2.5 Configure retry policies for database operations
  - Configure EF Core with `EnableRetryOnFailure` (3 retries, 5s max delay)
  - Test transient failure handling with connection interruption
  - _Requirements: 11.5_

---

### 3. Domain Layer - Core Models and Value Objects

- [ ] 3.1 Create domain enumerations
  - Create `ApplicationStatus` enum: Pending, Approved, Rejected, PendingReview
  - Create `DocumentType` enum: PASSPORT, ID_CARD, SELFIE
  - Create `DocumentVerificationStatus` enum: Valid, Invalid, Inconclusive
  - Create `ScreeningStatus` enum: Clear, PossibleMatch
  - Create `MatchType` enum: Sanctions, PEP
  - Create `RejectionReason` enum: InvalidDocument, FaceMismatch, InconclusiveVerificationResult, SanctionMatch, PossibleSanctionMatch, VerificationProviderUnavailable, ScreeningProviderUnavailable
  - _Requirements: 3.3-3.6, 4.3-4.6, 5.2, 5.4-5.5_

- [ ] 3.2 Create value objects and records
  - Create `IdentityVerificationResult` record with ProviderId, DocumentStatus, FaceMatch, Confidence, VerifiedAt
  - Create `SanctionsScreeningResult` record with CaseId, Status, Matches, ScreenedAt
  - Create `SanctionMatch` record with Type, Score, Subject
  - Create `ApplicationDecision` record with Status, Reasons, IsApproved, IsRejected properties
  - Create `AccountCreationResult` record with AccountId, AccountNumber, CreatedAt
  - Create `CardOrderResult` record with CardOrderId, CardReference, OrderedAt
  - Create `MarketConfiguration` record with MarketCode, CountryName, NationalIdRule, RequiresBranchActivation, IsActive
  - Create `NationalIdValidationRule` record with Pattern, Description, Example
  - _Requirements: 2.1-2.7, 3.2, 4.2, 5.3, 6.2, 7.2, 14.6, 16.2_

- [ ] 3.3 Create Application aggregate root
  - Create `Application` class with Id, FirstName, LastName, DateOfBirth, MarketCode, NationalId, Email, Phone, Status, CreatedAt, CompletedAt
  - Add navigation properties for Documents, IdentityVerification, SanctionsScreening, AccountDetails, CardOrder, Decision
  - Add private setters to enforce encapsulation
  - Implement business method: `StartVerification()`
  - Implement business method: `CompleteVerification(IdentityVerificationResult)`
  - Implement business method: `CompleteScreening(SanctionsScreeningResult)`
  - Implement business method: `Approve(Guid accountId, Guid cardOrderId)`
  - Implement business method: `Reject(IReadOnlyList<RejectionReason> reasons)`
  - Add validation rules: FirstName/LastName 1-100 chars, DateOfBirth 18+, immutable state transitions
  - _Requirements: 1.1, 5.4-5.5, 12.1_

- [ ] 3.4 Create domain validation rules
  - Implement age validation (must be 18+)
  - Implement state transition validation (Pending → Approved/Rejected only, no reversals)
  - Implement required fields validation
  - Implement string length constraints (FirstName, LastName, Email, Phone)
  - _Requirements: 1.3, 5.4-5.5_

---

### 4. Domain Layer - Business Logic

- [ ] 4.1 Implement market configuration provider
  - Create `IMarketConfigurationProvider` interface
  - Create `MarketConfigurationProvider` class
  - Implement `GetMarketConfiguration(string marketCode)` method
  - Load configurations from IConfiguration (appsettings.json)
  - Cache loaded configurations in memory
  - Validate all markets configured at startup (fail fast if missing)
  - _Requirements: 14.4-14.7, 16.1-16.5_

- [ ] 4.2 Implement national ID validator
  - Create `INationalIdValidator` interface
  - Create `NationalIdValidator` class
  - Implement `IsValid(string nationalId, string marketCode)` method
  - Use market-specific regex patterns from configuration
  - Return validation result with error messages including expected format
  - _Requirements: 2.1-2.7_

- [ ] 4.3 Implement application decision engine
  - Create `IApplicationDecisionEngine` interface
  - Create `ApplicationDecisionEngine` class (pure function, no dependencies)
  - Implement `MakeDecision(IdentityVerificationResult, SanctionsScreeningResult, MarketConfiguration)` method
  - **Approval rule:** DocumentStatus == Valid AND FaceMatch == true AND ScreeningStatus == Clear → APPROVED
  - **Rejection rules:**
    - Invalid or Inconclusive document → REJECTED (InvalidDocument / InconclusiveVerificationResult)
    - Face match false → REJECTED (FaceMismatch)
    - PossibleMatch screening → REJECTED (PossibleSanctionMatch) in v1
    - Clear sanctions match → REJECTED (SanctionMatch)
    - Provider unavailable → REJECTED (VerificationProviderUnavailable / ScreeningProviderUnavailable)
  - Accumulate multiple rejection reasons when applicable
  - Ensure deterministic behavior (same inputs → same outputs)
  - _Requirements: 3.3-3.6, 4.3-4.6, 5.1-5.7, 19.2_

---

### 5. Infrastructure Layer - External Service Adapters

- [ ] 5.1 Create mock identity verification service
  - Create `IIdentityVerificationService` interface with `VerifyIdentityAsync` method
  - Create `IdentityVerificationRequest` record with DocumentType, DocumentImageBase64, SelfieImageBase64
  - Create `MockIdentityVerificationService` class
  - Implement configuration-driven test scenarios:
    - **Happy path:** VALID document + faceMatch=true + confidence=0.95
    - **Invalid document:** INVALID status
    - **Face mismatch:** VALID document + faceMatch=false
    - **Inconclusive:** INCONCLUSIVE status
    - **Timeout scenario:** configurable delay to test timeout handling
  - Add logging for all verification calls
  - Return mock ProviderId (e.g., "IDNOW-{Guid}")
  - _Requirements: 3.1-3.6_

- [ ] 5.2 Create mock sanctions screening service
  - Create `ISanctionsScreeningService` interface with `ScreenAsync` method
  - Create `SanctionsScreeningRequest` record with FirstName, LastName, DateOfBirth, Nationality
  - Create `MockSanctionsScreeningService` class
  - Implement configuration-driven test scenarios:
    - **Clear:** Status.Clear with empty matches
    - **PossibleMatch:** Status.PossibleMatch with sample Sanctions match (score 0.75)
    - **PEP hit:** Status.PossibleMatch with PEP match (score 0.80)
    - **Timeout scenario:** configurable delay to test timeout handling
  - Add logging for all screening calls
  - Return mock CaseId (e.g., "WC-{Guid}")
  - _Requirements: 4.1-4.6_

- [ ] 5.3 Create Polly retry policies for external services
  - Create retry policy for IIdentityVerificationService: 3 retries, exponential backoff (2s, 4s, 8s)
  - Create retry policy for ISanctionsScreeningService: 3 retries, exponential backoff (2s, 4s, 8s)
  - Handle HttpRequestException, TaskCanceledException, and 5xx responses
  - Add logging on each retry attempt (Warning level)
  - Configure 10-second timeout for each request
  - _Requirements: 11.1-11.4, 17.1-17.2_

- [ ] 5.4 Create core banking service adapter (seam)
  - Create `ICoreBank ingService` interface with `OpenAccountAsync` method
  - Create `AccountCreationRequest` record with ApplicationId, FirstName, LastName, DateOfBirth, NationalId, Email, Phone, MarketCode
  - Create `MockCoreBankingService` class
  - Generate AccountId (Guid.NewGuid)
  - Generate AccountNumber (formatted string: "ACC-{first 8 chars of Guid}")
  - Use ApplicationId as idempotency key (store in-memory dictionary to return same result)
  - Add logging for account creation
  - _Requirements: 6.1-6.5, 15.1_

- [ ] 5.5 Create card service adapter (seam)
  - Create `ICardOrderingService` interface with `OrderCardAsync` method
  - Create `CardOrderRequest` record with AccountId, FirstName, LastName, DeliveryAddress
  - Create `MockCardOrderingService` class
  - Generate CardOrderId (Guid.NewGuid)
  - Generate CardReference (formatted string: "CARD-{first 8 chars of Guid}")
  - Use AccountId as idempotency key (store in-memory dictionary to return same result)
  - Add logging for card ordering
  - _Requirements: 7.1-7.6, 15.2_

- [ ] 5.6 Implement audit logger
  - Create `IAuditLogger` interface with methods:
    - `LogApplicationCreated(Guid, string marketCode, string userId)`
    - `LogVerificationCompleted(Guid, IdentityVerificationResult)`
    - `LogScreeningCompleted(Guid, SanctionsScreeningResult)`
    - `LogDecisionMade(Guid, ApplicationDecision)`
    - `LogAccountOpened(Guid, Guid accountId)`
    - `LogCardOrdered(Guid, Guid cardOrderId)`
    - `LogApplicationStatusChanged(Guid, ApplicationStatus from, ApplicationStatus to, string reason)`
    - `LogDataAccess(string userId, string resourceType, string resourceId, string action)`
  - Create `AuditLogger` class using ILogger<AuditLogger>
  - Emit structured logs with correlation ID, market code, user context
  - Also persist audit logs to AuditLogs table for 10-year retention
  - _Requirements: 10.1-10.10, 18.1-18.8_

- [ ] 5.7 Create application repository
  - Create `IApplicationRepository` interface
  - Implement `AddAsync(Application)` method
  - Implement `GetByIdAsync(Guid)` method
  - Implement `UpdateAsync(Application)` method
  - Use EF Core for persistence
  - Map between domain Application and ApplicationEntity
  - Include related entities (Documents, IdentityVerification, etc.) when loading
  - _Requirements: 12.1-12.10_

---

### 6. Application Layer - Use Cases

- [ ] 6.1 Create application validator
  - Create `IApplicationValidator` interface with `Validate(SubmitApplicationCommand)` method
  - Create `ValidationResult` record with IsValid, Errors collection
  - Create `ValidationError` record with Field, Code, Message
  - Create `ApplicationValidator` class
  - Validate required fields: firstName, lastName, dateOfBirth, market, nationalId, email, phone, documents
  - Validate market code is in supported markets (MA-MF)
  - Validate national ID format using INationalIdValidator
  - Validate document types (at least 1 PASSPORT/ID_CARD + 1 SELFIE required)
  - Validate base64 image data integrity
  - Validate email format (simple regex)
  - Validate phone format (E.164 recommended but not strictly enforced)
  - Validate termsAccepted == true
  - Validate age 18+
  - _Requirements: 1.2-1.8, 2.7_

- [ ] 6.2 Create submit application command and handler
  - Create `SubmitApplicationCommand` record with all required fields from API contract
  - Create `SubmitApplicationCommandHandler` class
  - Implement validation using IApplicationValidator
  - Throw ValidationException if validation fails
  - Create Application aggregate with status Pending
  - Persist using IApplicationRepository
  - Log application created event using IAuditLogger
  - Invoke IApplicationOrchestrator to process application
  - Return ApplicationResult with applicationId and status
  - _Requirements: 1.1-1.8, 8.5, 10.1_

- [ ] 6.3 Implement application orchestrator
  - Create `IApplicationOrchestrator` interface with `ProcessApplicationAsync(Application, CancellationToken)` method
  - Create `ApplicationResult` record with ApplicationId, Status, RejectionReason, AccountId, CardOrderId
  - Create `ApplicationOrchestrator` class
  - Inject IIdentityVerificationService, ISanctionsScreeningService, IApplicationDecisionEngine, ICoreBank ingService, ICardOrderingService, IApplicationRepository, IAuditLogger
  - **Step 1:** Execute identity verification and sanctions screening in parallel using `Task.WhenAll`
  - **Step 2:** Log verification and screening completion
  - **Step 3:** Invoke decision engine with results
  - **Step 4:** Log decision made
  - **Step 5:** If APPROVED:
    - Open account via CoreBankingService
    - Log account opened
    - Order card via CardOrderingService (set RequiresBranchActivation flag for MD market)
    - Log card ordered
    - Update application status to APPROVED
  - **Step 6:** If REJECTED:
    - Update application status to REJECTED with reasons
  - **Step 7:** Log application status changed
  - **Step 8:** Persist updated application
  - Handle cancellation token throughout
  - _Requirements: 5.1-5.7, 6.1-6.5, 7.1-7.6, 8.1-8.5, 10.2-10.7_

---

### 7. API Layer - Controllers and Middleware

- [ ] 7.1 Create applications controller with POST endpoint
  - Create `ApplicationsController` class
  - Implement `POST /applications` endpoint
  - Accept `SubmitApplicationCommand` from request body
  - Inject `ISubmitApplicationCommandHandler`
  - Call handler to process application
  - Return 201 Created with ApplicationResponse (applicationId, status)
  - Include Location header: `/applications/{applicationId}`
  - Handle ValidationException → 400 Bad Request with RFC 9110 Problem Details
  - Handle unhandled exceptions → 500 Internal Server Error with trace ID
  - _Requirements: 1.1, 1.7, 13.1-13.5, 13.10_

- [ ] 7.2 Create applications controller with GET endpoint
  - Implement `GET /applications/{id}` endpoint
  - Inject IApplicationRepository
  - Retrieve application by ID
  - If not found → return 404 Not Found
  - Map to ApplicationDetailsResponse with status, personal details, createdAt, completedAt
  - Include accountId if status is APPROVED
  - Include rejectionReasons if status is REJECTED
  - Return 200 OK with application details
  - _Requirements: 9.1-9.5, 13.6-13.8_

- [ ] 7.3 Create API contracts (request/response models)
  - Create `SubmitApplicationRequest` record matching API contract JSON
  - Create `DocumentSubmission` record with type and image
  - Create `ApplicationResponse` record with applicationId and status
  - Create `ApplicationDetailsResponse` record with full details
  - Add JSON property name mappings (camelCase)
  - Add validation attributes for model binding
  - _Requirements: 13.1, 13.9_

- [ ] 7.4 Configure global exception handling middleware
  - Create exception handling middleware
  - Catch ValidationException → return 400 Bad Request with field-specific errors in RFC 9110 Problem Details format
  - Catch NotFoundException → return 404 Not Found
  - Catch DbUpdateException → return 500 Internal Server Error with trace ID (no internal details)
  - Catch all other exceptions → return 500 Internal Server Error with trace ID
  - Log all exceptions with correlation ID
  - Never expose stack traces or internal details to external clients
  - _Requirements: 11.8, 13.5, 20.5-20.6_

- [ ] 7.5 Configure dependency injection
  - Register ApplicationDbContext with SQL Server connection string
  - Register IMarketConfigurationProvider → MarketConfigurationProvider (singleton)
  - Register INationalIdValidator → NationalIdValidator (singleton)
  - Register IApplicationDecisionEngine → ApplicationDecisionEngine (singleton)
  - Register IIdentityVerificationService → MockIdentityVerificationService with Polly policies (transient)
  - Register ISanctionsScreeningService → MockSanctionsScreeningService with Polly policies (transient)
  - Register ICoreBank ingService → MockCoreBankingService (scoped)
  - Register ICardOrderingService → MockCardOrderingService (scoped)
  - Register IAuditLogger → AuditLogger (scoped)
  - Register IApplicationRepository → ApplicationRepository (scoped)
  - Register IApplicationValidator → ApplicationValidator (scoped)
  - Register ISubmitApplicationCommandHandler → SubmitApplicationCommandHandler (scoped)
  - Register IApplicationOrchestrator → ApplicationOrchestrator (scoped)
  - _Requirements: 16.1-16.5_

- [ ] 7.6 Configure API middleware pipeline
  - Add HTTPS redirection middleware
  - Add exception handling middleware (custom)
  - Add authentication middleware (placeholder for future)
  - Add authorization middleware (placeholder for future)
  - Add request logging middleware (Serilog)
  - Add Swagger/OpenAPI middleware for development
  - Configure CORS if needed
  - _Requirements: 20.1, 20.4_

---

### 8. Testing - Unit Tests

- [ ] 8.1 Write unit tests for ApplicationDecisionEngine
  - Test approval: Valid doc + face match + clear screening → APPROVED
  - Test rejection: Invalid document → REJECTED (InvalidDocument)
  - Test rejection: Inconclusive document → REJECTED (InconclusiveVerificationResult)
  - Test rejection: Valid doc + face mismatch → REJECTED (FaceMismatch)
  - Test rejection: PossibleMatch screening → REJECTED (PossibleSanctionMatch) in v1
  - Test rejection: Clear sanctions match → REJECTED (SanctionMatch)
  - Test rejection: Provider unavailable → REJECTED (VerificationProviderUnavailable)
  - Test multiple rejection reasons accumulated correctly
  - Test determinism: same inputs produce same outputs
  - _Requirements: 5.1-5.7_

- [ ] 8.2 Write unit tests for NationalIdValidator
  - Test valid national ID for each market (MA-MF) → passes validation
  - Test invalid format for each market → fails validation with correct error
  - Test null/empty national ID → fails validation
  - Test national ID from wrong market → fails validation
  - Test idempotence: multiple calls with same inputs return same result
  - _Requirements: 2.1-2.7_

- [ ] 8.3 Write unit tests for ApplicationValidator
  - Test valid application → ValidationResult.IsValid == true
  - Test missing required fields → ValidationResult.IsValid == false with specific errors
  - Test invalid national ID format → validation error with expected format message
  - Test age < 18 → validation error
  - Test missing documents → validation error
  - Test missing SELFIE → validation error
  - Test invalid base64 image data → validation error
  - Test termsAccepted == false → validation error
  - Test unsupported market code → validation error
  - _Requirements: 1.2-1.8_

- [ ] 8.4 Write unit tests for Application aggregate
  - Test state transitions: Pending → Approved
  - Test state transitions: Pending → Rejected
  - Test invalid state transitions: Approved → Rejected (should throw)
  - Test invalid state transitions: Rejected → Approved (should throw)
  - Test Approve method sets AccountDetails and CardOrder correctly
  - Test Reject method sets Decision with reasons correctly
  - Test business method validation (cannot approve without verification/screening)
  - _Requirements: 5.4-5.5, 12.1_

- [ ] 8.5 Write unit tests for ApplicationOrchestrator
  - Mock all dependencies (IIdentityVerificationService, ISanctionsScreeningService, IApplicationDecisionEngine, etc.)
  - Test happy path: verification + screening → decision → account + card → APPROVED
  - Test rejection path: face mismatch → decision → REJECTED (no account/card created)
  - Test parallel execution: verify both services called concurrently
  - Test audit logging: verify all events logged correctly
  - Test cancellation token handling
  - Test MD market: verify RequiresBranchActivation flag set for card orders
  - _Requirements: 7.4, 8.1-8.5_

---

### 9. Testing - Property-Based Tests (FsCheck)

- [ ]* 9.1 Write property test for application status transitions
  - **Property 1: Application Status Transitions**
  - For any application with status Pending, processing SHALL transition to Approved or Rejected (never remain Pending)
  - Use FsCheck generators to create Pending applications
  - Verify result status is Approved OR Rejected
  - _Validates: Requirements 5.4, 5.5, 8.2_

- [ ]* 9.2 Write property test for approval implies downstream actions
  - **Property 2: Approval Implies Downstream Actions Completed**
  - For any approved application, both AccountDetails and CardOrder SHALL be non-null with valid identifiers
  - Use FsCheck generators to create approved applications
  - Verify AccountDetails != null AND CardOrder != null
  - _Validates: Requirements 6.1, 6.2, 7.1, 7.2_

- [ ]* 9.3 Write property test for rejection implies reasons
  - **Property 3: Rejection Implies At Least One Reason**
  - For any rejected application, at least one rejection reason SHALL be documented
  - Use FsCheck generators to create rejected applications
  - Verify Decision.Reasons.Count >= 1
  - _Validates: Requirements 5.7, 12.9_

- [ ]* 9.4 Write property test for decision determinism
  - **Property 4: Decision Determinism**
  - For any combination of verification, screening, market config, decision SHALL be identical on multiple invocations
  - Use FsCheck generators for inputs
  - Call MakeDecision twice with same inputs
  - Verify decision1 == decision2
  - _Validates: Requirement 5.6_

- [ ]* 9.5 Write property test for national ID validation idempotence
  - **Property 5: National ID Validation is Idempotent**
  - For any national ID and market code, validation SHALL return same result on multiple calls
  - Use FsCheck generators for nationalId and marketCode
  - Call IsValid twice with same inputs
  - Verify result1 == result2
  - _Validates: Requirements 2.1-2.6_

- [ ]* 9.6 Write property test for audit trail completeness
  - **Property 6: All Applications Must Be Audit Logged**
  - For any application created, at least one audit log entry SHALL exist with ApplicationCreated event
  - Use FsCheck generators for applications
  - Persist application
  - Query audit logs by applicationId
  - Verify at least one entry with EventType == "ApplicationCreated"
  - _Validates: Requirements 10.1, 10.9_

- [ ]* 9.7 Write property test for approval criteria satisfaction
  - **Property 7: Approval Criteria Satisfaction**
  - For any application where verification is Valid + face match + clear screening, decision SHALL be APPROVED
  - Use FsCheck generators for valid verification and clear screening
  - Call MakeDecision
  - Verify decision.IsApproved == true
  - _Validates: Requirements 3.3, 4.3, 5.1_

- [ ]* 9.8 Write property test for invalid verification rejection
  - **Property 8: Rejection for Invalid Verification**
  - For any application with Invalid/Inconclusive document OR face mismatch, decision SHALL be REJECTED
  - Use FsCheck generators for invalid verifications
  - Call MakeDecision
  - Verify decision.IsRejected == true
  - _Validates: Requirements 3.4, 3.5, 5.2_

- [ ]* 9.9 Write property test for v1 auto-rejection of PossibleMatch
  - **Property 9: V1 Auto-Rejection for PossibleMatch**
  - For any application with PossibleMatch screening, decision SHALL be REJECTED with PossibleSanctionMatch reason
  - Use FsCheck generators for PossibleMatch screening
  - Call MakeDecision
  - Verify decision.IsRejected == true AND Reasons.Contains(PossibleSanctionMatch)
  - _Validates: Requirements 4.4, 19.2_

- [ ]* 9.10 Write property test for market-specific national ID validation
  - **Property 10: Market-Specific National ID Validation**
  - For any national ID and market, validation SHALL succeed only when ID matches market-specific regex
  - Use FsCheck generators for nationalId and marketCode
  - Call IsValid
  - Verify result matches Regex.IsMatch with market pattern
  - _Validates: Requirements 2.1-2.7_

- [ ]* 9.11 Write property test for MD market branch activation flag
  - **Property 11: MD Market Branch Activation Flag**
  - For any approved application in market MD, card order SHALL have RequiresBranchActivation == true
  - Use FsCheck generators for MD market applications
  - Process to approval
  - Verify CardOrder.RequiresBranchActivation == true
  - _Validates: Requirement 7.4_

- [ ]* 9.12 Write property test for idempotency of external operations
  - **Property 12: Idempotency of External Operations**
  - For any external operation with same idempotency key, multiple invocations SHALL produce same result
  - Use FsCheck generators for AccountCreationRequest
  - Call OpenAccountAsync twice with same ApplicationId
  - Verify result1.AccountId == result2.AccountId
  - _Validates: Requirements 6.3, 7.3, 15.1-15.3_

- [ ]* 9.13 Write property test for application submission creates new resources
  - **Property 13: Application Submission Creates New Resources**
  - For any application data submitted multiple times, each submission SHALL create distinct application with unique ID
  - Use FsCheck generators for SubmitApplicationCommand
  - Call Handle twice with same command
  - Verify result1.ApplicationId != result2.ApplicationId
  - _Validates: Requirement 15.4_

- [ ]* 9.14 Write property test for comprehensive audit trail
  - **Property 14: Comprehensive Audit Trail for Lifecycle Events**
  - For any application lifecycle event, audit log entry SHALL exist with event type and relevant identifiers
  - Use FsCheck generators for applications
  - Process application through orchestrator
  - Query audit logs by applicationId
  - Verify entries exist for: VerificationCompleted, ScreeningCompleted, DecisionMade
  - _Validates: Requirements 10.2-10.7_

- [ ]* 9.15 Write property test for validation fails fast
  - **Property 15: Validation Fails Fast Without External Calls**
  - For any application with validation errors, system SHALL reject without invoking external services
  - Use FsCheck generators for invalid applications
  - Mock IIdentityVerificationService and ISanctionsScreeningService
  - Attempt to submit application (expect ValidationException)
  - Verify mocks never called
  - _Validates: Requirement 11.7_

---

### 10. Testing - Integration Tests

- [ ]* 10.1 Write integration test for happy path approval flow
  - Create WebApplicationFactory<Program>
  - Create valid SubmitApplicationRequest (MB market, valid national ID)
  - POST /applications
  - Assert 201 Created
  - Assert response.Status == "APPROVED"
  - Assert response.ApplicationId is valid Guid
  - Assert Location header is correct
  - GET /applications/{id}
  - Assert 200 OK with full details including accountId
  - _Requirements: 1.1, 5.1, 6.1, 7.1, 8.4, 9.1-9.5, 13.1-13.3_

- [ ]* 10.2 Write integration test for rejection flow (face mismatch)
  - Configure MockIdentityVerificationService to return face mismatch
  - Create valid SubmitApplicationRequest
  - POST /applications
  - Assert 201 Created
  - Assert response.Status == "REJECTED"
  - GET /applications/{id}
  - Assert rejectionReasons contains "FaceMismatch"
  - _Requirements: 3.5, 5.2, 9.3_

- [ ]* 10.3 Write integration test for validation errors
  - Create invalid SubmitApplicationRequest (invalid national ID for MB market)
  - POST /applications
  - Assert 400 Bad Request
  - Assert response contains RFC 9110 Problem Details
  - Assert errors.NationalId contains market-specific format message
  - _Requirements: 1.7, 2.2, 2.7, 13.4_

- [ ]* 10.4 Write integration test for provider timeout handling
  - Configure MockIdentityVerificationService to simulate timeout (delay > 10s)
  - Create valid SubmitApplicationRequest
  - POST /applications
  - Assert 201 Created
  - Assert response.Status == "REJECTED"
  - GET /applications/{id}
  - Assert rejectionReasons contains "VerificationProviderUnavailable"
  - _Requirements: 3.6, 11.1, 11.6, 17.1_

- [ ]* 10.5 Write integration test for GET application by ID (not found)
  - GET /applications/{nonexistent-guid}
  - Assert 404 Not Found
  - _Requirements: 9.4, 13.8_

- [ ]* 10.6 Write integration test for MD market branch activation
  - Create valid SubmitApplicationRequest for MD market
  - Configure mocks to return approval conditions
  - POST /applications
  - Assert 201 Created with APPROVED status
  - Query database for CardOrder
  - Assert CardOrder.RequiresBranchActivation == true
  - _Requirements: 7.4, 14.6_

- [ ]* 10.7 Write integration test for audit logging
  - Create valid SubmitApplicationRequest
  - POST /applications
  - Query AuditLogs table by applicationId
  - Assert entries exist for: ApplicationCreated, VerificationCompleted, ScreeningCompleted, DecisionMade, AccountOpened, CardOrdered
  - Assert all entries have correlation ID
  - _Requirements: 10.1-10.7, 18.2_

---

### 11. Error Handling and Resilience

- [ ] 11.1 Implement retry policy testing for identity verification
  - Configure mock to fail first 2 attempts, succeed on 3rd
  - Verify retry policy invoked correctly with exponential backoff
  - Verify final result is success after retries
  - Verify warning logs emitted for retries
  - _Requirements: 11.1, 11.3_

- [ ] 11.2 Implement retry policy testing for sanctions screening
  - Configure mock to fail first 2 attempts, succeed on 3rd
  - Verify retry policy invoked correctly
  - Verify final result is success after retries
  - _Requirements: 11.2, 11.4_

- [ ] 11.3 Implement provider unavailability handling
  - Configure mocks to fail all retry attempts
  - Verify application rejected with VerificationProviderUnavailable / ScreeningProviderUnavailable
  - Verify error logged with full context
  - _Requirements: 11.6_

- [ ] 11.4 Implement database retry policy testing
  - Simulate transient database connection failure
  - Verify EF Core retry policy invoked (up to 3 times)
  - Verify successful operation after transient failure
  - _Requirements: 11.5_

- [ ] 11.5 Implement validation error fast-fail testing
  - Submit application with validation errors
  - Verify ValidationException thrown immediately
  - Verify no external provider calls made (using mocks)
  - _Requirements: 11.7_

- [ ] 11.6 Implement unhandled exception handling
  - Simulate unhandled exception in orchestrator
  - Verify 500 Internal Server Error returned with trace ID
  - Verify exception logged with correlation ID
  - Verify no internal details exposed to client
  - _Requirements: 11.8, 20.6_

---

### 12. Documentation

- [ ] 12.1 Write README.md
  - Project overview and purpose
  - Architecture diagram (ASCII or link to mermaid)
  - Prerequisites: .NET 8 SDK, SQL Server, Seq
  - How to build: `dotnet build`
  - How to run database migrations: `dotnet ef database update`
  - How to run the API: `dotnet run --project Atlas.Api`
  - How to run tests: `dotnet test`
  - How to access Swagger UI: `https://localhost:5001/swagger`
  - How to access Seq: `http://localhost:5341`
  - Sample API requests with curl examples
  - Market codes and national ID formats table
  - _Requirements: All (general documentation)_

- [ ] 12.2 Write REASONING.md
  - Explain design-first workflow choice
  - Explain synchronous vs async trade-off (v1 auto-reject POSSIBLE_MATCH)
  - Explain mock providers vs real integrations
  - Explain clean architecture layer separation
  - Explain pure function decision engine for testability
  - Explain idempotency strategy for external operations
  - Explain 10-year audit retention strategy (AuditLogs table + Seq)
  - Explain data sovereignty approach (market code tagging)
  - Explain MD market branch activation special case
  - Reference design document for detailed rationale
  - _Requirements: 19.1-19.5 (v1 scope and extensibility)_

- [ ] 12.3 Write WHERE-TO-LOOK.md
  - Map requirements to implementation files
  - Decision engine logic: `Atlas.Domain/DecisionEngine/ApplicationDecisionEngine.cs`
  - Validation logic: `Atlas.Application/Validation/ApplicationValidator.cs`, `Atlas.Domain/Validation/NationalIdValidator.cs`
  - Orchestration: `Atlas.Application/Orchestration/ApplicationOrchestrator.cs`
  - API contracts: `Atlas.Api/Contracts/`
  - Database schema: `Atlas.Infrastructure/Data/Migrations/`
  - Mock providers: `Atlas.Infrastructure/ExternalServices/Mock*/`
  - Audit logging: `Atlas.Infrastructure/Logging/AuditLogger.cs`
  - Unit tests: `Atlas.Tests.Unit/`
  - Property tests: `Atlas.Tests.Unit/Properties/`
  - Integration tests: `Atlas.Tests.Integration/`
  - _Requirements: All (traceability)_

- [ ] 12.4 Write KNOWN-LIMITATIONS.md
  - v1 auto-rejects POSSIBLE_MATCH (compliance preference for manual review deferred)
  - Mock providers (IDNow, World-Check not integrated)
  - Seam-only core banking and card service (no real account/card creation)
  - No authentication/authorization (middleware placeholders only)
  - No async processing (all synchronous in v1)
  - No application amendments or withdrawals
  - No multi-language support (English only)
  - No delivery address capture for card orders
  - No retry mechanism for failed account/card operations
  - No advanced analytics or reporting
  - In-memory idempotency tracking (not persisted, lost on restart)
  - Single instance deployment (no distributed locking for idempotency)
  - _Requirements: 19.1-19.5 (v1 scope), Out of Scope section_

- [ ] 12.5 Create architecture diagram and update design document references
  - Create ASCII art or mermaid diagram for README
  - Document project structure (solution/projects)
  - Document dependency flow (API → Application → Domain ← Infrastructure)
  - Add links between README, REASONING, WHERE-TO-LOOK, KNOWN-LIMITATIONS
  - _Requirements: All (documentation completeness)_

---

## Checkpoint Tasks

- [ ] 13. Checkpoint - Day 1 End
  - Build solution successfully (no compilation errors)
  - Database migrations applied successfully
  - API runs and Swagger UI accessible
  - Happy path skeleton works: POST /applications → 201 Created with Pending status
  - Seq receives structured logs
  - Review progress with user, address any questions

- [ ] 14. Checkpoint - Day 2 End
  - All layers implemented (Domain, Application, Infrastructure, API)
  - Mock providers return configurable scenarios
  - Decision engine implements all approval/rejection rules
  - Complete flow works: POST /applications → verification + screening → decision → account + card → APPROVED/REJECTED
  - GET /applications/{id} returns correct details
  - Audit logging captures all events
  - Error handling middleware returns proper HTTP responses
  - Build solution successfully
  - Manual smoke test with Postman/curl for happy path and rejection scenarios
  - Review progress with user, address any questions

- [ ] 15. Final Checkpoint - Day 3 End
  - All unit tests passing (decision engine, validators, domain model)
  - All property-based tests passing (15 FsCheck properties)
  - All integration tests passing (happy path, rejection, validation errors, timeouts, MD market)
  - Error handling and resilience tests passing
  - Documentation complete (README, REASONING, WHERE-TO-LOOK, KNOWN-LIMITATIONS)
  - Build solution successfully with no warnings
  - Run full test suite: `dotnet test` → all green
  - Code coverage report generated (optional: coverlet + ReportGenerator)
  - Final review: Does the implementation demonstrate solid architectural thinking for a 3-day assessment?
  - User can run system from README instructions
  - System ready for evaluation

---

## Notes

- **Tasks marked with `*` are optional** and can be skipped for faster MVP completion. However, for a technical assessment demonstrating architectural quality, completing property-based tests and integration tests is highly recommended.
- **Core implementation tasks (NOT marked with `*)` are required** and form the working backend system.
- Each task references specific requirements for traceability back to the requirements document.
- **Checkpoints ensure incremental validation** at day boundaries to catch issues early.
- **Property-based tests use FsCheck** as specified in the design document, providing strong correctness guarantees.
- **3-day timeline is aggressive but achievable** with focus: Day 1 (foundation + database + domain core), Day 2 (complete flow + all layers), Day 3 (testing + documentation).
- **The implementation demonstrates architectural thinking** through clean architecture, pure functions, mock-friendly design, comprehensive testing, and forward-thinking extensibility.
- **When build is needed:** The user should run `dotnet build` in Visual Studio or command line after tasks that create/modify code (after 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 7.1, 8.1, etc.). Don't build the solution in VS automatically - just tell them when build is needed.

## Task Dependency Graph

```json
{
  "waves": [
    {
      "id": 0,
      "tasks": ["1.1", "1.2", "1.3", "1.4"]
    },
    {
      "id": 1,
      "tasks": ["2.1", "2.2", "3.1", "3.2"]
    },
    {
      "id": 2,
      "tasks": ["2.3", "2.4", "3.3", "3.4"]
    },
    {
      "id": 3,
      "tasks": ["2.5", "4.1", "4.2"]
    },
    {
      "id": 4,
      "tasks": ["4.3", "5.1", "5.2", "5.4", "5.5"]
    },
    {
      "id": 5,
      "tasks": ["5.3", "5.6", "5.7"]
    },
    {
      "id": 6,
      "tasks": ["6.1"]
    },
    {
      "id": 7,
      "tasks": ["6.2", "6.3"]
    },
    {
      "id": 8,
      "tasks": ["7.1", "7.2", "7.3", "7.4"]
    },
    {
      "id": 9,
      "tasks": ["7.5", "7.6"]
    },
    {
      "id": 10,
      "tasks": ["8.1", "8.2", "8.3", "8.4"]
    },
    {
      "id": 11,
      "tasks": ["8.5", "9.1", "9.2", "9.3", "9.4", "9.5"]
    },
    {
      "id": 12,
      "tasks": ["9.6", "9.7", "9.8", "9.9", "9.10"]
    },
    {
      "id": 13,
      "tasks": ["9.11", "9.12", "9.13", "9.14", "9.15"]
    },
    {
      "id": 14,
      "tasks": ["10.1", "10.2", "10.3", "10.4", "10.5"]
    },
    {
      "id": 15,
      "tasks": ["10.6", "10.7", "11.1", "11.2", "11.3"]
    },
    {
      "id": 16,
      "tasks": ["11.4", "11.5", "11.6"]
    },
    {
      "id": 17,
      "tasks": ["12.1", "12.2", "12.3", "12.4", "12.5"]
    }
  ]
}
```
