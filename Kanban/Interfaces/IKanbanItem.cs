namespace Kanban.Interfaces;
public interface IKanbanItem
{
    public ItemStatus Status { get; set; }
    public abstract Guid Guid { get; init; }
    public abstract string Name { get; set; }
    public abstract IKanbanContainer Parent {get; set;}
    public abstract string? Details { get; set; }
    public abstract uint Priority { get; set; }
    public abstract DateTime DateCreated { get; init; }
    public abstract DateTime? DateClosed { get; init; }
    public abstract DateTime? Deadline { get; set; }
    public abstract IList<IKanbanItem>? Links { get; set; }
    public abstract IList<string>? Comments { get; init; }
}
