using Kanban.Abstract;
namespace Kanban;

public sealed class KanbanBoard :  KanbanContainer<KanbanList>, IKanbanItem
{
    public ItemStatus Status { get; set; } = ItemStatus.PENDING;
    public new Guid Guid { get; init; } = Guid.NewGuid();
    public new string Name { get; set; } = "KanbanBoard";
    [JsonIgnore] public new Guid? Parent {get; set;} = default;
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; } = ItemPriority.Low;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get; set; }
    public DateTime? Deadline { get; set; }
    public IList<Guid> Links { get; init; } = new List<Guid>();
    public IList<string> Comments { get; init; } = new List<string>();
    public KanbanBoard(){}
    public KanbanBoard(string name, MetaData meta)
    {
        FileMeta = meta;
        Name = name;
        Status = ItemStatus.PENDING;
        Guid = Guid.NewGuid();
        Priority = ItemPriority.Low;
        DateCreated = DateTime.Now;
    }
}
