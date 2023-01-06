namespace Kanban;
using Kanban.Interfaces;

[Serializable]
internal class KanbanList : IKanbanContainer
{
    public int ID { get; init; }
    public string Name { get; set; } = "";
    public string? Details { get; set; }
    public uint Priority { get; set; }
    public IKanbanItem.ItemStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get; init; }

    private readonly List<IKanbanItem> _kanbanItems = new();
    public void AddKanbanItem(IKanbanItem item) => _kanbanItems.Add(item);
    public IKanbanItem GetKanbanItem(int itemId) => _kanbanItems[itemId];
    public void RemoveKanbanItem(int itemId) => _kanbanItems.RemoveAt(itemId);
}
