using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entity.Concrete;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Infrastructure.Repository.Base;

namespace Infrastructure.Repository.Abstract
{
    public interface IStoredCredentialDetailRepository : IRepository<StoredCredentialDetail, int>
    {
        Task<IEnumerable<StoredCredential>> ListCredentialsByUser(string username);
        Task<IEnumerable<PublicKeyCredentialDescriptor>> ListPublicKeysByUser(string username);
        Task<IEnumerable<StoredCredential>> ListCredentialsByPublicKeyIdAsync(byte[] credentialId);
        Task<StoredCredential> GetCredentialByPublicKeyIdAsync(byte[] credentialId);
        Task Store(string securityKeyAlias, Fido2User user, StoredCredential storedCredential);
        Task<IEnumerable<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle);
        Task UpdateCounter(byte[] credentialId, uint counter);
        Task<string> GetUsernameByIdAsync(byte[] userId);
        Task<List<StoredCredentialDetail>> ListCredentialDetailsByUser(byte[] userId);
        Task<List<StoredCredentialDetail>> ListCredentialDetailsByUser(string username);
        Task RemoveByPublicKeyId(byte[] publicKeyId);
        Task RemoveBySecretName(string name);
        Task<bool> HasSecurityKey(string name);
    }
}