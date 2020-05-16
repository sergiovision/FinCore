using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class UserToken
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string userName { get; set; }
    }

    public class LoginInfo
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
