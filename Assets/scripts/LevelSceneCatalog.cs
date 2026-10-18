using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelSceneCatalog
{
    private const string LevelPrefix = "level_";

    public static List<string> GetLevelScenes()
    {
        var scenesByNumber = new SortedDictionary<int, string>();

        for (int buildIndex = 0; buildIndex < SceneManager.sceneCountInBuildSettings; buildIndex++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(buildIndex);
            string sceneName = GetSceneName(scenePath);

            if (!sceneName.StartsWith(LevelPrefix))
                continue;

            string numberPart = sceneName.Substring(LevelPrefix.Length);
            if (int.TryParse(numberPart, out int levelNumber) && levelNumber > 0)
                scenesByNumber[levelNumber] = sceneName;
        }

        return new List<string>(scenesByNumber.Values);
    }

    public static string GetTutorialScene(string levelSceneName)
    {
        string numberPart = levelSceneName.StartsWith(LevelPrefix)
            ? levelSceneName.Substring(LevelPrefix.Length)
            : string.Empty;

        string tutorialScene = $"tutorial_level_{numberPart}";
        if (Application.CanStreamedLevelBeLoaded(tutorialScene))
            return tutorialScene;

        // Some existing scenes use the shorter name, for example tutorial_21.
        tutorialScene = $"tutorial_{numberPart}";
        return Application.CanStreamedLevelBeLoaded(tutorialScene) ? tutorialScene : null;
    }

    private static string GetSceneName(string scenePath)
    {
        int slashIndex = scenePath.LastIndexOf('/');
        int nameStart = slashIndex >= 0 ? slashIndex + 1 : 0;
        int extensionIndex = scenePath.LastIndexOf('.');
        int nameLength = extensionIndex > nameStart
            ? extensionIndex - nameStart
            : scenePath.Length - nameStart;

        return scenePath.Substring(nameStart, nameLength);
    }
}
