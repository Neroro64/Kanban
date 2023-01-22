# KanbanScheduler
A simple C# implementation of Kanban for project / task management. It provides intuitive PowerShell and TUI interfaces.

## Use cases and constraints
### Use cases
- Given a project, you can create multiple **KanbanBoard** for tracking milestones, features or epic tasks
- For a **KanbanBoard**, you can create multiple **KanbanColumns** and **Swimlanes** for orgranizing and tracking tasks
- For a **KanbanColumns**, you can 
    - create and add **KanbanItem**
    - create **KanbanEvents** that will fire upon entering or existing of an item. Example: set the item status to "OnGoing" upon entering.
- For a **KanbanItem**, you can add information, comments, links to other items, deadline.

### Constraints for MVP
- No time tracking features
- No collarboration features 
- No cloud-sharing features
- No GUI
- Not possibel to add other information than text to the task items
- No security features

## High level system design 
- [System Architecture Overview](docs/SystemArchitectureOverview.md)
- [Data Structure Overview](docs/DataStructureOverview.md)
- [Operation Flowchart](docs/OperationFlowChart.md)

## Core components 
- [Core Component Design](docs/CoreComponentDesign.md)

## Scale the design
