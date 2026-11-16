using System.Linq;
using GameRTS.Commands;

namespace GameRTS.Simulation
{
    public sealed partial class AiController
    {
        private bool AssignHunters(SimulationWorld world, System.Collections.Generic.List<UnitState> workers)
        {
            if (!world.TechnologiesEnabled || workers.Count < 5 || world.GetPlayerEconomy(State.PlayerId).Food >= 300
                || workers.Any(u => u.Villager.HuntAnimalId != 0)) return false;
            var town = world.GetDropOff(State.TownCenterId);
            var animal = world.Animals.Where(u => u.IsAlive && u.Animal.Definition.Kind == AnimalKind.Deer
                    && world.IsCellVisible(State.PlayerId, u.Cell) && Distance(world.Map, town.Cell, u.Cell) <= 144)
                .OrderBy(u => Distance(world.Map, town.Cell, u.Cell)).ThenBy(u => u.Id).FirstOrDefault();
            if (animal == null) return false;
            var hunters = workers.Where(u => u.Villager.BuildingId == 0 && u.TargetId == 0)
                .OrderBy(u => Distance(world.Map, u.Cell, animal.Cell)).ThenBy(u => u.Id).Take(2).Select(u => u.Id).ToArray();
            return hunters.Length == 2 && Send(world, seq => new HuntCommand(world.Tick + 2, State.PlayerId, seq, hunters, animal.Id));
        }
    }
}
