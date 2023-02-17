namespace Kanban.Test;

[TestFixture]
public class LocalDatabaseUnitTests
{
    LocalDatabase? database;
    private const string _LocalDataDir = ".kanban"; // TODO: Make this configurable
    private const string _ContainerDataDir = "data";
    private const string _IndexFileName = ".index";
    private void clearTempFiles()
    {
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        if (Path.Exists(localDataDir))
        {
            Directory.Delete(localDataDir, true);
            if (Path.Exists("App.log"))
                File.Delete("App.log");
        }
    }
    [SetUp] public void Setup()
    {
       clearTempFiles();
       database = LocalDatabase.s_Database;
    }
    [TearDown] public void TearDown() => clearTempFiles();

    [Test]
    public void CanCreateDatabase()
    {
        Assert.That(database, Is.Not.Null);
    }

    [Test]
    public void CanCreate_ModifyContainers()
    {
        // New Container
        KanbanBoard container = database!.NewContainer();

        // Add Container
        Assert.That(database.GetLoadedContainers().Count(), Is.EqualTo(1));
        var containers = database.FindContainers("Container");
        Assert.That(containers, Is.Not.Null);
        Assert.That(containers!.Count(), Is.EqualTo(1));
        Assert.That(database.Index[Abstract.ItemStatus.PENDING].ContainsKey(container.Guid), Is.True);

        // Modify Container
        var containerRef = database.GetContainer(container.Guid);
        Assert.That(containerRef, Is.Not.Null);
        containerRef!.Name = "ModifiedContainer";
        containerRef.Priority = Abstract.ItemPriority.Critical;

        // Move Container
        database.MoveContainer(containerRef.Guid, Abstract.ItemStatus.ARCHIVED);
        containerRef = database.GetContainer(containerRef.Guid);
        Assert.That(containerRef, Is.Not.Null);
        Assert.That(containerRef!.Status, Is.EqualTo(Abstract.ItemStatus.ARCHIVED));

        // Remove Container
        Assert.That(database.GetContainer(container.Guid)?.Name, Is.EqualTo("ModifiedContainer"));
        database.RemoveContainer(container.Guid);
        Assert.That(database.GetLoadedContainers().Count(), Is.EqualTo(0));
        Assert.That(database.Index[Abstract.ItemStatus.PENDING].ContainsKey(container.Guid), Is.False);
    }

    [Test]
    public void CanCreate_ModifyCard()
    {
        // Create new Card
        KanbanCard card = new() { Name = "TestKanbanCard" };
        Assert.That(card.Status, Is.EqualTo(Abstract.ItemStatus.PENDING));
        Assert.That(card.Priority, Is.EqualTo(Abstract.ItemPriority.Low));

        // Modify the fields
        card.Status = ItemStatus.ONGOING;
        card.Priority = ItemPriority.High;
        card.Details = "Test details";
        card.DateClosed = DateTime.Now;
        card.Deadline = DateTime.Now.AddDays(5);
        card.Comments.Add("First comment");

        // Link cards
        KanbanCard newCard = new() { Name = "NewTestKanbanCard" };
        card.Links.Add(newCard.Guid);

        Assert.That(card.Links.Count, Is.EqualTo(1));
        Assert.That(card.Links[0], Is.EqualTo(newCard.Guid));
    }

    [Test]
    public void CanCreate_ModifyList()
    {
        KanbanBoard container = database!.NewContainer();

        // Add Container
        KanbanList kanbanList = new() { Name = "TestKanbanList" };
        container.Add(kanbanList);
        Assert.That(container.Count(), Is.EqualTo(1));
        Assert.That(database.GetItem(kanbanList.Guid), Is.Not.Null);

        // Remove Container
        container.Remove(kanbanList.Guid);
        Assert.That(container.Count(), Is.EqualTo(0));

        // Add item to the list
        KanbanCard card = new()
        {
            Name = "TestCard",
        };
        kanbanList.Add(card);
        Assert.That(kanbanList.Count(), Is.EqualTo(1));
        Assert.That(card.Parent, Is.Not.Null);
        KanbanCard? item = default;
        Assert.That(kanbanList.TryGet(card.Guid, ref item), Is.True);
        Assert.That(item!.Parent, Is.EqualTo(kanbanList.Guid));

        // Remove the item
        kanbanList.Remove(card.Guid);
        Assert.That(kanbanList.Count(), Is.EqualTo(0));
    }

    [Test]
    public void CanSerialize_Empty()
    {
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        
        Assert.That(Path.Exists(localDataDir), Is.False);
        database!.DeInit();
        Assert.That(Path.Exists(localDataDir), Is.True);
        
        string[] files = Directory.GetFiles(localDataDir, ".", System.IO.SearchOption.AllDirectories);
        string[] subDirs = Directory.GetDirectories(localDataDir);

        Assert.That(files.Length, Is.GreaterThan(0));
        Assert.That(files.Any(str => str.Contains(_IndexFileName)), Is.True);
        Assert.That(subDirs.Length, Is.GreaterThan(0));
        Assert.That(subDirs.Any(str => str.Contains(_ContainerDataDir)), Is.True);
    }

    [Test]
    public void CanDeserialize_Empty()
    {
        database!.DeInit();
        database = new LocalDatabase();
        database.Init();
    }
    [Test]
    public void CanSerialize_Deserializae_NormalFlow()
    {
        /* 
        Creating the following 
        TestDatabase:
            KanbanBoard_0:
                KanbanList_0:
                    KanbanCard_0
                    KanbanCard_1
                KanbanList_1:
                    KanbanCard_2
            KanbanBoard_1:
                KanbanList_2:
                    KanbanCard_3
        */
        string boardName0 = "Board_0";
        string boardName1 = "Board_1";
        KanbanBoard[] containers = new KanbanBoard[]
        {
            database!.NewContainer(boardName0),
            database!.NewContainer(boardName1)
        };
        database.MoveContainer(containers[1].Guid, ItemStatus.ONGOING);

        KanbanList[] lists = new[] 
        { 
            new KanbanList(){Name = "List_0"},
            new KanbanList(){Name = "List_1"},
            new KanbanList(){Name = "List_2"}
        };
        containers[0].Add(lists[0]);
        containers[0].Add(lists[1]);
        containers[1].Add(lists[2]);

        KanbanCard[] cards = new[] 
        {
            new KanbanCard() { Name = "Card_0", Priority = ItemPriority.High },
            new KanbanCard() { Name = "Card_1", Deadline = DateTime.Now.AddDays(1)},
            new KanbanCard() { Name = "Card_2", Details = "Test details", Status = ItemStatus.ONGOING},
            new KanbanCard() { Name = "Card_3", Comments = new List<string>(){ "Test comment" }},
        };
        for (int i = 0; i < 4; ++i)
            cards[i].Links.Add(cards[(i+1)%4].Guid);

        lists[0].Add(cards[0]);
        lists[0].Add(cards[1]);
        lists[1].Add(cards[2]);
        lists[2].Add(cards[3]);
        
        // Save to disk
        database.DeInit();

        // Verify that both KanbanBoards have been saved
        string localDataDir = Path.Combine(Directory.GetCurrentDirectory(), _LocalDataDir);
        string[] files = Directory.GetFiles(localDataDir, ".", System.IO.SearchOption.AllDirectories);
        Assert.That(files.Length, Is.GreaterThan(1));

        string boardFilename_0 = files.Where(filename => filename.Contains(boardName0)).First();
        string boardFilename_1 = files.Where(filename => filename.Contains(boardName1)).First();
        Assert.That(boardFilename_0, Is.Not.Null);
        Assert.That(string.IsNullOrWhiteSpace(boardFilename_0), Is.False);
        Assert.That(boardFilename_1, Is.Not.Null);
        Assert.That(string.IsNullOrWhiteSpace(boardFilename_1), Is.False);

        FileInfo boardFile_0 = new(boardFilename_0);
        FileInfo boardFile_1 = new(boardFilename_1);
        Assert.That(boardFile_0.Length, Is.GreaterThan(0));
        Assert.That(boardFile_1.Length, Is.GreaterThan(1));

        // Deserialize
        LocalDatabase newDatabase = new();
        newDatabase.Init();
        var containerRef_0 = newDatabase.GetContainer(containers[0].Guid);
        var containerRef_1 = newDatabase.GetContainer(containers[1].Guid);
        Assert.That(containerRef_0, Is.Not.Null);
        Assert.That(containerRef_1, Is.Not.Null);
        KanbanList? listRef = default;
        Assert.That(containerRef_0!.TryGet(lists[0].Guid, ref listRef), Is.True);
        KanbanCard? cardRef = default;
        Assert.That(listRef!.TryGet(cards[0].Guid, ref cardRef), Is.True);
        Assert.That(listRef.Find(cards[1].Name)?.Count(), Is.GreaterThan(0));
        Assert.That(newDatabase.GetItem(cards[2].Guid), Is.Not.Null);
        Assert.That(newDatabase.GetItem(cards[3].Guid), Is.Not.Null);

        KanbanCard? card = cardRef as KanbanCard;
        for (int i = 0; i < 4; ++i)
        {
            Assert.That(card!.Links.Count, Is.GreaterThan(0));
            card = (KanbanCard?) newDatabase.GetItem(card!.Links[0]);
        }
    }
}
