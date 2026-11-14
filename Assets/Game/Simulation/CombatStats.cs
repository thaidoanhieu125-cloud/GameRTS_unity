using System;

namespace GameRTS.Simulation
{
    public enum UnitClass { Generic, Villager, Spearman, Archer, Cavalry }
    // Trusted match configuration. Distances/speeds use 1000 simulation units per tile.
    public sealed class CombatStats
    {
        public int MaxHP { get; }
        public int Armor { get; }
        public int AttackDamage { get; }
        public int AttackIntervalTicks { get; }
        public int AttackRange { get; }
        public int ProjectileSpeed { get; }
        public int AggroRange { get; }
        public UnitClass Class { get; }
        public bool IsRanged => ProjectileSpeed > 0;
        public static readonly CombatStats Passive = new CombatStats();
        public CombatStats(int maxHP = 40, int armor = 0, int attackDamage = 0, int attackIntervalTicks = 20,
            int attackRange = 1000, int projectileSpeed = 0, int aggroRange = 0, UnitClass unitClass = UnitClass.Generic)
        {
            if (maxHP < 1 || maxHP > 10000000 || armor < 0 || armor > 10000000 || attackDamage < 0 || attackDamage > 10000000
                || attackIntervalTicks < 1 || attackIntervalTicks > 1000000 || attackRange < 1000 || attackRange > 16000
                || projectileSpeed < 0 || projectileSpeed > 40000 || aggroRange < 0 || aggroRange > 20000
                || !Enum.IsDefined(typeof(UnitClass), unitClass) || (projectileSpeed == 0 && attackRange != 1000)) throw new ArgumentException("Invalid combat stats.");
            MaxHP = maxHP; Armor = armor; AttackDamage = attackDamage; AttackIntervalTicks = attackIntervalTicks;
            AttackRange = attackRange; ProjectileSpeed = projectileSpeed; AggroRange = aggroRange; Class = unitClass;
        }
        public static int DamageAgainst(int damage, UnitClass attacker, UnitClass victim, int armor)
        {
            long attack = attacker == UnitClass.Spearman && victim == UnitClass.Cavalry ? (long)damage * 5 / 2 : damage;
            return (int)Math.Max(1, attack - armor);
        }
    }
}
