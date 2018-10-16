using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnitOfWork.Repository;

namespace NTPAC.Persistence
{
    public abstract class UserRepositoryBase<TEntity> : IRepository<TEntity> where TEntity : class, new()
    {
        private readonly IRepository<TEntity> _repositoryImplementation;

        protected UserRepositoryBase(IRepository<TEntity> repositoryImplementation) { this._repositoryImplementation = repositoryImplementation; }

        
        public TEntity Get(Int32 id) => this._repositoryImplementation.Get(id);

        public IEnumerable<TEntity> GetAll() => this._repositoryImplementation.GetAll();

        public IEnumerable<TEntity> Find(Expression<Func<TEntity, Boolean>> predicate) => this._repositoryImplementation.Find(predicate);

        public TEntity SingleOrDefault(Expression<Func<TEntity, Boolean>> predicate) => this._repositoryImplementation.SingleOrDefault(predicate);

        public void Add(TEntity entity) { this._repositoryImplementation.Add(entity); }

        public void AddRange(IEnumerable<TEntity> entities) { this._repositoryImplementation.AddRange(entities); }

        public void Remove(TEntity entity) { this._repositoryImplementation.Remove(entity); }

        public void RemoveRange(IEnumerable<TEntity> entities) { this._repositoryImplementation.RemoveRange(entities); }
    }
}