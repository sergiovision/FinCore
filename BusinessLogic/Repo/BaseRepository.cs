using System.Collections.Generic;
using System.Linq;

namespace BusinessLogic.Repo;

public class BaseRepository<T> : IRepository<T>
{
    public List<T> GetAll()
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            return Session.Query<T>().ToList();
        }
    }

    public T GetById(int id)
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            return Session.Get<T>(id);
        }
    }

    public T Insert(T entity)
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            using (var Transaction = Session.BeginTransaction())
            {
                Session.Save(entity);
                Transaction.Commit();
            }
        }

        return entity;
    }

    public void Update(T entity)
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            using (var Transaction = Session.BeginTransaction())
            {
                Session.Update(entity);
                Transaction.Commit();
            }
        }
    }

    public void Delete(int id)
    {
        using (var Session = ConnectionHelper.CreateNewSession())
        {
            using (var Transaction = Session.BeginTransaction())
            {
                Session.Delete(Session.Load<T>(id));
                Transaction.Commit();
            }
        }
    }
}
