namespace Kanban.Prototype.Interfaces;
internal interface IDatabase
{
    /// <summary>
    /// A static function for retrieving the 'relevant' database, or creating it if not exists.
    /// </summary>
    /// <returns>a handler to an IDatabase for managing Kanbanboards</returns>
    public static abstract IDatabase GetInstance();

    public abstract IKanbanContainer CreateKanbanColumn(string name);
    public abstract IKanbanContainer GetKanbanColumn(string name);
    public abstract void RemoveKanbanColumn(string name);
}
