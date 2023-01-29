namespace Kanban.Test;

[TestFixture]
public class LocalDatabaseTests
{
    LocalDatabase? database;
    private const string _LocalDataDir = ".kanban"; // TODO: Expose this a config var
    private const string _ContainerDataDir = "data";
    private const string _IndexFileName = ".index";
    [SetUp]
    public void Setup() 
    { 
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        if (Path.Exists(localDataDir))
            Directory.Delete(localDataDir, true);
        database = LocalDatabase.s_Database;
    }

    [Test]
    public void CanCreate()
    {
        Assert.That(database, Is.Not.Null);
    }

    [Test]
    public void CanCreate_ModifyContainers()
    {
        // New Container
        KanbanBoard container = database.NewContainer();

        // Add Container
        database.AddContainer(container);
        Assert.That(database.GetLoadedContainers().Count(), Is.EqualTo(1));
        var containers = database.FindContainers("Container");
        Assert.That(containers, Is.Not.Null);
        Assert.That(containers.Count(), Is.EqualTo(1));
        Assert.That(database.Index[Abstract.ItemStatus.PENDING].ContainsKey(container.Guid), Is.True);

        // Modify Container
        var containerRef = database.GetContainer(container.Guid);
        Assert.That(containerRef, Is.Not.Null);
        containerRef.Name = "ModifiedContainer";
        containerRef.Priority = Abstract.ItemPriority.Critical;

        Assert.That(database.GetContainer(container.Guid)?.Name, Is.EqualTo("ModifiedContainer"));
        database.RemoveContainer(container.Guid);
        Assert.That(database.GetLoadedContainers().Count(), Is.EqualTo(0));
        Assert.That(database.Index[Abstract.ItemStatus.PENDING].ContainsKey(container.Guid), Is.False);
    }

    // [Test]
    // public void CanSerialize()
    // {
    //     KanbanBoard container = new()
    //     {
    //         ID = 0,
    //         Name = "Test",
    //         Priority = 0
    //     };
    //     database.AddContainer(container);
    //     string indexFilePath = database.SaveIndexFile();
    //     Assert.That(new FileInfo(indexFilePath).Exists, Is.EqualTo(true));
    // }

    // [Test]
    // public void CanDeserialize()
    // {
    //     LocalDatabase? _database = new();
    //     Assert.That(_database, Is.Not.Null);
    //     Assert.That(_database.Containers, Has.Count.EqualTo(1));
    //     var board = _database.GetContainer(0);
    //     Assert.That(board, Is.Not.Null);
    //     Assert.That(board.Name, Is.EqualTo("Test"));
    // }



}
