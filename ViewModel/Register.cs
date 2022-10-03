using System.ComponentModel.DataAnnotations;

namespace Authentication.ViewModel
{
    public class Register
    {
        public Register(string firstName, string lastName, string emailAddress, string userName, string password)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
            this.EmailAddress = emailAddress;
            this.UserName = userName;
            this.Password = password;

        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        public string Role {get;set;}
    }
}