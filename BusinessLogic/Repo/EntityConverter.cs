namespace BusinessLogic.Repo;

/// <summary>
///     Loads an entity by its id. Used by <see cref="BusinessLogic.EntityMapper" /> to map an int id
///     onto an entity reference (formerly AutoMapper's ConvertUsing&lt;EntityConverter&lt;T&gt;&gt;).
/// </summary>
public class EntityConverter<T>
{
    private readonly IRepository<T> _repository;

    public EntityConverter()
    {
        _repository = new BaseRepository<T>();
    }

    public T Convert(int source)
    {
        T val = _repository.GetById(source);
        return val;
    }
}
