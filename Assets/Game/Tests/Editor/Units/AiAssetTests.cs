using System.Linq;
using GameRTS.Data;
using GameRTS.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameRTS.Tests
{
    public sealed class AiAssetTests
    {
        [Test]
        public void StandardAiProfile()
        {
            var profile = AssetDatabase.LoadAssetAtPath<AiProfileSO>("Assets/Game/Data/AiStandard.asset");
            Assert.That(profile, Is.Not.Null); var config = profile.CreateSettings();
            Assert.That(config.DecisionInterval, Is.EqualTo(30)); Assert.That(config.VillagerTarget, Is.EqualTo(10));
            Assert.That(config.MilitaryVillagers, Is.EqualTo(8)); Assert.That(config.WaveSize, Is.EqualTo(6));
        }
        [Test]
        public void AiSceneAndNavigation()
        {
            Assert.That(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == "Assets/Game/Scenes/AiScene.unity"), Is.True);
            var hud = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/UI/Prefabs/EconomyHud.prefab");
            Assert.That(hud.transform.Find("AI Match").GetComponent<Button>(), Is.Not.Null);
            var serialized = new SerializedObject(hud.GetComponent<SceneSwitchView>());
            Assert.That(serialized.FindProperty("aiButton").objectReferenceValue, Is.Not.Null);
        }
    }
}
