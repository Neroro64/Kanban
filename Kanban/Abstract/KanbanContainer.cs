namespace Kanban.Abstract;
[Serializable]
public abstract class KanbanContainer : Undoable, ISerializable<KanbanContainer.MetaData>
{
    public MetaData Meta { get; init; } = default;
    protected Dictionary<Guid, IKanbanItem> m_items = new();
    public virtual void AddKanbanItem(IKanbanItem item)
    {
        m_items.TryAdd(item.Guid, item);
    }

    public virtual IEnumerable<IKanbanItem>? FindKanbanItems(string query)
    {
        return m_items.Values.Where(item => item.Name.Contains(query));
    }

    public virtual IKanbanItem? GetKanbanItem(Guid itemGuid)
    {
        if (m_items.TryGetValue(itemGuid, out var item))
            return item;
        return null;
    }

    public virtual void RemoveKanbanItem(Guid itemGuid)
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
                return this.Meta with { Data = serializedData };
            }
            catch (JsonSerializationException) { throw; }
        }));
    }
    [Serializable]
    public record struct MetaData 
    { 
        public string FilePath {get; init;}
        public string? Data {get; init;}
        public MetaData(string filePath, string? data){
            this.FilePath = filePath;
            this.Data = data;
        }
    }
}
