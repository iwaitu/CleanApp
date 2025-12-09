using CleanApp.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace CleanApp.Infrastructure
{
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
    {
        private readonly TContext _context;
        private bool _disposed;
        private IDbContextTransaction _transaction;

        public UnitOfWork(TContext context)
        {
            _context = context;
        }

        public void BeginTransaction()
        {
            if (_transaction == null)
                _transaction = _context.Database.BeginTransaction();
        }

        public IQueryable<T> GetRepository<T>() where T : class
        {
            return _context.Set<T>().AsNoTracking();
        }

        public async Task<T> FindByIdAsync<T>(object id) where T : class
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public void AddEntity<T>(T entity) where T : class
        {
            _context.Set<T>().Add(entity);
        }

        public void UpdateEntity<T>(T entity) where T : class
        {
            _context.Set<T>().Update(entity);
        }

        void IUnitOfWork<TContext>.RemoveEntity<T>(T entity) where T : class
        {
            if (entity is BaseEntity baseEntity)
                RemoveEntity(baseEntity);
            else
                _context.Set<T>().Remove(entity);
        }

        private void RemoveEntity(BaseEntity entity)
        {
            if (entity != null)
            {
                entity.IsDeleted = true;
                _context.Update(entity);
            }
        }

        public async Task<bool> CommitAsync()
        {
            try
            {
                int result = await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
                return result > 0;
            }
            catch
            {
                Rollback();
                throw;
            }
        }

        public void Rollback()
        {
            if (_transaction != null)
            {
                _transaction.Rollback();
                _transaction.Dispose();
                _transaction = null;
            }
            _context.ChangeTracker.Entries()
                .ToList()
                .ForEach(entry => entry.State = EntityState.Unchanged);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _transaction?.Dispose();
                _context.Dispose();
                _disposed = true;
            }
        }
    }
}
