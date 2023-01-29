using Kanban.Abstract;
namespace Kanban;

public class KanbanList : KanbanContainer
{
    public string Name {get; set;} = "KanbanList";
    public new MetaData Meta { get => throw new NotImplementedException(); init => throw new NotImplementedException(); }

}
