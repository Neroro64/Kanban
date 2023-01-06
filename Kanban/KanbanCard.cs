namespace Kanban;
using Kanban.Interfaces;

[Serializable]
internal class KanbanCard : IKanbanItem
{
    public int ID { get; init; }
    public string Name { get; set; } = "";
    public string? Details { get; set; }
    public uint Priority { get; set; }
    public IKanbanItem.ItemStatus Status { get; set; } = IKanbanItem.ItemStatus.PENDING;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get; init; }
}
