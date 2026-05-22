namespace aoe.DTOs.User
{
    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; } = default!;

        public string NewPassword { get; set; } = default!;
    }
}
