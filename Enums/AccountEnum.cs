using System.ComponentModel.DataAnnotations;

namespace Enums;

public enum AccountStatusEnum
{
    Active = 1,
    InActive = 2,
    Deleted = 3
}
public enum GenderEnum
{
    [Display(Name = "Nam")] Male = 1,
    [Display(Name = "Nữ")] Female = 2,
    [Display(Name = "Khác")] Other = 3
}