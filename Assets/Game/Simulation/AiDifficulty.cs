using System;
namespace GameRTS.Simulation
{
    public enum AiDifficulty { Easy, Medium, Hard }
    public static class AiDifficulties
    {
        public static AiSettings Create(AiDifficulty difficulty)
        {
            switch (difficulty)
            {
                case AiDifficulty.Easy: return new AiSettings(40, 8, 8, 6, 12, 10, 400, 1200);
                case AiDifficulty.Medium: return new AiSettings(30, 10, 8, 11, 22, 10, 200, 400);
                case AiDifficulty.Hard: return new AiSettings(20, 12, 5, 15, 30, 10, 100, 0, true, true);
                default: throw new ArgumentOutOfRangeException(nameof(difficulty));
            }
        }
    }
}
