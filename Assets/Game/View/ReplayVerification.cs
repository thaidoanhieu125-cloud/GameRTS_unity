using System;
using System.IO;
using System.Text;
using GameRTS.Simulation;
using UnityEngine;

namespace GameRTS.View
{
    public static class ReplayVerification
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RunWhenRequested()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-rtsReplayCheck");
            if (index < 0) return;
            try
            {
                if (index + 1 >= args.Length) throw new ArgumentException("Missing replay output path.");
                File.WriteAllText(args[index + 1], DeterminismScenario.Run(), new UTF8Encoding(false));
                Debug.Log("RTS replay verification finished: movement, gathering, construction and worker recovery; 600 checksum sets.");
                Application.Quit(0);
            }
            catch (Exception e) { Debug.LogException(e); Application.Quit(1); }
        }
    }
}
