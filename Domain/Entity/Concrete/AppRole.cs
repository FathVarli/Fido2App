using Domain.Entity.Abstract;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entity.Concrete
{
    public class AppRole : IdentityRole<int>, IEntity
    {
        
    }
}