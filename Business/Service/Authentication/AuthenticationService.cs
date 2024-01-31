using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Service.Identity;
using Core.Results.Base;
using Core.Results.DataResults;
using Domain.Dto.Authentication;
using Domain.Entity.Concrete;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Infrastructure.Repository.Abstract;
using Infrastructure.UnitOfWork;

namespace Business.Service.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly CustomUserManager<AppUser> _userManager;
        private readonly IStoredCredentialDetailRepository _storedCredentialDetailRepository;
        private readonly IFido2 _fido2;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticationService(
            CustomUserManager<AppUser> userManager, 
            IStoredCredentialDetailRepository storedCredentialDetailRepository, 
            IFido2 fido2, 
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _storedCredentialDetailRepository = storedCredentialDetailRepository;
            _fido2 = fido2;
            _unitOfWork = unitOfWork;
        }
        
        
        #region public async Task<IDataResult<CredentialCreateOptions>> MakeCredentialOptions(MakeCredentialOptionsRequestDto makeCredentialOptionsRequestDto)

        public async Task<IDataResult<CredentialCreateOptions>> MakeCredentialOptions(MakeCredentialOptionsRequestDto makeCredentialOptionsRequestDto)
        {
            if (string.IsNullOrEmpty(makeCredentialOptionsRequestDto.Username))
            {
                return new ErrorDataResult<CredentialCreateOptions>("");
            }

            var isUsernameExist = await _userManager.FindByEmailAsync(makeCredentialOptionsRequestDto.Username);
            if (isUsernameExist is not null)
            {
                return new ErrorDataResult<CredentialCreateOptions>("");
            }
            
            var fidoUser = new Fido2User
            {
                DisplayName = makeCredentialOptionsRequestDto.DisplayName,
                Name = makeCredentialOptionsRequestDto.Username,
                Id = Encoding.UTF8.GetBytes(makeCredentialOptionsRequestDto.Username)
            };
            
            // 1. Get user existing keys by username
            var existingKeys = await _storedCredentialDetailRepository.ListPublicKeysByUser(fidoUser.Name);
            
            // 2. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = makeCredentialOptionsRequestDto.RequireResidentKey, //bir kullanıcının doğrulama cihazında (authenticator) kalıcı bir anahtar (resident key) oluşturup oluşturamayacağını belirtir.
                UserVerification = makeCredentialOptionsRequestDto.UserVerification.ToEnum<UserVerificationRequirement>(), // dogrulama asamasinda biyometrik veya pin dogrulamasini ayarlar
                AuthenticatorAttachment = makeCredentialOptionsRequestDto.AuthenticatorAttachment.ToEnum<AuthenticatorAttachment>(), //kullanıcının cihazında (authenticator) bulunan doğrulama anahtarlarının (credentials) nasıl bağlı olduğunu belirler.
            };
            
            var authenticationExtensionsClientInputs = new AuthenticationExtensionsClientInputs() //kullanıcı doğrulama isteğinin bir parçası olarak gönderilebilecek ekstra verilerini içerir.
            {
                Extensions = true, 
                UserVerificationIndex = true, 
                Location = true, 
                UserVerificationMethod = true, 
                BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds 
                { 
                    FAR = float.MaxValue, 
                    FRR = float.MaxValue 
                } 
            };

            var options = _fido2.RequestNewCredential(fidoUser, existingKeys.ToList(), authenticatorSelection,
                makeCredentialOptionsRequestDto.AttestationType.ToEnum<AttestationConveyancePreference>(), authenticationExtensionsClientInputs);

            return new SuccessDataResult<CredentialCreateOptions>(options);

        }

        #endregion

        #region public async Task<IDataResult<Fido2.CredentialMakeResult>> MakeCredential(MakeCredentialRequestDto makeCredentialRequestDto)

        public async Task<IDataResult<Fido2.CredentialMakeResult>> MakeCredential(MakeCredentialRequestDto makeCredentialRequestDto)
        {
            // 2. Kullanıcının kimlik doğrulama anahtarının (credential) benzersiz olup olmadığını kontrol etmektir.
            IsCredentialIdUniqueToUserAsyncDelegate callback = async args =>
            {
                var users = await _storedCredentialDetailRepository.ListCredentialsByPublicKeyIdAsync(args.CredentialId);
                return !users.Any();
            };
            
            // 2. Verify and make the credentials
            var success = await _fido2.MakeNewCredentialAsync(makeCredentialRequestDto.AuthenticatorAttestationRawResponse, makeCredentialRequestDto.CredentialCreateOptions, callback);
            
            // 3. Store the credentials in db
            var user = new AppUser
            {
                
                UserName = makeCredentialRequestDto.CredentialCreateOptions.User.DisplayName,
                Email = makeCredentialRequestDto.CredentialCreateOptions.User.Name
            };
            
            // 4. Create user at ASP.NET Identity
            var result = await _userManager.CreateAsync(user);
            var createdUser = await _userManager.FindByEmailAsync(user.Email);
            
            var fidoUser = new Fido2User
            {
                DisplayName = createdUser.UserName,
                Name = createdUser.Email,
                Id = Encoding.UTF8.GetBytes(createdUser.Id.ToString()) // byte representation of userID is required
            };

            await _storedCredentialDetailRepository.Store("some-random-alias", fidoUser, new StoredCredential
            {
                Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
                PublicKey = success.Result.PublicKey,
                UserHandle = success.Result.User.Id,
                SignatureCounter = success.Result.Counter,
                CredType = success.Result.CredType,
                RegDate = DateTime.UtcNow,
                AaGuid = success.Result.Aaguid
            });

            await _unitOfWork.CompleteAsync();

            return new SuccessDataResult<Fido2.CredentialMakeResult>(success);
        }

        #endregion

        #region public async Task<IDataResult<AssertionOptions>> AssertionOptions(AssertionOptionsRequestDto assertionOptionsRequestDto)

        public async Task<IDataResult<AssertionOptions>> AssertionOptions(AssertionOptionsRequestDto assertionOptionsRequestDto)
        {
            var isUserExist = await _userManager.FindByEmailAsync(assertionOptionsRequestDto.Username);
            if (isUserExist is null)
            {
                return new NotFoundDataResult<AssertionOptions>("User not found");
            }

            var fidoUsername = await _storedCredentialDetailRepository.GetUsernameByIdAsync(Encoding.UTF8.GetBytes(isUserExist.Id.ToString()));

            if (!fidoUsername.Equals(assertionOptionsRequestDto.Username))
            {
                return new NotFoundDataResult<AssertionOptions>("User not found");
            }
            
            
            var userCredential = (await _storedCredentialDetailRepository.ListPublicKeysByUser(assertionOptionsRequestDto.Username)).ToList();
            if (!userCredential.Any())
            {
                return new ErrorDataResult<AssertionOptions>("User not registered");
            }
            
            var authenticationExtensionsClientInputs = new AuthenticationExtensionsClientInputs() //kullanıcı doğrulama isteğinin bir parçası olarak gönderilebilecek ekstra verilerini içerir.
            {
                Extensions = true, 
                UserVerificationIndex = true, 
                Location = true, 
                UserVerificationMethod = true, 
                BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds 
                { 
                    FAR = float.MaxValue, 
                    FRR = float.MaxValue 
                } 
            };
            
            // 3. Create options
            var userVerificationRequirement = string.IsNullOrEmpty(assertionOptionsRequestDto.UserVerification) 
                ? UserVerificationRequirement.Discouraged 
                : assertionOptionsRequestDto.UserVerification.ToEnum<UserVerificationRequirement>();
            
            var options = _fido2.GetAssertionOptions(
                userCredential,
                userVerificationRequirement,
                authenticationExtensionsClientInputs
            );

            return new SuccessDataResult<AssertionOptions>(options);
        }

        #endregion

        #region public async Task<IDataResult<AssertionVerificationResult>> MakeAssertion(MakeAssertionRequestDto makeAssertionRequest)

        public async Task<IDataResult<AssertionVerificationResult>> MakeAssertion(MakeAssertionRequestDto makeAssertionRequest)
        {
            var storedCredential = await _storedCredentialDetailRepository
                .GetCredentialByPublicKeyIdAsync(makeAssertionRequest.AuthenticatorAssertionRawResponse.Id);

            if (storedCredential is null)
            {
                return new UnauthorizedDataResult<AssertionVerificationResult>("Unknown Credentials");
            }

            var storedCounter = storedCredential.SignatureCounter;
            
            // Create callback to check if userhandle owns the credentialId
            IsUserHandleOwnerOfCredentialIdAsync callback = async (args) =>
            {
                var storedCredentials = await _storedCredentialDetailRepository.GetCredentialsByUserHandleAsync(args.UserHandle);
                return storedCredentials.ToList().Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
            };
            
            // Make the assertion
            var res = await _fido2.MakeAssertionAsync(makeAssertionRequest.AuthenticatorAssertionRawResponse,
                makeAssertionRequest.AssertionOption, 
                storedCredential.PublicKey,
                storedCounter, 
                callback);
            
            // 6. Store the updated counter
            await _storedCredentialDetailRepository.UpdateCounter(res.CredentialId, res.Counter);
            await _unitOfWork.CompleteAsync();
            
            // Login at ASP.NET Identity

            return new SuccessDataResult<AssertionVerificationResult>(res);
        }

        #endregion
        
    }
}