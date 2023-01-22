namespace Kanban.Interface;

public interface IKanbanEvent 
{
    public abstract class IKanbanEventArgs : System.EventArgs {};
    public EventHandler<IKanbanEventArgs> EventFired {get; init;}
    public virtual void OnEventFired(IKanbanEventArgs eventArgs) { EventFired?.Invoke(this, eventArgs); }
} 