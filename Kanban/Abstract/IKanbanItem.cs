namespace Kanban.Abstract;
public interface IKanbanItem : IIdentifiable
{
    public ItemStatus Status { get; set; }
    public new Guid Guid { get; init; }
    public new string Name { get; set; }
    public new Guid? Parent {get; set;}
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; }
    public DateTime DateCreated { get; init; }
    public DateTime? DateClosed { get; set; }
    public DateTime? Deadline { get; set; }
    public IList<Guid> Links { get; init; }
    public IList<string> Comments { get; init; }
}
