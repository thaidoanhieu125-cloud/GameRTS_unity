using System.Collections.Generic;

namespace GameRTS.Commands
{
    public sealed class HuntCommand : ISimulationCommand
    {
        public int Tick { get; }
        public int PlayerId { get; }
        public int Sequence { get; }
        public IReadOnlyList<int> UnitIds { get; }
        public int AnimalId { get; }
        public HuntCommand(int tick, int player, int sequence, IEnumerable<int> ids, int animalId)
        {
            var checkedIds = new AttackCommand(tick, player, sequence, ids, AttackTargetKind.Unit, animalId);
            Tick = tick; PlayerId = player; Sequence = sequence; UnitIds = checkedIds.UnitIds; AnimalId = animalId;
        }
    }
}
