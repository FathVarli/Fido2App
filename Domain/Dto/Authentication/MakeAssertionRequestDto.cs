using Fido2NetLib;

namespace Domain.Dto.Authentication
{
    public class MakeAssertionRequestDto
    {
        public AuthenticatorAssertionRawResponse AuthenticatorAssertionRawResponse { get; set; } //doğrulama cihazının kendisi tarafından üretilen ve doğrulama işleminin başarıyla tamamlandığını doğrulayan bir imza içerir. CBOR (Concise Binary Object Representation) formatında
        public AssertionOptions AssertionOption { get; set; }
    }
}