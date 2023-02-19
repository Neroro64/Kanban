namespace Kanban.Cmdlet;

[Cmdlet(VerbsCommon.Get, "KanbanIndex")]
[OutputType(typeof(Dictionary<ItemStatus, KanbanBoard.MetaData>))]
public class GetKanbanIndexCommand : PSCmdlet
{
    protected override void EndProcessing()
    {
        WriteObject(LocalDatabase.s_Database.Index, true);
    }
}

[Cmdlet(VerbsCommon.Find, "KanbanBoard")]
[OutputType(typeof(List<KanbanBoard>))]
public class FindKanbanBoardCommand : PSCmdlet
{
    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true
    )]
    public string Name {get; set;} = "";

    protected override void EndProcessing()
    {
        WriteObject(LocalDatabase.s_Database.FindContainers(Name), true);
    }
}

[Cmdlet(VerbsCommon.Get, "KanbanBoard")]
[OutputType(typeof(KanbanBoard))]
public class GetKanbanBoardCommand : PSCmdlet
{
    [Parameter(
        Position = 0,
        Mandatory = true,
        ValueFromPipeline = true,
        ValueFromPipelineByPropertyName = true
    )]
    public Guid BoardGuid {get; set;}

    protected override void EndProcessing()
    {
        WriteObject(LocalDatabase.s_Database.GetContainer(BoardGuid));
    }
}

[Cmdlet(VerbsCommon.New, "KanbanBoard")]
[OutputType(typeof(KanbanBoard))]
public class NewKanbanBoardCommand : PSCmdlet
{
    [Parameter(
        Position = 0,
        Mandatory = true
    )]
    public string Name {get; set;} = "";

    [Parameter]
    public ItemStatus Status {get; set;} = ItemStatus.PENDING;
    [Parameter]
    public ItemPriority Priority {get; set;} = ItemPriority.Low;
    [Parameter]
    public string? Details {get; set;}
    [Parameter]
    public DateTime? Deadline {get; set;}

    protected override void EndProcessing()
    {
        KanbanBoard board = LocalDatabase.s_Database.NewContainer(Name);
        board.Status = Status;
        board.Priority = Priority;
        board.Details = Details;
        board.Deadline = Deadline;
        WriteObject(board);
    }
}

[Cmdlet(VerbsCommon.Remove, "KanbanBoard")]
public class RemoveKanbanBoardCommand : PSCmdlet
{
    [Parameter(
        Position = 0,
        Mandatory = true
    )]
    public Guid BoardGuid {get; set;}

    protected override void EndProcessing()
    {
        LocalDatabase.s_Database.RemoveContainer(BoardGuid);
    }
}