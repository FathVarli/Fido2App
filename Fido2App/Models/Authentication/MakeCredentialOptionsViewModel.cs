namespace Fido2App.Models.Authentication
{
    public class MakeCredentialOptionsViewModel
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AttestationType { get; set; }
        public string AuthenticatorAttachment { get; set; }
        public bool RequireResidentKey { get; set; } 
        public string UserVerification { get; set; }
    }
}