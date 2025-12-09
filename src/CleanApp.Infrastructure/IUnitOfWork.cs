using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanApp.Infrastructure
{
    public interface IUnitOfWork<T> : IDisposable where T : DbContext
    {
        void BeginTransaction();
        void AddEntity<T>(T entity) where T : class;
        Task<bool> CommitAsync();
        void Dispose();
        Task<T> FindByIdAsync<T>(object id) where T : class;
        IQueryable<T> GetRepository<T>() where T : class;
        void RemoveEntity<T>(T entity) where T : class;
        void Rollback();
        void UpdateEntity<T>(T entity) where T : class;
    }
}
