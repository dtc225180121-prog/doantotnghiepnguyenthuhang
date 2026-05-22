using System.Text.RegularExpressions;

namespace aoe.Helpers
{
    public static class ValidationHelper
    {
        public static bool ValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$"
            );
        }

        public static bool ValidPhone(string phone)
        {
            return Regex.IsMatch(phone, @"^[0-9]{10,11}$");
        }

        public static bool ValidName(string name)
        {
            return name.Length >= 2 && name.Length <= 50;
        }

        public static bool ValidPassword(string password)
        {
            return password.Length >= 6 && password.Length <= 50;
        }

        public static bool ValidRole(string role)
        {
            return role == "teacher" || role == "student";
        }
    }
}
