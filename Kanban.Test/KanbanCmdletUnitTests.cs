using System.Management.Automation;
using System.Management.Automation.Runspaces;
namespace Kanban.Test;

[TestFixture]
public class KanbanCmdletTests
{
    private const string _LocalDataDir = ".kanban"; // TODO: Make this configurable
    private const string _ContainerDataDir = "data";
    private const string _IndexFileName = ".index";
    // private Runspace _runspace;

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
        // var initialSessionState = InitialSessionState.CreateDefault();
        // initialSessionState.Commands.Add(
        //     new SessionStateCmdletEntry("Test-SampleCmdlet", typeof(TestSampleCmdletCommand), null)
        // );
        // _runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        // _runspace.Open();
    } 
    [TearDown] public void TearDown() => clearTempFiles();

    [Test]
    public void CanGetIndex()
    {
        // using var powershell = PowerShell.Create(_runspace.InitialSessionState);
        
        // // Configure Command
        // var command = new Command("Get-KanbanIndex");
        // powershell.Commands.AddCommand(command);
        
        // // Run Command
        // var result = powershell.Invoke<Dictionary<ItemStatus, KanbanBoard.MetaData>>()[0];

        // // Assert
        // Assert.That(result, Is.Not.Null);
        var cmdlet = new GetKanbanIndexCommand();
        var result = cmdlet.Invoke<Dictionary<ItemStatus, Dictionary<Guid, KanbanBoard.MetaData>>>();
        //var result = cmdlet.Invoke<Dictionary<ItemStatus, Dictionary<Guid, KanbanBoard.MetaData>>>();
        var newCmdlet = new NewKanbanBoardCommand() { Name = "Yo", } ;
        var board = newCmdlet.Invoke<KanbanBoard>();
        Assert.That(result, Is.Not.Null);
    }
}