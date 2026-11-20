using System;
using System.Collections.Generic;

namespace GameRTS.Simulation
{
    // Hand-drawn north/south layout. Integer world-cell data shared by Simulation and View.
    public static class MeadowMapLayout
    {
        public const int Width = 128, Height = 192, MinimumX = -64, MinimumY = -32;
        public const int MiddleY = 64;
        public readonly struct Lake
        {
            public readonly int X, Y, RadiusX, RadiusY;
            public Lake(int x, int y, int rx, int ry) { X = x; Y = y; RadiusX = rx; RadiusY = ry; }
        }
        public readonly struct Cell
        {
            public readonly int X, Y;
            public Cell(int x, int y) { X = x; Y = y; }
        }
        public readonly struct Resource
        {
            public readonly Cell Position;
            public readonly ResourceType Type;
            public Resource(int x, int y, ResourceType type) { Position = new Cell(x,y); Type = type; }
        }
        private static readonly Lake[] lakes = {
            new Lake(-47,-5,8,9), new Lake(47,-5,8,9),
            new Lake(0,34,11,5), new Lake(0,94,11,5),
            new Lake(-47,133,8,9), new Lake(47,133,8,9)
        };
        public static int LakeCount => lakes.Length;
        public static Lake GetLake(int index) => lakes[index];
        private static readonly Cell[] rocks = BuildRocks();
        private static readonly HashSet<int> rockKeys = BuildRockKeys();
        public static IReadOnlyList<Cell> Rocks => rocks;
        private static int Key(int x, int y) => (y - MinimumY) * Width + x - MinimumX;
        private static int Hash(int x, int y) => unchecked((x * 73856093) ^ (y * 19349663)) & int.MaxValue;
        private static bool Protected(int x, int y) => Math.Abs(x) < 20 && (y < 17 || y > 111);
        public static bool IsPortraitMap(GridMap map) => map.Width == Width && map.Height == Height
            && map.OriginX == MinimumX*1000 && map.OriginY == MinimumY*1000;
        public static bool IsWaterCell(int x, int y)
        {
            foreach (Lake lake in lakes)
            {
                // Closest tile point plus half-cell unit clearance, using doubled integers.
                long dx = Math.Max(0, Math.Abs(2 * x + 1 - 2 * lake.X) - 2);
                long dy = Math.Max(0, Math.Abs(2 * y + 1 - 2 * lake.Y) - 2);
                long rx = 2 * lake.RadiusX, ry = 2 * lake.RadiusY;
                if (dx * dx * ry * ry + dy * dy * rx * rx <= rx * rx * ry * ry) return true;
            }
            return false;
        }
        public static bool IsRockCell(int x, int y) => x >= MinimumX && y >= MinimumY
            && x < MinimumX + Width && y < MinimumY + Height && rockKeys.Contains(Key(x,y));
        public static bool BlocksCell(int x, int y) => IsWaterCell(x,y) || IsRockCell(x,y);
        private static bool NearWater(int x,int y)
        {
            for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if(IsWaterCell(x+dx,y+dy))return true;
            return false;
        }
        public static IEnumerable<Cell> Trees()
        {
            // Two winding forest belts beside the contested resource field, plus base groves.
            int[,] groves = { {-25,-13,6,7}, {25,-13,6,7}, {-24,6,5,6}, {24,6,5,6},
                {-25,141,6,7}, {25,141,6,7}, {-24,122,5,6}, {24,122,5,6},
                {-17,20,5,4}, {17,108,5,4}, {17,20,5,4}, {-17,108,5,4} };
            for(int y=MinimumY+4;y<MinimumY+Height-4;y+=2)
            for(int x=MinimumX+4;x<MinimumX+Width-4;x+=2)
            {
                if(Protected(x,y) || BlocksCell(x,y) || NearWater(x,y))continue;
                int bend = (Math.Abs(y-MiddleY)/8)%3-1;
                bool belt = y>=40 && y<=88 && Math.Abs(Math.Abs(x)-(36+bend*2))<=5;
                bool grove=false;
                for(int i=0;i<groves.GetLength(0);i++)
                {
                    int dx=x-groves[i,0],dy=y-groves[i,1],rx=groves[i,2],ry=groves[i,3];
                    if(dx*dx*ry*ry+dy*dy*rx*rx<=rx*rx*ry*ry) { grove=true;break; }
                }
                if((belt || grove) && Hash(x,y)%11!=0)yield return new Cell(x,y);
                else if(y>=54 && y<=74 && Math.Abs(x)>=18 && Math.Abs(x)<=24 && Hash(x,y)%3==0)
                    yield return new Cell(x,y);
            }
        }
        public static IEnumerable<Resource> CentralDeposits()
        {
            for(int y=56;y<=72;y+=4)
            for(int x=-10;x<=10;x+=4)
                yield return new Resource(x,y,Math.Abs(x)>=6?ResourceType.Gold:ResourceType.Food);
        }
        private static Cell[] BuildRocks()
        {
            var result=new List<Cell>();
            for(int y=MinimumY+3;y<MinimumY+Height-3;y++)for(int x=MinimumX+3;x<MinimumX+Width-3;x++)
            {
                // Four solid low mountain clusters; no isolated walkable pockets inside them.
                int dx=Math.Abs(x)-35,dy=Math.Min(Math.Abs(y-22),Math.Abs(y-106));
                if(dx*dx*25+dy*dy*121<=121*25) result.Add(new Cell(x,y));
            }
            return result.ToArray();
        }
        private static HashSet<int> BuildRockKeys()
        { var keys=new HashSet<int>();foreach(var c in rocks)keys.Add(Key(c.X,c.Y));return keys; }
        public static GridMap CreateMap()
        {
            var map=new GridMap(Width,Height,MinimumX*1000,MinimumY*1000);
            for(int y=0;y<Height;y++)for(int x=0;x<Width;x++)map.SetWalkable(x,y,!BlocksCell(x+MinimumX,y+MinimumY));
            return map;
        }
    }
}
