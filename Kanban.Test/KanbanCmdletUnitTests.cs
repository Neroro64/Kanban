namespace Kanban.Test;

[TestFixture]
public class KanbanCmdletTests
{
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
    }
    [TearDown] public void TearDown() => clearTempFiles();

    [Test]
    public void CanCreateDatabase()
    {
    }
}
