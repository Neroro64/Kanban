using Kanban.Abstract;
namespace Kanban;
public sealed class LocalDatabase : Database<KanbanBoard>
{
    private static readonly Lazy<LocalDatabase> _database = new(() => new LocalDatabase());
    public static LocalDatabase s_Database
    {
        get  {return _database.Value;}
    }
    public Dictionary<ItemStatus, Dictionary<Guid, KanbanContainer.MetaData>> Index { get; set; } = new();

    private Dictionary<Guid, KanbanBoard> _loadedContainers = new();

    public LocalDatabase()
    {
        FileInfo log4netConfig = new("log4net.config");
        if (!log4netConfig.Exists)
            BasicConfigurator.Configure();
        else
            XmlConfigurator.Configure(configFile: log4netConfig);
        
        foreach (var status in Enum.GetValues(typeof(ItemStatus)).Cast<ItemStatus>())
            Index[status] = new();
    }
    ~LocalDatabase()
    {
        DeInit();
    }

    public override KanbanBoard? GetContainer(Guid guid)
    {
        KanbanBoard? board;
        if (_loadedContainers.TryGetValue(guid, out board))
            return board;
        foreach ((var _, var containerDict) in Index)
        {
            if (containerDict.TryGetValue(guid, out KanbanContainer.MetaData metaData))
            {
                board = loadContainer(guid, metaData);
                break;
            }
        }
        return board;
    }

    public override IEnumerable<KanbanBoard>? FindContainers(string query)
    {
        var foundContainers = GetLoadedContainers().Where(container => container.Name.Contains(query));
        if (foundContainers != null && foundContainers.Count() > 0)
            return foundContainers;

        foreach ((ItemStatus _, var containerDict) in Index)
        {
            var containers = containerDict.Where(kv => kv.Value.FilePath.Contains(query));
            if (containers != null)
            {
                return containers.Select(kv => loadContainer(kv.Key, kv.Value));
            }
        }
        return null;
    }

    public override KanbanBoard NewContainer(string name="NewContainer")
    {
        KanbanBoard board = new(name, new KanbanContainer.MetaData() { FilePath = GetRelativeFilePath(name + ".json") });
        return board;
    }
    public override void AddContainer(KanbanBoard newContainer)
    {
        _loadedContainers.TryAdd(newContainer.Guid, newContainer);
        if (!Index[newContainer.Status].TryAdd(newContainer.Guid, newContainer.Meta))
        {
            s_Logger.Warn($"""
            Failed to add new container with GUID: {newContainer.Guid},
            because it already exists: {GetContainer(newContainer.Guid)}
            """);
            return;
        }
    }

    public override void RemoveContainer(Guid guid, bool unload = true)
    {
        if (unload && _loadedContainers.ContainsKey(guid))
            _loadedContainers.Remove(guid);

        foreach (var kv in Index.Values)
        {
            if (kv.ContainsKey(guid))
            {
                kv.Remove(guid);
                break;
            }
        }
    }

    public override void MoveContainer(Guid guid, ItemStatus newStatus)
    {
        KanbanBoard? board = GetContainer(guid);
        if (board == null)
        {
            s_Logger.Error($"Container to be moved not found in database! Guid: {guid}");
            return;
        }
        board.Status = newStatus;
        RemoveContainer(guid, unload: false);
        Index[newStatus][guid] = board.Meta;
    }

    public override IEnumerable<KanbanBoard> GetLoadedContainers()
    {
        return _loadedContainers.Values;
    }

    protected override void Init()
    {
        (this as ISerializable<bool>).LoadAsync().Wait();
    }

    protected override void DeInit()
    {
        (this as ISerializable<bool>).SaveAsync().Wait();
    }

    protected override Task<bool> PreLoadContainers()
    {
        return Task.Run(() =>
        {
            Parallel.ForEach(Index[ItemStatus.ONGOING], kv =>
            {
                loadContainer(kv.Key, kv.Value);
            });
            return true;
        });
    }
    private KanbanBoard loadContainer(Guid guid, KanbanContainer.MetaData metaData)
    {
        KanbanBoard board = Load<KanbanBoard>(metaData.FilePath).Result;
        _loadedContainers.Add(guid, board);
        return board;
    }
}
