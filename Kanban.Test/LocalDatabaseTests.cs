using Kanban.Interfaces;

namespace Kanban.Test;

[TestFixture]
public class LocalDatabaseTests
{
    static LocalDatabase? database;

    [SetUp]
    public void Setup() { }

    [Test]
    public void CanCreate()
    {
        database = LocalDatabase.Database.Value;
        Assert.That(database, Is.Not.Null);
    }

    [Test]
    public void CanModifyContainers()
    {
        KanbanBoard container = new()
        {
            ID = 0,
            Name = "Test",
            Priority = 0
        };

        database = LocalDatabase.Database.Value;
        database.AddContainer(container);
        Assert.That(database.Containers, Has.Count.EqualTo(1));

        var containerRef = database.GetContainer(0);
        Assert.That(containerRef, Is.Not.Null);
        Assert.That(containerRef, Is.EqualTo(container));
        Assert.That(containerRef.Name, Is.EqualTo("Test"));

        database.RemoveContainer(0);
        Assert.That(database.Containers, Has.Count.EqualTo(0));
    }

    [Test]
    public void CanSerialize()
    {
        database = LocalDatabase.Database.Value;
        KanbanBoard container = new()
        {
            ID = 0,
            Name = "Test",
            Priority = 0
        };
        database.AddContainer(container);
        string indexFilePath = database.SaveIndexFile();
        Assert.That(new FileInfo(indexFilePath).Exists, Is.EqualTo(true));
    }

    [Test]
    public void CanDeserialize()
    {
        LocalDatabase? _database = new();
        Assert.That(_database, Is.Not.Null);
        Assert.That(_database.Containers, Has.Count.EqualTo(1));
        var board = _database.GetContainer(0);
        Assert.That(board, Is.Not.Null);
        Assert.That(board.Name, Is.EqualTo("Test"));
    }


}
