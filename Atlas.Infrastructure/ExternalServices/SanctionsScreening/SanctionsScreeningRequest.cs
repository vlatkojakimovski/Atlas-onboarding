namespace Atlas.Infrastructure.ExternalServices.SanctionsScreening;

/// <summary>
/// Request model for sanctions screening service containing applicant details.
/// </summary>
/// <param name="FirstName">Applicant's first name.</param>
/// <param name="LastName">Applicant's last name.</param>
/// <param name="DateOfBirth">Applicant's date of birth.</param>
/// <param name="Nationality">Applicant's nationality (country code).</param>
public record SanctionsScreeningRequest(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string Nationality);
