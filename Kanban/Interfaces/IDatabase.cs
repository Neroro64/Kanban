namespace Kanban.Interfaces;
using IndexType = Dictionary<ItemStatus, Dictionary<Guid, ContainerMetaData>>;
/// <summary>
/// Interface that specifies a singelton Database construct that contains and manages all <see cref="IKanbanContainer"/> in the current project.
/// </summary>
/// <typeparam name="ContainerType">An implementation of <see cref="IKanbanContainer"></see></typeparam>
public interface IDatabase<ContainerType> : ISerializable<bool> where ContainerType : IKanbanContainer, new()

{
    #region Properties
    /// <summary>
    /// A reference to the singelton lazy-initialized Database construct.
    /// </summary>
    static public Lazy<IDatabase<ContainerType>> Database { get => Database ?? new(); }

    // protected IndexType Index {get; init;} 
    // protected Dictionary<Guid, ContainerType> LoadedContainers {get; init;}
    #endregion

    #region Logger
    private static readonly ILog logger = LogManager.GetLogger(typeof(IDatabase<ContainerType>));
    #endregion

    #region Container Methods
    public abstract ContainerType? GetContainer(Guid guid);
    public abstract ContainerType? FindContainer(string query);
    public abstract void AddContainer(ContainerType newContainer, ItemStatus status=ItemStatus.PENDING);
    public abstract void RemoveContainer(Guid guid);
    public abstract void MoveContainer(Guid guid, ItemStatus newStatus);
    public abstract IEnumerable<ContainerType> GetLoadedContainers();
    protected abstract IndexType GetIndex();
    protected abstract void SetIndex();
    #endregion

    #region Method
    protected abstract void Init();
    protected abstract void DeInit();
    protected abstract Task<bool> PreLoadContainers();
    #endregion

    #region Serialization
    /// <summary>
    /// Make the changes on the database permenant. 
    /// This involves serializing and saving of each modified <see cref="IKanbanContainer"/> within the database.
    /// </summary>
    async Task<bool> ISerializable<bool>.SaveAsync()
    {
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        string containerDataDir = Path.Combine(localDataDir, _ContainerDataDir);
        string indexFilePath = Path.Combine(localDataDir, _IndexFileName);

        List<Tuple<string, string?>> containerData = new();
        List<Task> containerSaveTasks = new();

        JsonSerializer serializer = new(); // NOTE: Addtional formatting might be required

        foreach (ContainerType container in GetLoadedContainers())
        {
            Task<ContainerMetaData> saveTask = container.SaveAsync(serializer);
            containerSaveTasks.Add(saveTask);
            var awaiter = saveTask.GetAwaiter();
            awaiter.OnCompleted(() =>
            {
                if (saveTask.IsFaulted)
                {
                    logger.Warn($"ERROR: Save task faulted with exception: {saveTask.Exception?.Message}");
                }
                ContainerMetaData metaData  = awaiter.GetResult();
                containerData.Add(new(metaData.FilePath, metaData.Data));
            });
        }

        await Task.WhenAll(containerSaveTasks);
        
        try
        {
            if (!Path.Exists(localDataDir))
                Directory.CreateDirectory(localDataDir);
            if (!Path.Exists(containerDataDir))
                Directory.CreateDirectory(containerDataDir);

            using StreamWriter sw = new(indexFilePath);
            using JsonWriter writer = new JsonTextWriter(sw);
            serializer.Serialize(writer, GetIndex());

        }
        catch (JsonSerializationException exp)
        {
            logger.Warn($"Failed to serialze the index file with error: {exp.Message}");
            return false;
        }
        catch (Exception exp)
        {
            logger.Warn($"Failed when saving the database index file with error: {exp.Message}");
            return false;
        }

        try
        {
            Parallel.ForEach(containerData, data =>
            {
                (string filePath, string? serializedData) = data;
                using StreamWriter sw = new(Path.Join(containerDataDir, filePath));
                using JsonWriter writer = new JsonTextWriter(sw);
                serializer.Serialize(writer, serializedData);
            });
        }
        catch (AggregateException ae)
        {
            var exceptions = ae.Flatten().InnerExceptions;
            foreach (var ex in exceptions)
            {
                logger.Warn($"Failed to write the container data with error: {ex.Message}");
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
            logger.Warn($"Failed to find Index file at: {indexFilePath}");
            return false;
        }
        try
        {
            using StreamReader reader = new StreamReader(indexFilePath);
            using JsonReader jsonReader = new JsonTextReader(reader);
            JsonSerializer serializer = new();
            IndexType indexFile = serializer.Deserialize<IndexType>(jsonReader) ?? new();
            SetIndex(indexFile);
            
        }
        catch (JsonReaderException exp)
        {
            logger.Warn($"Failed to parse Index file with exception: {exp.Message}");
            throw;
        }
        catch (Exception exp)
        {
            logger.Warn($"Failed to restore the database state with error: {exp.Message}");
            throw;
        }
        return await PreLoadContainers();
    }
    #endregion

    private const string _LocalDataDir = ".kanban"; // TODO: Expose this a config var
    private const string _ContainerDataDir = "data"; // TODO: Expose this a config var
    private const string _IndexFileName = ".index";
}
