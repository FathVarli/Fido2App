using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Helpers.Fido.Mapper;
using Domain.Entity.Concrete;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Infrastructure.Context;
using Infrastructure.Repository.Abstract;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.Concrete
{
    public class StoredCredentialDetailRepository : Base.Repository<StoredCredentialDetail,int>, IStoredCredentialDetailRepository
    {
        private readonly AppDbContext _context;
        public StoredCredentialDetailRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<StoredCredential>> ListCredentialsByUser(string username)
        {
            
            var credentials = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.Username == username.ToLower().Trim()).ToListAsync();
            return credentials.Select(StoredCredentialMapper.ToDomain);
        }

        public async Task<IEnumerable<PublicKeyCredentialDescriptor>> ListPublicKeysByUser(string username)
        {
            var pks = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.Username == username.ToLower().Trim()).ToListAsync();
            return pks.Select(PublicKeyCredentialDescriptorMapper.ToDomain);
        }

        public async Task<IEnumerable<StoredCredential>> ListCredentialsByPublicKeyIdAsync(byte[] credentialId)
        {
            var users = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.PublicKeyId == credentialId).ToListAsync();
            return users.Select(StoredCredentialMapper.ToDomain);
        }

        public async Task<StoredCredential> GetCredentialByPublicKeyIdAsync(byte[] credentialId)
        {
            var users = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().FirstOrDefaultAsync(w => w.PublicKeyId == credentialId);
            return users == null ? null : StoredCredentialMapper.ToDomain(users);
        }

        public async Task Store(string securityKeyAlias, Fido2User user, StoredCredential storedCredential) // Kaydetme islemi
        {
            var model = StoredCredentialMapper.ToModel(storedCredential).UpdateUserDetails(user).SetSecurityKeyName(securityKeyAlias);
            await AddAsync(model);
        }

        public async Task<IEnumerable<StoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
        {
            var users = await _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.UserHandle == userHandle).ToListAsync();
            return users.Select(StoredCredentialMapper.ToDomain);
        }

        public async Task UpdateCounter(byte[] credentialId, uint counter)
        {
            var cred = await _context.Fido2StoredCredential.FirstOrDefaultAsync(f => f.PublicKeyId == credentialId);
            if (cred != null)
            {
                cred.SignatureCounter = counter;
            }
            Update(cred);
        }

        public Task<string> GetUsernameByIdAsync(byte[] userId)
        {
            return _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.UserId == userId).Select(s => s.Username).FirstOrDefaultAsync();
        }

        public Task<List<StoredCredentialDetail>> ListCredentialDetailsByUser(byte[] userId)
        {
            return _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.UserId == userId).ToListAsync();
        }

        public Task<List<StoredCredentialDetail>> ListCredentialDetailsByUser(string username)
        {
            return _context.Fido2StoredCredential.AsNoTrackingWithIdentityResolution().Where(w => w.Username == username.ToLower().Trim()).ToListAsync();
        }

        public async Task RemoveByPublicKeyId(byte[] publicKeyId)
        {
            var key = await _context.Fido2StoredCredential.FirstOrDefaultAsync(f => f.PublicKeyId == publicKeyId);
            if (key is not null)
            {
                await DeleteAsync(key.Id);
            }
        }

        public async Task RemoveBySecretName(string name)
        {
            var key = await _context.Fido2StoredCredential.FirstOrDefaultAsync(f => f.SecurityKeyName == name);
            if (key is not null)
            {
                await DeleteAsync(key.Id);
            }
        }

        public async Task<bool> HasSecurityKey(string name)
        {
            return await _context.Fido2StoredCredential.AnyAsync(f => f.SecurityKeyName == name);
        }
    }
}