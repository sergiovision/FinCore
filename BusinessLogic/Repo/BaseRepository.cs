using Autofac;
using BusinessLogic.BusinessObjects;
using BusinessObjects;
using NHibernate;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLogic.Repo
{
    public class BaseRepository<T> : IRepository<T> 
    {
        public BaseRepository()
        {
        }

        public List<T> GetAll()
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                return Session.Query<T>().ToList();
            }
        }

        public T GetById(int id)
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                return Session.Get<T>(id);
            }
        }

        public T Insert(T entity)
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                using (ITransaction Transaction = Session.BeginTransaction())
                {
                    Session.Save(entity);
                    Transaction.Commit();
                }
            }

            return entity;
        }

        public void Update(T entity)
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                using (ITransaction Transaction = Session.BeginTransaction())
                {
                    Session.Update(entity);
                    Transaction.Commit();
                }
            }
        }

        public void Delete(int id)
        {
            using (ISession Session = ConnectionHelper.CreateNewSession())
            {
                using (ITransaction Transaction = Session.BeginTransaction())
                {
                    Session.Delete(Session.Load<T>(id));
                    Transaction.Commit();
                }
            }
        }
    }
}