namespace Domain.Dto.Authentication
{
    public class MakeCredentialOptionsRequestDto
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string AttestationType { get; set; } // possible values: none, direct, indirect
        public string AuthenticatorAttachment { get; set; } // possible values: <empty>, platform, cross-platform
        public bool RequireResidentKey { get; set; } // possible values: true , false
        public string UserVerification { get; set; } // possible values: preferred, required, discouraged
    }
}