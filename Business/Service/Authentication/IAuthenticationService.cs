using System.Threading.Tasks;
using Core.Results.Base;
using Domain.Dto.Authentication;
using Fido2NetLib;
using Fido2NetLib.Objects;

namespace Business.Service.Authentication
{
    public interface IAuthenticationService
    {
        Task<IDataResult<CredentialCreateOptions>> MakeCredentialOptions(MakeCredentialOptionsRequestDto makeCredentialOptionsRequestDto);
        Task<IDataResult<Fido2.CredentialMakeResult>> MakeCredential(MakeCredentialRequestDto makeCredentialRequestDto);
        Task<IDataResult<AssertionOptions>> AssertionOptions(AssertionOptionsRequestDto assertionOptionsRequestDto);
        Task<IDataResult<AssertionVerificationResult>> MakeAssertion(MakeAssertionRequestDto makeAssertionRequest);
    }
}