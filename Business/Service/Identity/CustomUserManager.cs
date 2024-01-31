using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entity.Concrete;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Business.Service.Identity
{
    public class CustomUserManager<TUser> : UserManager<TUser> where TUser : AppUser
    {
        public CustomUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators, IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public Task<TUser> FindByIdAsync(int userId)
        {
            return base.FindByIdAsync(userId.ToString());
        }
    }
}