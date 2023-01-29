using Kanban.Abstract;
namespace Kanban;

public sealed class KanbanBoard :  KanbanContainer, IKanbanItem
{
    public MetaData Meta { get; init; } = default;
    public ItemStatus Status { get; set; } = ItemStatus.PENDING;
    public Guid Guid { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "KanbanBoard";
    public KanbanContainer? Parent { get; set; }
    public string? Details { get; set; }
    public ItemPriority Priority { get; set; } = ItemPriority.Low;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get; init; }
    public DateTime? Deadline { get; set; }
    public IList<IKanbanItem>? Links { get; set; }
    public IList<string>? Comments { get; init; }

    private Dictionary<string, KanbanList> m_lists = new();
    public KanbanBoard(){}
    public KanbanBoard(string name, MetaData meta)
    {
        Meta = meta;
        Name = name;
        Status = ItemStatus.PENDING;
        Guid = Guid.NewGuid();
        Priority = ItemPriority.Low;
        DateCreated = DateTime.Now;
    }
    
    public void AddKanbanList(KanbanList list)
    {
        m_lists.TryAdd(list.Name, list);
    }

    public IEnumerable<KanbanList>? FindKanbanLists(string query)
    {
        return m_lists.Values.Where(item => item.Name.Contains(query));
    }

    public KanbanList? GetKanbanList(string name)
    {
        if (m_lists.TryGetValue(name, out var list))
            return list;
        return null;
    }

    public void RemoveKanbanList(string name)
    {
        if (m_lists.ContainsKey(name))
            m_lists.Remove(name);
    }
    
}
