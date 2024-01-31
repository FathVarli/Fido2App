using Fido2NetLib;

namespace Domain.Dto.Authentication
{
    public class MakeCredentialRequestDto
    {
        public AuthenticatorAttestationRawResponse AuthenticatorAttestationRawResponse { get; set; } //doğrulama cihazının kendisi tarafından üretilen ve doğrulama işleminin başarıyla tamamlandığını doğrulayan bir imza içerir. CBOR (Concise Binary Object Representation) formatında
        public CredentialCreateOptions CredentialCreateOptions { get; set; }
    }
}