using System;
namespace Kanban.Prototype.Interfaces;
internal interface IKanbanContainer
{
    internal abstract IEnumerable<IKanbanItem> m_container { get; init; }
    public abstract void AddKanbanItem(IKanbanItem item);
    public abstract IKanbanItem GetKanbanItem(uint id);
    public abstract void RemoveKanbanItem(uint id);
    public abstract uint Length { get; }

}
