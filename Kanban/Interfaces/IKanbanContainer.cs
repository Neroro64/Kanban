namespace Kanban.Interfaces;
public interface IKanbanContainer : IKanbanItem, ISerializable<ContainerMetaData>
{
    public abstract void AddKanbanItem(IKanbanItem item);
    public abstract IKanbanItem GetKanbanItem(Guid itemGuid);
    public abstract IKanbanItem FindKanbanItem(Guid itemGuid);
    public abstract void RemoveKanbanItem(Guid itemGuid);
    public static abstract IKanbanContainer CreatePlaceHolder(ContainerMetaData metaData);
}

public record struct ContainerMetaData 
{ 
    public string Name {get; init;}
    public string FilePath {get; init;}
    public string? Data {get; init;}
    public ContainerMetaData(string name, string filePath, string? data){
        this.Name = name;
        this.FilePath = filePath;
        this.Data = data;
    }
}

