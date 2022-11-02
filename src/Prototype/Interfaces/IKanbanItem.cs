namespace Kanban.Prototype.Interfaces;
internal interface IKanbanItem
{
    internal abstract uint m_id { get; init; }
    internal abstract string m_name { get; set; }
    internal abstract uint m_priority { get; set; }
    internal abstract uint m_dateCreated { get; init; }
    internal abstract uint m_dateClosed { get; init; }
}
