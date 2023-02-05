namespace Kanban.Abstract;
public interface IKanbanItem : IIdentifiable
{
    public ItemStatus Status { get; set; }
    public new Guid Guid { get; init; }
    public new string Name { get; set; }
    public KanbanContainer<IKanbanItem>? Parent {get; set;}
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; }
    public DateTime DateCreated { get; init; }
    public DateTime? DateClosed { get; init; }
    public DateTime? Deadline { get; set; }
    public IList<IKanbanItem>? Links { get; set; }
    public IList<string>? Comments { get; init; }
}
