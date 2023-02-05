namespace Kanban.Abstract;
public interface IIdentifiable 
{
    public Guid Guid {get; init;}
    public string Name {get; set;}
}
