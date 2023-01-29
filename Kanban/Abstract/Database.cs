namespace Kanban.Abstract;
using IndexType = Dictionary<ItemStatus, Dictionary<Guid, KanbanContainer.MetaData>>;
public abstract class Database<ContainerType> : Undoable, ISerializable<bool> where ContainerType : KanbanContainer, new()
{
    #region Properties
    protected IndexType Index { get; set; }
    protected JsonSerializerSettings SerializerSettings
    {
        get => new() { Formatting = Formatting.Indented };
    }

    // protected IndexType Index {get; init;} 
    // protected Dictionary<Guid, ContainerType> LoadedContainers {get; init;}
    #endregion

    #region Logger
    protected static readonly ILog s_Logger = LogManager.GetLogger(typeof(Database<ContainerType>));
    #endregion

    #region Container Methods
    public abstract ContainerType? GetContainer(Guid guid);
    public abstract IEnumerable<ContainerType>? FindContainers(string query);
    public abstract ContainerType NewContainer(string name = "NewContainer");
    public abstract void AddContainer(ContainerType newContainer);
    public abstract void RemoveContainer(Guid guid, bool unload=true);
    public abstract void MoveContainer(Guid guid, ItemStatus newStatus);
    public abstract IEnumerable<ContainerType> GetLoadedContainers();
    protected string GetRelativeFilePath(string name) { return Path.Combine(_ContainerDataDir, name); }
    #endregion

    #region Initialization
    protected abstract void Init();
    protected abstract void DeInit();
    protected abstract Task<bool> PreLoadContainers();
    #endregion

    #region Serialization
    /// <summary>
    /// Make the changes on the database permenant. 
    /// This involves serializing and saving of each modified <see cref="IKanbanContainer"/> in the database.
    /// </summary>
    async Task<bool> ISerializable<bool>.SaveAsync()
    {
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        string containerDataDir = Path.Combine(localDataDir, _ContainerDataDir);
        string indexFilePath = Path.Combine(localDataDir, _IndexFileName);

        List<Tuple<string, string?>> containerData = new();
        List<Task> containerSaveTasks = new();

        JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);

        foreach (ContainerType container in GetLoadedContainers())
        {
            // TODO: Need to synchronize the index and loadedContainer
            Task<KanbanContainer.MetaData> saveTask = container.SaveAsync(SerializerSettings);
            containerSaveTasks.Add(saveTask);
            var awaiter = saveTask.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                if (saveTask.IsFaulted)
                {
                    s_Logger.Warn($"ERROR: Save task faulted with exception: {saveTask.Exception?.Message}");
                }
                KanbanContainer.MetaData metaData  = awaiter.GetResult();
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
            s_Logger.Warn($"Failed to serialze the index file with error: {exp.Message}");
            return false;
        }
        catch (Exception exp)
        {
            s_Logger.Warn($"Failed when saving the database index file with error: {exp.Message}");
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
                s_Logger.Warn($"Failed to write the container data with error: {ex.Message}");
            }
            if (exceptions.Count > 0)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Load the index file and load the data of <see cref="ItemStatus.ONGOING"/> containers.
    /// </summary>
    async Task<bool> ISerializable<bool>.LoadAsync()
    {
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        string indexFilePath = Path.Combine(localDataDir, _IndexFileName);

        if (!Path.Exists(indexFilePath))
        {
            s_Logger.Warn($"Failed to find Index file at: {indexFilePath}");
            return false;
        }
        try
        {
            using StreamReader reader = new StreamReader(indexFilePath);
            using JsonReader jsonReader = new JsonTextReader(reader);
            JsonSerializer serializer = JsonSerializer.Create(SerializerSettings);
            Index = serializer.Deserialize<IndexType>(jsonReader) ?? new();
        }
        catch (JsonReaderException exp)
        {
            s_Logger.Warn($"Failed to parse Index file with exception: {exp.Message}");
            throw;
        }
        catch (Exception exp)
        {
            s_Logger.Warn($"Failed to restore the database state with error: {exp.Message}");
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
                s_Logger.Warn($"Failed to parse file with exception: {exp.Message}");
                throw;
            }
            catch (Exception exp)
            {
                s_Logger.Warn($"Failed to load file with error: {exp.Message}");
                throw;
            }
        });
    }

    public Task<bool> SaveAsync(JsonSerializerSettings _)
    {
        throw new NotImplementedException();
    }
    #endregion

    private const string _LocalDataDir = ".kanban"; // TODO: Expose this a config var
    private const string _ContainerDataDir = "data";
    private const string _IndexFileName = ".index";
    
}
