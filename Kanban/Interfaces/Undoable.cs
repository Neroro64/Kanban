namespace Kanban.Interfaces;
public abstract class Undoable 
{
    public class TransactionEventArgs : System.EventArgs
    {
        public Action PerformedAction {get; init;}
        public Action UndoAction {get; init;}
        public TransactionEventArgs(Action action, Action undoAction){
            this.PerformedAction = action;
            this.UndoAction = undoAction;
        }
    } 
    
    protected Stack<TransactionEventArgs> UndoStack {get; init;}
    protected Stack<TransactionEventArgs> RedoStack {get; init;}
    protected event EventHandler<TransactionEventArgs> Transacted;

    public Undoable() 
    { 
        Transacted += AddActionToStack;
        UndoStack = new();
        RedoStack = new();
    }
    public void Undo() 
    {
        if(UndoStack.TryPop(out TransactionEventArgs? latestEvent))
        {
            RedoStack.Push(latestEvent);
            latestEvent.UndoAction();
        }
    }
    public void Redo() 
    {
        if(RedoStack.TryPop(out TransactionEventArgs? latestEvent))
        {
            UndoStack.Push(latestEvent);
            latestEvent.PerformedAction();
        }
    }
    public virtual void OnTransactionPerformed(TransactionEventArgs eventArgs) 
    {
        RedoStack.Clear();
        Transacted(this, eventArgs);
    }

    private void AddActionToStack(object? _, TransactionEventArgs eventArgs) {UndoStack.Push(eventArgs);} 

}