using Kanban.Interfaces;
namespace Kanban;
using IndexType = Dictionary<ItemStatus, Dictionary<Guid, ContainerMetaData>>;
public sealed class LocalDatabase : Undoable, IDatabase<KanbanBoard>
{
    public static Lazy<LocalDatabase> Database {
        get => Database ?? new(() => new LocalDatabase());
    }
    private IndexType _index = new();
    private Dictionary<Guid, KanbanBoard> _loadedContainers = new();

    public LocalDatabase()
    {
        FileInfo log4netConfig = new("log4net.config");
        if (!log4netConfig.Exists)
            BasicConfigurator.Configure();
        else
            XmlConfigurator.Configure(configFile:log4netConfig);
    }
    ~LocalDatabase()
    {
    }

    public KanbanBoard? GetContainer(Guid guid)
    {
        KanbanBoard? board;
        if (_loadedContainers.TryGetValue(guid, out board))
            return board;
        foreach ((var _, var containerDict) in _index) 
        {
            if (containerDict.TryGetValue(guid, out ContainerMetaData metaData))
            {
                board = loadContainer(guid, metaData);
                break;
            }
        }
        return board;
    }

    public KanbanBoard? FindContainer(string query)
    {
        KanbanBoard? board = _loadedContainers.Where(kv => kv.Value.Name.Contains(query))?.First().Value;
        if (board == null)
        {
            foreach ((var _, var containerDict) in _index) 
            {
                KeyValuePair<Guid,ContainerMetaData>? container = containerDict.Where(kv => kv.Value.Name.Contains(query))?.First();
                if (container != null)
                {
                    board = loadContainer(container.Value.Key, container.Value.Value);
                    return board;
                }
            }               
        }
        return board;
    }

    public void AddContainer(KanbanBoard newContainer, ItemStatus status = ItemStatus.PENDING)
    {
        throw new NotImplementedException();
    }

    public void RemoveContainer(Guid guid)
    {
        throw new NotImplementedException();
    }

    public void MoveContainer(Guid guid, ItemStatus newStatus)
    {
        throw new NotImplementedException();
    }
    private KanbanBoard loadContainer(Guid guid, ContainerMetaData metaData)
    {
        KanbanBoard board = new(metaData);
        _loadedContainers.Add(guid, board);
        return board;
    }
 
    public IEnumerable<KanbanBoard> GetLoadedContainers()
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveAsync(JsonSerializer _)
    {
        throw new NotImplementedException();
    }

    void IDatabase<KanbanBoard>.Init()
    {
        throw new NotImplementedException();
    }

    void IDatabase<KanbanBoard>.DeInit()
    {
        throw new NotImplementedException();
    }

    Task<bool> IDatabase<KanbanBoard>.PreLoadContainers()
    {
        throw new NotImplementedException();
    }

    IndexType IDatabase<KanbanBoard>.GetIndex()
    {
        throw new NotImplementedException();
    }

    void IDatabase<KanbanBoard>.SetIndex()
    {
        throw new NotImplementedException();
    }
}
