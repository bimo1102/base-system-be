using BaseDomains;
using Enums;

namespace AccountDomains;

public class UserInformation: BaseDomain
{
    #region Properties
    public string? FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public GenderEnum? Gender { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? CompanyName { get; set; }
    #endregion
}