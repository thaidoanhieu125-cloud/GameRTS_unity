using System.Collections;
using GameRTS.Simulation;
using GameRTS.UI;
using GameRTS.View;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace GameRTS.Tests
{
    public sealed class DifficultyViewTests
    {
        [UnityTest]
        public IEnumerator ChooseEveryLevel()
        {
            foreach(var level in new[]{AiDifficulty.Easy,AiDifficulty.Medium,AiDifficulty.Hard})
            {
                yield return SceneManager.LoadSceneAsync("GameScene"); yield return null;
                var hud=Object.FindFirstObjectByType<EconomyHudView>();
                hud.transform.Find("AI Match").GetComponent<Button>().onClick.Invoke(); yield return null;
                var menu=Object.FindFirstObjectByType<DifficultyMenuView>(); Assert.That(DifficultyMenuView.IsOpen,Is.True);
                menu.transform.Find("Difficulty Menu/"+level).GetComponent<Button>().onClick.Invoke();
                Assert.That(menu.Selected,Is.EqualTo(level));
                menu.transform.Find("Difficulty Menu/Start").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                var w=Object.FindFirstObjectByType<RtsGameView>().World;
                Assert.That(w.GetAi(2).Settings.WaveSize,Is.EqualTo(AiDifficulties.Create(level).WaveSize));
                Assert.That(w.GetPlayerEconomy(1).Population,Is.EqualTo(3));
                Assert.That(DifficultyMenuView.IsOpen,Is.False);
            }
            yield return SceneManager.LoadSceneAsync("GameScene"); yield return null;
        }
    }
}
