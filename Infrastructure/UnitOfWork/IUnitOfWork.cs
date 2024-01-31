using System;
using System.Threading.Tasks;
using Domain.Entity.Abstract;
using Infrastructure.Repository.Base;

namespace Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<TEntity, TPrimaryKey> GetRepository<TEntity, TPrimaryKey>()
            where TEntity : class, IEntity, new() where TPrimaryKey : IEquatable<TPrimaryKey>;

        int Complete();

        Task<int> CompleteAsync();

        void RollBack();

        Task RollBackAsync();
    }
}