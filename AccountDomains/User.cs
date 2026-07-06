using BaseDomains;
using Enums;

namespace AccountDomains;

public class User : BaseDomain
{
    #region Properties

    public new string Id { get; private set; }
    public string UserName { get; set; }
    public string? EmailAddress { get; private set; }
    public string? Password { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? 
        PasswordSalt { get; private set; }
    public AccountStatusEnum Status { get; private set; }
    #endregion
}