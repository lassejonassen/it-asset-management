namespace UserService.Dtos;

public record UserPasswordChangeDto
{
    public Guid UserId { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}