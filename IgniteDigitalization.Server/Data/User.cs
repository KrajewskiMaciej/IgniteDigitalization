using System.ComponentModel.DataAnnotations;

namespace backend.Data
{
    public class User
    {
        public int Users_Id { get; set; }
        [MaxLength(50)]
        public string Names { get; set; } = string.Empty;
        [MaxLength(50)]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Email_Confirmed { get; set; } = false;
        [MaxLength(50)]
        public string? Link_Token { get; set; }
        public DateTime? Token_Expire_Date { get; set; }
        public int Licenses_Owned { get; set; }
        public int Licenses_Used { get; set; }
        public int Games_In_Progress { get; set; }
        public int Role { get; set; } = 1;   // 1 = ograniczony, 9 = pełny dostęp

    }
}
