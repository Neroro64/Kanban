using System.Collections;

namespace Kanban.Abstract;
[Serializable]
public abstract class KanbanContainer<T> : Undoable, ISerializable<KanbanContainer<T>.MetaData>, IEnumerable<T> where T : IIdentifiable
{
    public MetaData FileMeta { get; init; } = default;
    protected Dictionary<Guid, T> m_items = new();
    public virtual void Add(T item)
    {
        m_items.TryAdd(item.Guid, item);
    }

    public virtual IEnumerable<T>? Find(string query)
    {
        return m_items.Values.Where(item => item.Name.Contains(query));
    }

    public virtual bool TryGet(Guid itemGuid, ref T? itemRef)
    {
        return m_items.TryGetValue(itemGuid, out itemRef);
    }

    public virtual void Remove(Guid itemGuid)
    {
        if (m_items.ContainsKey(itemGuid))
            m_items.Remove(itemGuid);
    }

    public Task<bool> LoadAsync()
    {
        throw new NotImplementedException();
    }

    public Task<MetaData> SaveAsync()
    {
        return SaveAsync(new JsonSerializerSettings());
    }
    public Task<MetaData> SaveAsync(JsonSerializerSettings settings)
    {
        return Task.Run((Func<MetaData>)(() =>
        {
            try
            {
                string serializedData = JsonConvert.SerializeObject(this, settings);
                return this.FileMeta with { Data = serializedData };
            }
            catch (JsonSerializationException) { throw; }
        }));
    }

    public IEnumerator<T> GetEnumerator()
    {
        return m_items.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    [Serializable]
    public record struct MetaData
    {
        public string FilePath { get; init; }
        public string? Data { get; init; }
        public MetaData(string filePath, string? data)
        {
            this.FilePath = filePath;
            this.Data = data;
        }
    }
}
