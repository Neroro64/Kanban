using Kanban.Abstract;
namespace Kanban;

[Serializable]
public sealed class KanbanCard : Undoable, IKanbanItem
{
    public ItemStatus Status { get; set; } = ItemStatus.PENDING;
    public Guid Guid { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "KanbanCard";
    public Guid? Parent {get; set;} = default;
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; } = ItemPriority.Low;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get; set; }
    public DateTime? Deadline { get; set; }
    public IList<Guid> Links { get; init; } = new List<Guid>();
    public IList<string> Comments { get; init; } = new List<string>();

}
