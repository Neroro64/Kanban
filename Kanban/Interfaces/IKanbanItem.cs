namespace Kanban.Interfaces;
public interface IKanbanItem
{
    public enum ItemStatus { PENDING, ONGOING, WAITING, COMPLETED }
    public abstract int ID { get; init; }
    public abstract string Name { get; set; }
    public abstract string? Details { get; set; }
    public abstract uint Priority { get; set; }
    public abstract ItemStatus Status { get; set;}
    public abstract DateTime DateCreated { get; init; }
    public abstract DateTime? DateClosed { get; init; }
}
