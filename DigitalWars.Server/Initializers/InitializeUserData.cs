using backend.Data;

namespace backend.Initializers
{
    public static class InitialUserData
    {
        public static User GetInitialUser()
        {
            return new User
            {
                Names = "Admin",
                Email = "itmserwis@itm.com.pl",
                Password = BCrypt.Net.BCrypt.HashPassword("itmserwis"),
                Email_Confirmed = true,
                Licenses_Owned = 999,
                Licenses_Used = 0,
                Games_In_Progress = 0
            };
        }
    }
}