namespace Kanban.Interfaces;
public interface IKanbanContainer : IKanbanItem
{
    public abstract void AddKanbanItem(IKanbanItem item);
    public abstract IKanbanItem GetKanbanItem(int itemId);
    public abstract void RemoveKanbanItem(int itemId);
}
