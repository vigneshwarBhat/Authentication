using System.ComponentModel.DataAnnotations;
namespace Authentication.ViewModel
{
    public class SignIn
    {  
        public SignIn(string emailAddress, string password)
        {
            this.EmailAddress = emailAddress;
            this.Password = password;

        }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }

    }
}