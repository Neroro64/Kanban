namespace Kanban;
using Newtonsoft.Json;
using Kanban.Interfaces;
using System.IO;
using log4net;
using log4net.Config;

public sealed class LocalDatabase : IDatabase
{
    private static readonly ILog logger = LogManager.GetLogger(typeof(LocalDatabase));
    public static Lazy<LocalDatabase> Database {
        get => new(() => new LocalDatabase());
    }
    public List<IKanbanContainer> Containers { get; } = new();

    private static readonly string _LocalDataDir = ".kanban"; // TODO: Expose this a config var
    private static readonly string _IndexFileName = ".index";
    private readonly string _indexFilePath = "";

    public LocalDatabase()
    {
        FileInfo log4netConfig = new("log4net.config");
        if (!log4netConfig.Exists)
            BasicConfigurator.Configure();
        else
            XmlConfigurator.Configure(configFile:log4netConfig);

        string currentDir = Directory.GetCurrentDirectory();
        string localDataDir = Path.Combine(currentDir, _LocalDataDir);

        if (!Path.Exists(localDataDir))
        {
            Directory.CreateDirectory(localDataDir);
        }
        else
        {
            _indexFilePath = Path.Combine(localDataDir, _IndexFileName);
            if (!Path.Exists(_indexFilePath))
            {
                logger.Warn($"Failed to find Index file at: {_indexFilePath}");
            }
            else
            {
                try 
                {
                    var containers = LoadIndexFile(_indexFilePath) ?? new();
                    foreach(var container in containers)
                    {
                        var cast = container as IKanbanContainer;
                        if (cast != null)
                            Containers.Add(cast);
                    }
                }
                catch (JsonReaderException exp)
                {
                    logger.Warn($"Failed to parse Index file with exception: {exp.Message}");
                    Containers = new();
                }
            }
        }
    }
    ~LocalDatabase()
    {
        SaveIndexFile();
    }

    public void AddContainer(IKanbanContainer newContainer) => Containers.Add(newContainer);
    public void RemoveContainer(int containerID) => Containers.RemoveAt(containerID);
    public IKanbanContainer? GetContainer(int containerID) => Containers.ElementAtOrDefault(containerID);
    public string SaveIndexFile()
    {
        using StreamWriter sw = new(_indexFilePath);
        using JsonWriter writer = new JsonTextWriter(sw);
        JsonSerializer serializer = new();
        serializer.Serialize(writer, Containers);
        return _indexFilePath;
    }
    public List<KanbanBoard>? LoadIndexFile(string indexFilePath)
    {
        using StreamReader reader = new StreamReader(indexFilePath);
        using JsonReader jsonReader = new JsonTextReader(reader);
        JsonSerializer serializer = new();
        return serializer.Deserialize<List<KanbanBoard>>(jsonReader);
    }
}
