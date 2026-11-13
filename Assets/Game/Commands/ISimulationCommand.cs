namespace GameRTS.Commands
{
    public interface ISimulationCommand
    {
        int Tick { get; }
        int PlayerId { get; }
        int Sequence { get; }
    }
}
