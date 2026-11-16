using System;

namespace GameRTS.Simulation
{
    public enum AnimalKind { Deer, Boar, Elephant }
    public sealed class AnimalDefinition
    {
        public string TypeId { get; }
        public AnimalKind Kind { get; }
        public int Food { get; }
        public int Speed { get; }
        public CombatStats Stats { get; }
        public bool Aggressive => Kind != AnimalKind.Deer;
        public AnimalDefinition(string typeId, AnimalKind kind, int food, int speed, CombatStats stats)
        {
            if (string.IsNullOrWhiteSpace(typeId) || !Enum.IsDefined(typeof(AnimalKind), kind) || food < 1 || food > 10000000
                || speed < 1 || speed > 20000 || stats == null || stats.IsRanged) throw new ArgumentException("Invalid animal data.");
            TypeId = typeId; Kind = kind; Food = food; Speed = speed; Stats = stats;
        }
    }
    public sealed class AnimalState
    {
        public AnimalDefinition Definition { get; }
        public int ThreatId { get; internal set; }
        public int FleeUntil { get; internal set; }
        public int NextThinkTick { get; internal set; }
        public int CorpseResourceId { get; internal set; }
        internal AnimalState(AnimalDefinition definition) { Definition = definition; }
    }
}
