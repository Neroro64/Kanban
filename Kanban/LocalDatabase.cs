using Kanban.Abstract;
namespace Kanban;
public sealed class LocalDatabase : Undoable, IDatabase<KanbanBoard>
{
    protected JsonSerializerSettings SerializerSettings
    {
        get => new() { Formatting = Formatting.Indented };
    }

    private static readonly Lazy<LocalDatabase> _database = new(() => new LocalDatabase());
    public static LocalDatabase s_Database
    {
        get { return _database.Value; }
    }

    public Dictionary<ItemStatus, Dictionary<Guid, KanbanBoard.MetaData>> Index { get; set; } = new();
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


    #region Container Methods
    public KanbanBoard? GetContainer(Guid guid)
    {
        KanbanBoard? board;
        if (_loadedContainers.TryGetValue(guid, out board))
            return board;
        foreach ((var _, var containerDict) in Index)
        {
            if (containerDict.TryGetValue(guid, out KanbanBoard.MetaData metaData))
            {
                board = loadContainer(guid, metaData);
                break;
            }
        }
        return board;
    }

    public IKanbanItem? GetItem(Guid guid)
    {
        // TODO: Need a way to search in all items ever created.
        foreach ((var _, var container) in _loadedContainers)
        {
            KanbanList? itemRef = default;
            if (container.TryGet(guid, ref itemRef))
                return itemRef as IKanbanItem;
        }
        return null;
    }

    public IEnumerable<KanbanBoard>? FindContainers(string query)
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

    public KanbanBoard NewContainer(string name = "NewContainer")
    {
        KanbanBoard board = new(name, new KanbanBoard.MetaData() { FilePath = GetRelativeFilePath(name + ".json") });
        return board;
    }

    public void AddContainer(KanbanBoard newContainer)
    {
        _loadedContainers.TryAdd(newContainer.Guid, newContainer);
        if (!Index[newContainer.Status].TryAdd(newContainer.Guid, newContainer.Meta))
        {
            IDatabase<KanbanBoard>.s_Logger.Warn($"""
            Failed to add new container with GUID: {newContainer.Guid},
            because it already exists: {GetContainer(newContainer.Guid)}
            """);
            return;
        }
    }

    public void RemoveContainer(Guid guid, bool unload = true)
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

    public void MoveContainer(Guid guid, ItemStatus newStatus)
    {
        KanbanBoard? board = GetContainer(guid);
        if (board == null)
        {
            IDatabase<KanbanBoard>.s_Logger.Error($"Container to be moved not found in database! Guid: {guid}");
            return;
        }
        board.Status = newStatus;
        RemoveContainer(guid, unload: false);
        Index[newStatus][guid] = board.Meta;
    }

    public IEnumerable<KanbanBoard> GetLoadedContainers()
    {
        return _loadedContainers.Values;
    }
    #endregion

    #region Initialization
    public void Init() => (this as ISerializable<bool>).LoadAsync().Wait();

    public void DeInit() => (this as ISerializable<bool>).SaveAsync().Wait();

    public Task<bool> PreLoadContainers()
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

    private KanbanBoard loadContainer(Guid guid, KanbanBoard.MetaData metaData)
    {
        KanbanBoard board = Load<KanbanBoard>(metaData.FilePath).Result;
        _loadedContainers.Add(guid, board);
        return board;
    }

    public string GetRelativeFilePath(string name) { return Path.Combine(_ContainerDataDir, name); }
    #endregion

    #region Serialization
    Task<bool> ISerializable<bool>.SaveAsync(JsonSerializerSettings _)
    {
        return (this as ISerializable<bool>).SaveAsync();
    }

    async Task<bool> ISerializable<bool>.SaveAsync()
    {
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        string containerDataDir = Path.Combine(localDataDir, _ContainerDataDir);
        string indexFilePath = Path.Combine(localDataDir, _IndexFileName);

        List<Tuple<string, string?>> containerData = new();
        List<Task> containerSaveTasks = new();

        JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);

        foreach (KanbanBoard container in GetLoadedContainers())
        {
            // TODO: Need to synchronize the index and loadedContainer
            Task<KanbanBoard.MetaData> saveTask = container.SaveAsync(SerializerSettings);
            containerSaveTasks.Add(saveTask);
            var awaiter = saveTask.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                if (saveTask.IsFaulted)
                {
                    IDatabase<KanbanBoard>.s_Logger.Warn($"ERROR: Save task faulted with exception: {saveTask.Exception?.Message}");
                }
                KanbanBoard.MetaData metaData = awaiter.GetResult();
                containerData.Add(new(metaData.FilePath, metaData.Data));
            });
        }
        try
        {
            if (!Path.Exists(localDataDir))
                Directory.CreateDirectory(localDataDir);
            if (!Path.Exists(containerDataDir))
                Directory.CreateDirectory(containerDataDir);

            using StreamWriter sw = new(indexFilePath);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, Index);
        }
        catch (JsonSerializationException exp)
        {
            IDatabase<KanbanBoard>.s_Logger.Warn($"Failed to serialze the index file with error: {exp.Message}");
            return false;
        }
        catch (Exception exp)
        {
            IDatabase<KanbanBoard>.s_Logger.Warn($"Failed when saving the database index file with error: {exp.Message}");
            return false;
        }

        await Task.WhenAll(containerSaveTasks);

        try
        {
            Parallel.ForEach(containerData, data =>
            {
                (string filePath, string? serializedData) = data;
                using StreamWriter sw = new(filePath);
                using JsonWriter writer = new JsonTextWriter(sw);
                serializer.Serialize(writer, serializedData);
            });
        }
        catch (AggregateException ae)
        {
            var exceptions = ae.Flatten().InnerExceptions;
            foreach (var ex in exceptions)
            {
                IDatabase<KanbanBoard>.s_Logger.Warn($"Failed to write the container data with error: {ex.Message}");
            }
            if (exceptions.Count > 0)
                return false;
        }
        return true;
    }

    async Task<bool> ISerializable<bool>.LoadAsync()
    {
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        string indexFilePath = Path.Combine(localDataDir, _IndexFileName);

        if (!Path.Exists(indexFilePath))
        {
            IDatabase<KanbanBoard>.s_Logger.Warn($"Failed to find Index file at: {indexFilePath}");
            return false;
        }
        try
        {
            using StreamReader reader = new StreamReader(indexFilePath);
            using JsonReader jsonReader = new JsonTextReader(reader);
            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            Index = serializer.Deserialize<Dictionary<ItemStatus, Dictionary<Guid, KanbanBoard.MetaData>>>(jsonReader) ?? new();
        }
        catch (JsonReaderException exp)
        {
            IDatabase<KanbanBoard>.s_Logger.Warn($"Failed to parse Index file with exception: {exp.Message}");
            throw;
        }
        catch (Exception exp)
        {
            IDatabase<KanbanBoard>.s_Logger.Warn($"Failed to restore the database state with error: {exp.Message}");
            throw;
        }
        return await PreLoadContainers();
    }

    public Task<T> Load<T>(string filePath) where T : new()
    {
        return Task.Run(() =>
        {
            if (!Path.Exists(filePath))
                return new T();
            try
            {
                using StreamReader reader = new StreamReader(filePath);
                using JsonReader jsonReader = new JsonTextReader(reader);
                JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
                return serializer.Deserialize<T>(jsonReader) ?? new T();
            }
            catch (JsonReaderException exp)
            {
                IDatabase<KanbanBoard>.s_Logger.Warn($"Failed to parse file with exception: {exp.Message}");
                throw;
            }
            catch (Exception exp)
            {
                IDatabase<KanbanBoard>.s_Logger.Warn($"Failed to load file with error: {exp.Message}");
                throw;
            }
        });
    }

    #endregion

    private const string _LocalDataDir = ".kanban"; // TODO: Expose this a config var
    private const string _ContainerDataDir = "data";
    private const string _IndexFileName = ".index";
}
