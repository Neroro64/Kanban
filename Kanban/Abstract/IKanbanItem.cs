namespace Kanban.Abstract;
public interface IKanbanItem
{
    public ItemStatus Status { get; set; }
    public Guid Guid { get; init; }
    public string Name { get; set; }
    public KanbanContainer? Parent {get; set;}
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; }
    public DateTime DateCreated { get; init; }
    public DateTime? DateClosed { get; init; }
    public DateTime? Deadline { get; set; }
    public IList<IKanbanItem>? Links { get; set; }
    public IList<string>? Comments { get; init; }
}
