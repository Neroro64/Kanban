namespace Kanban.Abstract;
public interface IDatabase<ContainerType> : ISerializable<bool>
{
    protected static readonly ILog s_Logger = LogManager.GetLogger(typeof(IDatabase<ContainerType>));

    #region Container Methods
    public ContainerType? GetContainer(Guid guid);
    public IIdentifiable? GetItem(Guid guid);
    public IEnumerable<ContainerType>? FindContainers(string query);
    public ContainerType NewContainer(string name = "NewContainer", bool addToDatabase=true);
    public void AddContainer(ContainerType newContainer);
    public void RemoveContainer(Guid guid, bool unload = true);
    public void MoveContainer(Guid guid, ItemStatus newStatus);
    public IEnumerable<ContainerType> GetLoadedContainers();
    #endregion

    #region Initialization
    public void Init();
    public void DeInit();
    public Task<bool> PreLoadContainers();
    #endregion


}
