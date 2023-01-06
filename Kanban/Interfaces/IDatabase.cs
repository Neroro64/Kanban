namespace Kanban.Interfaces;
public interface IDatabase
{
    static public Lazy<IDatabase>? Database { get; }
     
    public abstract void AddContainer(IKanbanContainer newContainer);
    public abstract IKanbanContainer? GetContainer(int containerID);
    public abstract void RemoveContainer(int containerID);
}
