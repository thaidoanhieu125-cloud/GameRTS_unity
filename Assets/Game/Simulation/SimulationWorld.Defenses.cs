using System;
using System.Linq;
using GameRTS.Commands;
namespace GameRTS.Simulation
{
    public sealed partial class SimulationWorld
    {
        public struct MineExplosion { public int BuildingId, Tick, X, Y; }
        private readonly System.Collections.Generic.List<MineExplosion> explosions = new System.Collections.Generic.List<MineExplosion>();
        public System.Collections.Generic.IReadOnlyList<MineExplosion> Explosions => explosions;
        public const int MineDamage=60, MineBlastRadius=1500;
        private void AdvanceDefenses()
        {
            explosions.RemoveAll(e=>Tick-e.Tick>10);
            foreach(var b in buildings.Values)
            {
                if(b.IsDestroyed || !b.IsComplete) continue;
                int x=Map.CenterX(b.Cell)+(b.Width-1)*500, y=Map.CenterY(b.Cell)+(b.Height-1)*500;
                if(b.Definition.IsMine)
                {
                    if(!units.Any(u=>u.IsAlive && u.OwnerId!=b.OwnerId && Map.CellAt(u.X,u.Y)==b.Cell)) continue;
                    explosions.Add(new MineExplosion { BuildingId=b.Id, Tick=Tick, X=x, Y=y });
                    b.HitPoints=0; DestroyStructure(b.Id,b.Cell,b.Width,b.Height,b);
                    foreach(var u in units.Where(u=>u.IsAlive && u.OwnerId!=b.OwnerId).ToArray())
                    {
                        long dx=(long)u.X-x,dy=(long)u.Y-y;
                        if(dx*dx+dy*dy>(long)b.Definition.MineRadius*b.Definition.MineRadius) continue;
                        u.CurrentHP=Math.Max(0,u.CurrentHP-b.Definition.MineDamage); u.LastDamageTick=Tick;
                        if(!u.IsAlive) KillUnit(u);
                    }
                }
                else if(b.Definition.TowerDamage>0 && Tick>=b.NextShotTick)
                {
                    long radius=b.Definition.TowerRange*1000L;
                    var target=units.Where(u=>u.IsAlive && u.OwnerId!=b.OwnerId && IsCellVisible(b.OwnerId,u.Cell))
                        .Select(u=>new { Unit=u, Distance=((long)u.X-x)*(u.X-x)+((long)u.Y-y)*(u.Y-y) })
                        .Where(t=>t.Distance<=radius*radius).OrderBy(t=>t.Distance).ThenBy(t=>t.Unit.Id).FirstOrDefault();
                    if(target==null) continue;
                    if(nextProjectileId==int.MaxValue) throw new InvalidOperationException("Projectile ID exhausted.");
                    var arrow=new ProjectileState(nextProjectileId++,b,target.Unit,x,y,Tick); projectiles.Add(arrow.Id,arrow);
                    b.NextShotTick=Tick+b.Definition.TowerInterval;
                }
            }
        }
        private void HashDefenses(Action<int> add)
        {
            add(explosions.Count); foreach(var e in explosions) {add(e.BuildingId);add(e.Tick);add(e.X);add(e.Y);}
            foreach(var d in buildingDefinitions.Values)
            {add(d.SightRadius);add(d.TowerDamage);add(d.TowerInterval);add(d.TowerRange);add(d.IsMine?1:0);add(d.MineDamage);add(d.MineRadius);}
            foreach(var b in buildings.Values) add(b.NextShotTick);
        }
    }
}
