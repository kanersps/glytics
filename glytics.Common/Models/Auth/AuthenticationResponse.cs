namespace glytics.Common.Models.Auth
{
    public class AuthenticationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }
}