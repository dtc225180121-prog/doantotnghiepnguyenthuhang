namespace aoe.DTOs.Auth
{
    public class ChangePasswordDTO
    {
        public string OldPassword { get; set; } = default!;

        public string NewPassword { get; set; } = default!;
    }
}
