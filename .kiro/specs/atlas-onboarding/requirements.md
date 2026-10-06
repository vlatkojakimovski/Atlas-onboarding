# Requirements Document

## Feature: Atlas Onboarding Backend System

## Introduction

Atlas is a digital customer onboarding platform for a banking group operating across six markets (MA through MF). The system automates customer application processing through identity verification and sanctions screening, delivering approved applications with account creation and card ordering within 3 minutes. This requirements document formalizes the functional, non-functional, API, data, and integration requirements that the technical design satisfies.

**Scope (Phase 1/v1):**
- Synchronous, fully automated application processing
- Auto-rejection of POSSIBLE_MATCH sanctions results (pragmatic trade-off for speed)
- Mock implementations for IDNow and World-Check integrations
- Seam-only implementations for core banking and card service integrations
- Architecture prepared for future async manual review workflow

## Glossary

- **Application_System**: The Atlas onboarding backend service
- **Identity_Verification_Service**: IDNow integration for document and face verification
- **Sanctions_Screening_Service**: World-Check integration for sanctions and PEP screening
- **Decision_Engine**: Business rules engine for approval/rejection decisions
- **Core_Banking_Service**: Adapter for account creation in core banking system
- **Card_Service**: Adapter for card ordering system
- **Audit_Logger**: Comprehensive audit logging component for compliance
- **Application**: Customer onboarding application aggregate containing personal data, documents, verification results, and decision outcomes
- **Market**: Geographic market identified by two-letter code (MA-MF)
- **National_ID**: Government-issued national identification number with market-specific format
- **Document_Verification_Status**: Result of document authenticity check (Valid, Invalid, Inconclusive)
- **Screening_Status**: Result of sanctions screening (Clear, PossibleMatch)
- **Application_Status**: State of application (Pending, Approved, Rejected, PendingReview)
- **Provider**: External service for verification or screening
- **Idempotency**: Property ensuring repeated operations produce same result

---

## Requirements

### Requirement 1: Application Submission

**User Story:** As a mobile app user, I want to submit a customer onboarding application with my personal details and identity documents, so that I can open a bank account digitally.

#### Acceptance Criteria

1. WHEN a user submits an application with valid data THEN THE Application_System SHALL create a new application with status Pending and return an application identifier
2. WHEN a user submits an application THEN THE Application_System SHALL validate that first name, last name, date of birth, market code, national ID, email, phone, and at least two documents are provided
3. WHEN a user submits an application THEN THE Application_System SHALL validate that the date of birth indicates the applicant is at least 18 years old
4. WHEN a user submits an application THEN THE Application_System SHALL validate that the market code is one of the supported markets (MA, MB, MC, MD, ME, MF)
5. WHEN a user submits an application THEN THE Application_System SHALL validate that at least one identity document (PASSPORT or ID_CARD) and one SELFIE document are provided
6. WHEN a user submits an application THEN THE Application_System SHALL validate that document images are provided in valid base64 encoding
7. WHEN a user submits an application with invalid data THEN THE Application_System SHALL return a 400 Bad Request response with field-specific validation error messages
8. WHEN a user submits an application THEN THE Application_System SHALL validate that termsAccepted is true

### Requirement 2: Market-Specific Validation

**User Story:** As a compliance officer, I want applications to be validated against market-specific rules, so that we collect valid identification for each geographic market.

#### Acceptance Criteria

1. WHERE market code is MA, WHEN a user submits a national ID THEN THE Application_System SHALL validate the national ID matches the pattern of 10 digits
2. WHERE market code is MB, WHEN a user submits a national ID THEN THE Application_System SHALL validate the national ID matches the pattern of 13 digits
3. WHERE market code is MC, WHEN a user submits a national ID THEN THE Application_System SHALL validate the national ID matches the pattern of 2 letters followed by 6 digits
4. WHERE market code is MD, WHEN a user submits a national ID THEN THE Application_System SHALL validate the national ID matches the pattern of 9 digits
5. WHERE market code is ME, WHEN a user submits a national ID THEN THE Application_System SHALL validate the national ID matches the pattern of 11 digits
6. WHERE market code is MF, WHEN a user submits a national ID THEN THE Application_System SHALL validate the national ID matches the pattern of 1 letter followed by 7 digits
7. WHEN a national ID does not match the market-specific pattern THEN THE Application_System SHALL reject the application with a validation error indicating the expected format

### Requirement 3: Identity Verification

**User Story:** As a risk officer, I want to verify the authenticity of identity documents and confirm face matching, so that we prevent identity fraud.

#### Acceptance Criteria

1. WHEN an application is submitted THEN THE Application_System SHALL invoke the Identity_Verification_Service with the identity document image and selfie image
2. WHEN the Identity_Verification_Service responds THEN THE Application_System SHALL persist the verification result including document status, face match result, confidence score, and provider identifier
3. WHEN the Identity_Verification_Service returns a Valid document status and face match true THEN THE Application_System SHALL consider the identity verification requirement satisfied for approval
4. WHEN the Identity_Verification_Service returns Invalid or Inconclusive document status THEN THE Decision_Engine SHALL reject the application
5. WHEN the Identity_Verification_Service returns Valid document but face match false THEN THE Decision_Engine SHALL reject the application
6. WHEN the Identity_Verification_Service is unavailable after retry attempts THEN THE Decision_Engine SHALL reject the application with reason VerificationProviderUnavailable

### Requirement 4: Sanctions and PEP Screening

**User Story:** As a compliance officer, I want to screen applicants against sanctions lists and politically exposed persons (PEP) databases, so that we comply with anti-money laundering regulations.

#### Acceptance Criteria

1. WHEN an application is submitted THEN THE Application_System SHALL invoke the Sanctions_Screening_Service with the applicant's first name, last name, date of birth, and nationality
2. WHEN the Sanctions_Screening_Service responds THEN THE Application_System SHALL persist the screening result including case identifier, screening status, any matches, and screened timestamp
3. WHEN the Sanctions_Screening_Service returns Clear status THEN THE Application_System SHALL consider the sanctions screening requirement satisfied for approval
4. WHEN the Sanctions_Screening_Service returns PossibleMatch status THEN THE Decision_Engine SHALL reject the application in version 1
5. WHEN the Sanctions_Screening_Service identifies a high-confidence sanctions match THEN THE Decision_Engine SHALL reject the application with reason SanctionMatch
6. WHEN the Sanctions_Screening_Service is unavailable after retry attempts THEN THE Decision_Engine SHALL reject the application with reason ScreeningProviderUnavailable

### Requirement 5: Application Decision Logic

**User Story:** As a system operator, I want applications to be automatically approved or rejected based on verification and screening results, so that eligible customers can be onboarded without manual intervention.

#### Acceptance Criteria

1. WHEN identity verification returns Valid document AND face match true AND sanctions screening returns Clear THEN THE Decision_Engine SHALL approve the application
2. WHEN any of the following conditions occur THEN THE Decision_Engine SHALL reject the application: invalid document, inconclusive document, face mismatch, PossibleMatch screening result, or provider unavailability
3. WHEN the Decision_Engine makes a decision THEN THE Application_System SHALL persist the decision status and any rejection reasons
4. WHEN the Decision_Engine approves an application THEN THE Application_System SHALL transition the application status from Pending to Approved
5. WHEN the Decision_Engine rejects an application THEN THE Application_System SHALL transition the application status from Pending to Rejected
6. THE Decision_Engine SHALL produce deterministic results for the same input parameters
7. WHEN an application is rejected THEN THE Application_System SHALL document at least one rejection reason

### Requirement 6: Account Creation for Approved Applications

**User Story:** As an approved applicant, I want a bank account automatically created for me, so that I can start using banking services immediately.

#### Acceptance Criteria

1. WHEN an application is approved THEN THE Application_System SHALL invoke the Core_Banking_Service to create a new account with the applicant's personal details
2. WHEN the Core_Banking_Service successfully creates an account THEN THE Application_System SHALL persist the account identifier and account number
3. WHEN the Core_Banking_Service creates an account THEN THE Application_System SHALL use the application identifier as the idempotency key to prevent duplicate account creation
4. WHEN the Core_Banking_Service fails to create an account THEN THE Application_System SHALL log the failure and reject the application with technical reason
5. WHEN an account is created THEN THE Audit_Logger SHALL log the account creation event with application identifier and account identifier

### Requirement 7: Card Ordering for Approved Applications

**User Story:** As an approved applicant, I want a bank card automatically ordered for me, so that I can access my account via ATM and point-of-sale terminals.

#### Acceptance Criteria

1. WHEN an application is approved and an account is created THEN THE Application_System SHALL invoke the Card_Service to order a card linked to the account
2. WHEN the Card_Service successfully orders a card THEN THE Application_System SHALL persist the card order identifier and card reference
3. WHEN the Card_Service orders a card THEN THE Application_System SHALL use the account identifier as the idempotency key to prevent duplicate card orders
4. WHERE market code is MD, WHEN a card is ordered THEN THE Application_System SHALL mark the card order with requiresBranchActivation flag set to true
5. WHEN the Card_Service fails to order a card after account creation THEN THE Application_System SHALL still mark the application as approved and log the card ordering failure for retry
6. WHEN a card is ordered THEN THE Audit_Logger SHALL log the card order event with application identifier and card order identifier

### Requirement 8: Application Processing Orchestration

**User Story:** As a system architect, I want application processing to execute verification and screening in parallel, so that we minimize end-to-end processing time.

#### Acceptance Criteria

1. WHEN an application is processed THEN THE Application_System SHALL invoke the Identity_Verification_Service and Sanctions_Screening_Service concurrently
2. WHEN both verification and screening complete THEN THE Application_System SHALL proceed to decision logic
3. WHEN an application is approved THEN THE Application_System SHALL execute account creation followed by card ordering sequentially
4. THE Application_System SHALL complete end-to-end processing within 3 minutes for the majority of applications
5. WHEN application processing encounters a cancellation request THEN THE Application_System SHALL respect the cancellation token and perform cleanup

### Requirement 9: Application Status Retrieval

**User Story:** As a mobile app user, I want to retrieve the current status of my application, so that I can know whether it was approved or rejected.

#### Acceptance Criteria

1. WHEN a user requests an application by identifier THEN THE Application_System SHALL return the application status, personal details, creation timestamp, and completion timestamp if available
2. WHERE application status is Approved, WHEN a user requests the application THEN THE Application_System SHALL include the account identifier in the response
3. WHERE application status is Rejected, WHEN a user requests the application THEN THE Application_System SHALL include rejection reasons in the response
4. WHEN a user requests an application that does not exist THEN THE Application_System SHALL return a 404 Not Found response
5. WHEN a user requests an application THEN THE Application_System SHALL return a 200 OK response with the application details

### Requirement 10: Audit Logging for Compliance

**User Story:** As a compliance officer, I want comprehensive audit logs of all application events and data access, so that I can demonstrate regulatory compliance and investigate issues.

#### Acceptance Criteria

1. WHEN an application is created THEN THE Audit_Logger SHALL log the application created event with application identifier, market code, and user identifier
2. WHEN identity verification completes THEN THE Audit_Logger SHALL log the verification result including document status, face match result, and provider identifier
3. WHEN sanctions screening completes THEN THE Audit_Logger SHALL log the screening result including screening status, case identifier, and any matches
4. WHEN a decision is made THEN THE Audit_Logger SHALL log the decision status and any rejection reasons
5. WHEN an account is created THEN THE Audit_Logger SHALL log the account creation event
6. WHEN a card is ordered THEN THE Audit_Logger SHALL log the card order event
7. WHEN application status changes THEN THE Audit_Logger SHALL log the status transition with previous status, new status, and reason
8. WHEN data is accessed THEN THE Audit_Logger SHALL log who accessed what resource when
9. THE Audit_Logger SHALL emit structured logs with correlation identifiers for request tracing
10. THE Application_System SHALL support 10-year retention of audit logs for compliance requirements

### Requirement 11: Error Handling and Resilience

**User Story:** As a system operator, I want the system to handle transient failures gracefully with retry policies, so that temporary issues do not unnecessarily reject valid applications.

#### Acceptance Criteria

1. WHEN the Identity_Verification_Service request times out THEN THE Application_System SHALL retry the request up to 3 times with exponential backoff
2. WHEN the Sanctions_Screening_Service request times out THEN THE Application_System SHALL retry the request up to 3 times with exponential backoff
3. WHEN the Identity_Verification_Service returns a 5xx error THEN THE Application_System SHALL retry the request according to the retry policy
4. WHEN the Sanctions_Screening_Service returns a 5xx error THEN THE Application_System SHALL retry the request according to the retry policy
5. WHEN database connection fails transiently THEN THE Application_System SHALL retry the operation up to 3 times
6. WHEN a provider is unavailable after all retry attempts THEN THE Application_System SHALL reject the application and log the provider failure
7. WHEN a validation error occurs THEN THE Application_System SHALL fail fast without invoking external providers
8. WHEN an unhandled exception occurs during processing THEN THE Application_System SHALL return a 500 Internal Server Error with a trace identifier

### Requirement 12: Data Persistence and Schema

**User Story:** As a database administrator, I want application data stored in a relational database with proper referential integrity, so that we maintain data consistency and enable efficient querying.

#### Acceptance Criteria

1. THE Application_System SHALL persist applications with identifier, personal details, market code, status, created timestamp, and completed timestamp
2. THE Application_System SHALL persist documents with identifier, application reference, document type, image data, and uploaded timestamp
3. THE Application_System SHALL persist identity verification results with identifier, application reference, provider identifier, document status, face match, confidence, and verified timestamp
4. THE Application_System SHALL persist sanctions screening results with identifier, application reference, case identifier, screening status, and screened timestamp
5. WHEN screening identifies potential matches THEN THE Application_System SHALL persist each match with type, score, and subject
6. THE Application_System SHALL persist account details with identifier, application reference, account identifier, account number, and created timestamp
7. THE Application_System SHALL persist card orders with identifier, application reference, account identifier, card order identifier, card reference, branch activation flag, and ordered timestamp
8. THE Application_System SHALL persist application decisions with identifier, application reference, decision status, and decided timestamp
9. WHEN an application is rejected THEN THE Application_System SHALL persist each rejection reason linked to the decision
10. THE Application_System SHALL enforce referential integrity with foreign key constraints between related entities

### Requirement 13: API Design and HTTP Semantics

**User Story:** As a mobile app developer, I want RESTful APIs with proper HTTP semantics and JSON payloads, so that I can integrate with the onboarding system using standard conventions.

#### Acceptance Criteria

1. THE Application_System SHALL expose a POST /applications endpoint for submitting new applications
2. WHEN a valid application is submitted THEN THE Application_System SHALL return HTTP 201 Created with the application identifier in the response body
3. WHEN a valid application is submitted THEN THE Application_System SHALL include a Location header with the URI of the created application resource
4. WHEN an invalid application is submitted THEN THE Application_System SHALL return HTTP 400 Bad Request with detailed validation errors
5. WHEN an internal error occurs THEN THE Application_System SHALL return HTTP 500 Internal Server Error with a trace identifier
6. THE Application_System SHALL expose a GET /applications/{id} endpoint for retrieving application status
7. WHEN an existing application is requested THEN THE Application_System SHALL return HTTP 200 OK with application details
8. WHEN a non-existent application is requested THEN THE Application_System SHALL return HTTP 404 Not Found
9. THE Application_System SHALL accept and return JSON payloads
10. THE Application_System SHALL follow RFC 9110 Problem Details format for error responses

### Requirement 14: Data Sovereignty and Multi-Market Support

**User Story:** As a compliance officer, I want application data tagged with market codes and audit logs filterable by market, so that we can comply with data sovereignty regulations.

#### Acceptance Criteria

1. THE Application_System SHALL store the market code with every application record
2. THE Application_System SHALL include the market code in all audit log entries related to applications
3. THE Application_System SHALL support efficient querying of applications by market code through database indexing
4. THE Application_System SHALL load market-specific configuration from a centralized configuration file
5. THE Application_System SHALL support six markets identified by codes MA, MB, MC, MD, ME, and MF
6. WHERE market configuration contains a RequiresBranchActivation flag, THE Application_System SHALL apply market-specific logic for card activation
7. THE Application_System SHALL use a single codebase with configuration-driven market differences

### Requirement 15: Idempotency for External Operations

**User Story:** As a system architect, I want external operations to be idempotent, so that retries and duplicate requests do not create duplicate accounts or card orders.

#### Acceptance Criteria

1. WHEN the Core_Banking_Service is invoked THEN THE Application_System SHALL use the application identifier as the idempotency key
2. WHEN the Card_Service is invoked THEN THE Application_System SHALL use the account identifier as the idempotency key
3. WHEN an idempotent operation is retried with the same key THEN THE Application_System SHALL receive the same result without creating duplicate resources
4. WHEN a client resubmits an application THEN THE Application_System SHALL create a new application with a new identifier (application submission itself is not idempotent by design)

### Requirement 16: Configuration Management

**User Story:** As a DevOps engineer, I want market-specific rules and provider settings externalized in configuration files, so that I can deploy changes without code modifications.

#### Acceptance Criteria

1. THE Application_System SHALL load market configurations from appsettings.json at startup
2. WHERE a market configuration exists, THE Application_System SHALL include country name, national ID validation pattern, national ID description, example, and branch activation flag
3. THE Application_System SHALL validate configuration completeness at startup and fail fast if markets are not properly configured
4. THE Application_System SHALL support adding new markets by adding configuration entries without code changes
5. THE Application_System SHALL reload configuration when the configuration file changes without requiring application restart

### Requirement 17: Performance and Timeout Management

**User Story:** As a mobile app user, I want my application processed quickly, so that I receive approval or rejection within a reasonable time.

#### Acceptance Criteria

1. THE Application_System SHALL configure identity verification requests with a 10-second timeout
2. THE Application_System SHALL configure sanctions screening requests with a 10-second timeout
3. THE Application_System SHALL execute identity verification and sanctions screening concurrently to minimize total processing time
4. THE Application_System SHALL complete end-to-end processing within 3 minutes for applications that do not encounter provider failures
5. WHEN a provider request exceeds the configured timeout THEN THE Application_System SHALL cancel the request and proceed with retry logic

### Requirement 18: Logging and Observability

**User Story:** As a system operator, I want structured logs with correlation identifiers, so that I can trace requests across components and troubleshoot issues efficiently.

#### Acceptance Criteria

1. THE Application_System SHALL emit structured logs in JSON format
2. THE Application_System SHALL include a correlation identifier in all logs related to a single application processing request
3. THE Application_System SHALL include user context in logs when available
4. THE Application_System SHALL include market context in logs for application-related operations
5. THE Application_System SHALL send logs to a centralized log aggregation system (Seq)
6. THE Application_System SHALL log information-level events for application lifecycle transitions
7. THE Application_System SHALL log warning-level events for retry attempts
8. THE Application_System SHALL log error-level events for provider failures and unhandled exceptions

### Requirement 19: Version 1 Scope and Future Extensibility

**User Story:** As a product owner, I want the system architecture to support future manual review workflows, so that we can upgrade from auto-reject to manual review of POSSIBLE_MATCH cases.

#### Acceptance Criteria

1. THE Application_System SHALL include a PendingReview status in the application status enumeration but not use it in version 1
2. WHEN the Sanctions_Screening_Service returns PossibleMatch THEN THE Application_System SHALL reject the application in version 1
3. THE Application_System database schema SHALL include columns and constraints that support the PendingReview status
4. THE Application_System architecture SHALL separate decision logic from orchestration logic to enable future workflow modifications
5. THE Application_System SHALL document the version 1 trade-off of auto-rejecting POSSIBLE_MATCH cases in technical documentation

### Requirement 20: Security and Data Protection

**User Story:** As a security officer, I want sensitive customer data protected in transit and at rest, so that we prevent unauthorized access and data breaches.

#### Acceptance Criteria

1. THE Application_System SHALL accept requests only over HTTPS
2. THE Application_System SHALL store document images in the database with appropriate access controls
3. THE Application_System SHALL not log sensitive personal data (national ID, full document images) in plain text logs
4. THE Application_System SHALL include authentication and authorization middleware in the request pipeline
5. WHEN validation errors occur THEN THE Application_System SHALL not expose internal system details in error messages
6. WHEN unhandled exceptions occur THEN THE Application_System SHALL return a generic error message without stack traces to external clients

---

## Requirements Traceability

This requirements document is derived from the technical design document and maintains traceability to design decisions:

| Requirement | Design Section | Design Component |
|-------------|----------------|------------------|
| Req 1 | API Contracts, Component 5 | ApplicationValidator |
| Req 2 | Market Configuration, Component 5 | NationalIdValidator |
| Req 3 | Component 3, Algorithm 1 | IdentityVerificationService |
| Req 4 | Component 4, Algorithm 1 | SanctionsScreeningService |
| Req 5 | Component 2, Algorithm 2 | ApplicationDecisionEngine |
| Req 6 | Component 6, Algorithm 1 | CoreBankingService |
| Req 7 | Component 7, Algorithm 1 | CardOrderingService |
| Req 8 | Component 1, Algorithm 1 | ApplicationOrchestrator |
| Req 9 | API Contracts | ApplicationsController GET |
| Req 10 | Component 8, Data Models | AuditLogger |
| Req 11 | Error Handling | Polly retry policies |
| Req 12 | Data Models, Database Schema | SQL Server schema |
| Req 13 | API Contracts | API design |
| Req 14 | Market Configuration, Data Models | Market configuration |
| Req 15 | Component 6, Component 7 | Idempotency patterns |
| Req 16 | Market Configuration | appsettings.json |
| Req 17 | Algorithm 1, Error Handling | Timeout and performance |
| Req 18 | Component 8 | Logging and observability |
| Req 19 | Overview, Component 2 | Future extensibility |
| Req 20 | API Contracts, Error Handling | Security |

---

## Non-Functional Requirements Summary

### Performance
- **Response Time:** End-to-end processing < 3 minutes for 95th percentile
- **Concurrent Processing:** Identity verification and sanctions screening execute in parallel
- **Timeout Management:** Individual provider requests timeout after 10 seconds

### Availability
- **Retry Policies:** 3 retry attempts with exponential backoff for transient failures
- **Graceful Degradation:** Provider unavailability results in application rejection with clear reason

### Security
- **Transport Security:** HTTPS only
- **Data Protection:** Sensitive data not logged in plain text
- **Access Control:** Authentication and authorization middleware

### Compliance
- **Audit Logging:** Comprehensive logging of all application events and data access
- **Data Retention:** 10-year retention for audit logs
- **Data Sovereignty:** Market code tagging and filtering support

### Maintainability
- **Clean Architecture:** Separation of concerns across layers
- **Configuration-Driven:** Market-specific rules externalized
- **Single Codebase:** No code forks for different markets

### Testability
- **Pure Functions:** Decision engine has no side effects
- **Mock-Friendly:** External integrations abstracted behind interfaces
- **Property-Based Testing:** Correctness properties defined for business logic

---

## Out of Scope (Version 1)

The following items are explicitly out of scope for version 1 but supported by the architecture for future implementation:

1. **Manual Review Workflow:** POSSIBLE_MATCH cases are auto-rejected in v1; manual review for compliance officers is deferred
2. **Real Provider Integrations:** IDNow and World-Check are mocked; real API integrations deferred
3. **Core Banking Integration:** Account creation is seam-only; real integration with banking core deferred
4. **Card Service Integration:** Card ordering is seam-only; real integration with card issuer deferred
5. **Delivery Address Capture:** Card delivery address not captured in v1
6. **Application Amendments:** Cannot modify submitted applications
7. **Application Withdrawal:** Users cannot withdraw pending applications
8. **Async Processing:** All processing is synchronous in v1
9. **Multi-Language Support:** English only in v1
10. **Advanced Analytics:** No reporting or analytics dashboards in v1

---

## Assumptions and Dependencies

### Assumptions
1. Mobile app handles user authentication before calling the onboarding API
2. Mobile app captures and encodes identity document images in base64 format
3. Users accept terms and conditions before submitting applications
4. Each market has a single national ID format
5. Document images are under 10MB in size
6. Market configurations are managed through deployment pipelines

### Dependencies
1. SQL Server database for application persistence
2. Seq for log aggregation and monitoring
3. IDNow API for identity verification (mocked in v1)
4. World-Check API for sanctions screening (mocked in v1)
5. Core banking system API for account creation (seam in v1)
6. Card issuer API for card ordering (seam in v1)
7. HTTPS infrastructure for secure communication

---

## Glossary of Terms

- **EARS:** Easy Approach to Requirements Syntax - a structured format for writing requirements
- **PEP:** Politically Exposed Person - individual in prominent public position
- **Sanctions Screening:** Process of checking individuals against international sanctions lists
- **Face Match:** Biometric comparison between document photo and selfie
- **Idempotency:** Property ensuring repeated operations with same input produce same result
- **Correlation ID:** Unique identifier for tracing requests across system components
- **Base64:** Binary-to-text encoding for transmitting document images
- **HTTP 201:** Created status code indicating successful resource creation
- **HTTP 400:** Bad Request status code indicating validation error
- **HTTP 404:** Not Found status code indicating resource does not exist
- **HTTP 500:** Internal Server Error status code indicating system failure
- **Transient Failure:** Temporary error that may succeed on retry
- **Provider:** External service provider for verification or screening
