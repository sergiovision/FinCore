using AutoMapper;

namespace BusinessLogic.Repo;

public class EntityConverter<T> : ITypeConverter<int, T> 
{
    private readonly IRepository<T> _repository;
    public EntityConverter()
    {
        _repository = new BaseRepository<T>();
    }

    public T Convert(int source, T destination, ResolutionContext context)
    {
        T val = _repository.GetById(source); 
        return val;
    }
}