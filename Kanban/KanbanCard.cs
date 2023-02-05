using Kanban.Abstract;
namespace Kanban;

[Serializable]
public sealed class KanbanCard : Undoable, IKanbanItem
{
    public ItemStatus Status { get; set; } = ItemStatus.PENDING;
    public Guid Guid { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "KanbanCard";
    public KanbanContainer<IKanbanItem>? Parent { get; set; }
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; } = ItemPriority.Low;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get; init; }
    public DateTime? Deadline { get; set; }
    public IList<IKanbanItem>? Links { get; set; }
    public IList<string>? Comments { get; init; }

}
