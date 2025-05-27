namespace GeeYeangSore.DTO.User;

public class UpdateProfileRequest
{
    public string Name { get; set; }
    public DateTime? Birthday { get; set; }
    public string Gender { get; set; } // "male" or "female"
    public string City { get; set; }
    public string Address { get; set; }
    public string Phone { get; set; }
    public string Avatar { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
}
