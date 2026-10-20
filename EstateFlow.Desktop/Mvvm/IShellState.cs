namespace EstateFlow.Desktop.Mvvm;

public interface IShellState
{
    Guid? LastOpenedEstateId { get; }
}