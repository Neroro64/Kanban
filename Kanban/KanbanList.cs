using Kanban.Abstract;
namespace Kanban;

public class KanbanList : KanbanContainer<IKanbanItem>, IIdentifiable
{
    public Guid Guid {get; init;}
    public string Name {get; set;} = "KanbanList";
    public new MetaData Meta { get; init; } = default;

}
