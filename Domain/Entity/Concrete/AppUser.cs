using Domain.Entity.Abstract;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entity.Concrete
{
    public class AppUser : IdentityUser<int>, IEntity
    {
        
    }
}