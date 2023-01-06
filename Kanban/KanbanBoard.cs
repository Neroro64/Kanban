namespace Kanban;
using Kanban.Interfaces;

[Serializable]
public sealed class KanbanBoard : IKanbanContainer
{
    public int ID { get ; init ; }
    public string Name { get; set; } = "";
    public string? Details { get ; set ; }
    public uint Priority { get ; set ; }
    public IKanbanItem.ItemStatus Status { get; set; } = IKanbanItem.ItemStatus.PENDING;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get ; init ; }

    private readonly Dictionary<string, IKanbanContainer> _kanbanLists = new();

    void AddKanbanContainer(IKanbanContainer item) => _kanbanLists.Add(item.Name, item);
    void IKanbanContainer.AddKanbanItem(IKanbanItem item)
    {
        if (item is IKanbanContainer container)
            _kanbanLists.Add(item.Name, container);
    }

    void RemoveKanbanContainer(string key) => _kanbanLists.Remove(key);
    void IKanbanContainer.RemoveKanbanItem(int id)
    {
        _kanbanLists.Remove(_kanbanLists.First(x => x.Value.ID == id).Key);
    }

    IKanbanContainer GetKanbanContainer(string key) => _kanbanLists[key];
    IKanbanItem IKanbanContainer.GetKanbanItem(int id)
    {
        return _kanbanLists.First(x => x.Value.ID == id).Value;
    }
}
