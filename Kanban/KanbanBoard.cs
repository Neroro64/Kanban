using Kanban.Abstract;
namespace Kanban;

public sealed class KanbanBoard :  KanbanContainer<KanbanList>, IKanbanItem
{
    public MetaData Meta { get; init; } = default;
    public ItemStatus Status { get; set; } = ItemStatus.PENDING;
    public Guid Guid { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "KanbanBoard";
    public KanbanContainer<IKanbanItem>? Parent {get; set;} = default;
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; } = ItemPriority.Low;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get; init; }
    public DateTime? Deadline { get; set; }
    public IList<IKanbanItem>? Links { get; set; }
    public IList<string>? Comments { get; init; }

    private Dictionary<string, KanbanList> m_lists = new();
    public KanbanBoard(){}
    public KanbanBoard(string name, MetaData meta)
    {
        Meta = meta;
        Name = name;
        Status = ItemStatus.PENDING;
        Guid = Guid.NewGuid();
        Priority = ItemPriority.Low;
        DateCreated = DateTime.Now;
    }
}
