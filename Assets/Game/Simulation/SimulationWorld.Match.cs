using System;

namespace GameRTS.Simulation
{
    public enum MatchOutcome { Ongoing, Victory, Defeat, Draw }
    public sealed partial class SimulationWorld
    {
        private int townA, townB, playerA, playerB;
        public bool IsMatchOver { get; private set; }
        public int WinnerId { get; private set; }
        public int MatchEndTick { get; private set; }
        public void ConfigureTownCenterVictory(int firstTownId, int secondTownId)
        {
            var first = GetDropOff(firstTownId); var second = GetDropOff(secondTownId);
            if (Tick != 0 || townA != 0 || first == null || second == null || first.OwnerId == second.OwnerId)
                throw new InvalidOperationException("Victory needs two opposing Town Centers before tick 1.");
            townA = firstTownId; townB = secondTownId; playerA = first.OwnerId; playerB = second.OwnerId;
        }
        public MatchOutcome GetOutcome(int playerId)
        {
            if (!IsMatchOver || (playerId != playerA && playerId != playerB)) return MatchOutcome.Ongoing;
            return WinnerId == 0 ? MatchOutcome.Draw : WinnerId == playerId ? MatchOutcome.Victory : MatchOutcome.Defeat;
        }
        private void EvaluateMatch()
        {
            if (townA == 0 || IsMatchOver) return;
            bool firstDead = GetDropOff(townA) == null, secondDead = GetDropOff(townB) == null;
            if (!firstDead && !secondDead) return;
            IsMatchOver = true; MatchEndTick = Tick;
            WinnerId = firstDead && secondDead ? 0 : firstDead ? playerB : playerA;
        }
        private void HashMatch(Action<int> add)
        { add(townA); add(townB); add(playerA); add(playerB); add(IsMatchOver ? 1 : 0); add(WinnerId); add(MatchEndTick); }
    }
}
