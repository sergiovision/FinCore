using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace BusinessLogic.Repo
{
    public interface IRepository<TEntity> //where TEntity : class
    {
        List<TEntity> GetAll();

        TEntity GetById(int id);

        TEntity Insert(TEntity entity);

        void Update(TEntity entity);

        void Delete(int id);

        //IQueryable<T> SearchFor(Expression<Func<T, bool>> predicate);
    }
}