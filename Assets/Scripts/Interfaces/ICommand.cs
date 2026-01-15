using System;

namespace Assets.Scripts.Interfaces
{
    public interface ICommand
    {
        float ExecutionTime { get; set; }
        void Execute();
        void Reset();
    }
    public interface ICommandRegistry
    {
        void RegisterCommandForModel(int modelID, ICommand command);
    }
}
