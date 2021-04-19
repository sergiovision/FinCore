using System.Collections.Generic;

namespace BusinessLogic.Repo
{
    public interface IRepository<TEntity>
    {
        List<TEntity> GetAll();

        TEntity GetById(int id);

        TEntity Insert(TEntity entity);

        void Update(TEntity entity);

        void Delete(int id);
    }
}