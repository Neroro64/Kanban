namespace Kanban.Interfaces
{
    public interface ISerializable<ResultType>
    {
        public abstract Task<ResultType> SaveAsync();
        public abstract Task<ResultType> SaveAsync(JsonSerializer _);
        public abstract Task<bool> LoadAsync();
    }
}
