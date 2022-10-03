namespace Authentication.ViewModel
{
    public class AuthResult
    {
        public string Token {get;set;}

        public DateTime ExpiresAt {get;set;}

        public string RefreshToken { get; set; }
    }
}