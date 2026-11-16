using System;
using System.Collections.Generic;
using System.Linq;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        private readonly SortedDictionary<TechnologyId, ResearchDefinition> technologies = new SortedDictionary<TechnologyId, ResearchDefinition>();
        private readonly SortedDictionary<int, SortedSet<TechnologyId>> completedTechnologies = new SortedDictionary<int, SortedSet<TechnologyId>>();
        private readonly SortedDictionary<int, ResearchJob> researchJobs = new SortedDictionary<int, ResearchJob>();
        public bool TechnologiesEnabled { get; private set; }
        public IEnumerable<ResearchDefinition> Technologies => technologies.Values;
        public ResearchIssue LastResearchIssue { get; private set; }
        public int LastResearchPlayer { get; private set; }
        public int LastResearchSequence { get; private set; } = -1;

        public void ConfigureTechnologies(IEnumerable<ResearchDefinition> definitions)
        {
            if (Tick != 0 || TechnologiesEnabled || definitions == null) throw new InvalidOperationException("Configure technologies once before tick 1.");
            var copy = definitions.ToArray();
            if (copy.Any(d => d == null) || copy.Select(d => d.Id).Distinct().Count() != copy.Length)
                throw new ArgumentException("Duplicate or missing technology.");
            foreach (var d in copy) technologies.Add(d.Id, d);
            TechnologiesEnabled = true;
        }
        public bool HasTechnology(int player, TechnologyId id) => completedTechnologies.TryGetValue(player, out var done) && done.Contains(id);
        public int GetAge(int player) => HasTechnology(player, TechnologyId.BronzeAge) ? 2 : 1;
        public ResearchJob GetResearch(int buildingId) => researchJobs.TryGetValue(buildingId, out var job) ? job : null;
        public bool CanBuildInAge(int player, string type) => !TechnologiesEnabled || GetAge(player) >= 2 || type != "stable" && type != "watch_tower";

        public string ResearchBuildingType(int id, out int owner)
        {
            owner = 0;
            var b = GetBuilding(id);
            if (b != null)
            {
                if (b.IsDestroyed || !b.IsComplete) return null;
                owner = b.OwnerId; return b.Definition.TypeId;
            }
            var depot = GetDropOff(id);
            if (depot == null || depot.IsDestroyed) return null;
            owner = depot.OwnerId; return "town_center";
        }
        public ResearchIssue CheckResearch(int player, int buildingId, TechnologyId id)
        {
            if (!TechnologiesEnabled || !technologies.TryGetValue(id, out var d)) return ResearchIssue.Disabled;
            string type = ResearchBuildingType(buildingId, out int owner);
            if (type == null || type != d.BuildingType) return ResearchIssue.InvalidBuilding;
            if (owner != player) return ResearchIssue.NotOwner;
            if (HasTechnology(player, id)) return ResearchIssue.AlreadyDone;
            if (GetAge(player) < d.RequiredAge) return ResearchIssue.RequiresAge;
            if (researchJobs.ContainsKey(buildingId) || researchJobs.Values.Any(j => j.PlayerId == player && j.Definition.Id == id)
                || GetTraining(buildingId)?.Queue.Count > 0) return ResearchIssue.Busy;
            var wallet = GetPlayerEconomy(player);
            return wallet == null || wallet.Wood < d.Wood || wallet.Food < d.Food || wallet.Gold < d.Gold
                ? ResearchIssue.InsufficientResources : ResearchIssue.None;
        }
        private void Apply(ResearchCommand c)
        {
            LastResearchPlayer = c.PlayerId; LastResearchSequence = c.Sequence; LastResearchIssue = ResearchIssue.None;
            if (c.Operation == ResearchOperation.Cancel)
            {
                var job = GetResearch(c.BuildingId);
                if (job == null || job.Definition.Id != c.Technology || ResearchBuildingType(c.BuildingId, out int owner) == null)
                { LastResearchIssue = ResearchIssue.InvalidJob; return; }
                if (owner != c.PlayerId) { LastResearchIssue = ResearchIssue.NotOwner; return; }
                if (!GetPlayerEconomy(owner).TryAddResources(job.Definition.Wood, job.Definition.Food, job.Definition.Gold))
                { LastResearchIssue = ResearchIssue.RefundOverflow; return; }
                researchJobs.Remove(c.BuildingId); return;
            }
            LastResearchIssue = CheckResearch(c.PlayerId, c.BuildingId, c.Technology);
            if (LastResearchIssue != ResearchIssue.None) return;
            var d = technologies[c.Technology];
            if (!GetPlayerEconomy(c.PlayerId).TrySpendResources(d.Wood, d.Food, d.Gold))
            { LastResearchIssue = ResearchIssue.InsufficientResources; return; }
            researchJobs.Add(c.BuildingId, new ResearchJob(c.BuildingId, c.PlayerId, d));
        }
        private void AdvanceResearch()
        {
            foreach (var pair in researchJobs.ToArray())
            {
                var job = pair.Value;
                if (ResearchBuildingType(job.BuildingId, out int owner) == null || owner != job.PlayerId)
                { researchJobs.Remove(pair.Key); continue; } // Destruction loses the paid research.
                if (++job.Progress < job.Definition.Duration) continue;
                if (!completedTechnologies.TryGetValue(owner, out var done)) completedTechnologies.Add(owner, done = new SortedSet<TechnologyId>());
                done.Add(job.Definition.Id); researchJobs.Remove(pair.Key);
                foreach (var unit in units.Where(u => u.OwnerId == owner)) ApplyUpgrades(unit);
            }
        }
        private void ApplyUpgrades(UnitState unit)
        {
            var s = unit.BaseStats;
            bool arrow = s.Class == UnitClass.Archer && HasTechnology(unit.OwnerId, TechnologyId.Fletching);
            bool armor = (s.Class == UnitClass.Spearman || s.Class == UnitClass.Cavalry) && HasTechnology(unit.OwnerId, TechnologyId.ScaleArmor);
            unit.Stats = new CombatStats(s.MaxHP, Math.Min(10000000, s.Armor + (armor ? 1 : 0)), Math.Min(10000000, s.AttackDamage + (arrow ? 1 : 0)),
                s.AttackIntervalTicks, Math.Min(16000, s.AttackRange + (arrow ? 1000 : 0)), s.ProjectileSpeed, s.AggroRange, s.Class);
            if (unit.Villager != null) unit.Villager.Capacity = unit.Villager.BaseCapacity + (HasTechnology(unit.OwnerId, TechnologyId.LeatherBag) ? 5 : 0);
        }
        private void HashResearch(Action<int> add)
        {
            add(TechnologiesEnabled ? 1 : 0); add(technologies.Count);
            add(LastResearchPlayer); add(LastResearchSequence); add((int)LastResearchIssue);
            foreach (var d in technologies.Values)
            { add((int)d.Id); add(d.BuildingType.Length); foreach (char ch in d.BuildingType) add(ch); add(d.RequiredAge); add(d.Wood); add(d.Food); add(d.Gold); add(d.Duration); }
            add(completedTechnologies.Count);
            foreach (var pair in completedTechnologies)
            { add(pair.Key); add(pair.Value.Count); foreach (var id in pair.Value) add((int)id); }
            foreach (var u in units)
            {
                var s = u.BaseStats;
                add(s.MaxHP); add(s.Armor); add(s.AttackDamage); add(s.AttackIntervalTicks); add(s.AttackRange); add(s.ProjectileSpeed); add(s.AggroRange); add((int)s.Class);
            }
            add(researchJobs.Count);
            foreach (var j in researchJobs.Values) { add(j.BuildingId); add(j.PlayerId); add((int)j.Definition.Id); add(j.Progress); }
        }
    }
}
