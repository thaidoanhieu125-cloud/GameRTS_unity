using System;

namespace GameRTS.Commands
{
    // Removes stock for testing only; does not deposit resources in the player's wallet.
    public sealed class ResourceDebugCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public int NodeId { get; }
        public int Amount { get; }

        public ResourceDebugCommand(int tick, int playerId, int sequence, int nodeId, int amount)
        {
            if (tick <= 0 || playerId <= 0 || sequence < 0 || nodeId <= 0 || amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount));
            Tick = tick; PlayerId = playerId; Sequence = sequence; NodeId = nodeId; Amount = amount;
        }
    }
}
