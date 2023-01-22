namespace Kanban;

using System.Collections.Generic;
using System.Threading.Tasks;
using Kanban.Interfaces;
using Newtonsoft.Json;

[Serializable]
public sealed class KanbanBoard : Undoable, IKanbanContainer
{
    public int ID { get ; init ; }
    public string Name { get; set; } = "";
    public string? Details { get ; set ; }
    public uint Priority { get ; set ; }
    public IKanbanItem.ItemStatus Status { get; set; } = IKanbanItem.ItemStatus.PENDING;
    public DateTime DateCreated { get; init; } = DateTime.Now;
    public DateTime? DateClosed { get ; init ; }
    ItemStatus IKanbanItem.Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Guid Guid { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }
    public IKanbanContainer Parent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public DateTime? Deadline { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IList<IKanbanItem>? Links { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IList<string>? Comments { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }

    private readonly Dictionary<string, IKanbanContainer> _kanbanLists = new();

    public KanbanBoard(ContainerMetaData metaData) 
    {

    }
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

    public IKanbanItem GetKanbanItem(Guid itemGuid)
    {
        throw new NotImplementedException();
    }

    public IKanbanItem FindKanbanItem(Guid itemGuid)
    {
        throw new NotImplementedException();
    }

    public void RemoveKanbanItem(Guid itemGuid)
    {
        throw new NotImplementedException();
    }

    public static IKanbanContainer CreatePlaceHolder(ContainerMetaData metaData)
    {
        throw new NotImplementedException();
    }

    public Task<Tuple<string, string>> SaveAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Tuple<string, string>> SaveAsync(JsonSerializer _)
    {
        throw new NotImplementedException();
    }

    public Task<bool> LoadAsync()
    {
        throw new NotImplementedException();
    }
}
