namespace Kanban.Abstract
{
    public interface ISerializable<ResultType>
    {
        public abstract Task<ResultType> SaveAsync();
        public abstract Task<ResultType> SaveAsync(JsonSerializerSettings _);
        public abstract Task<bool> LoadAsync();
    }
}
