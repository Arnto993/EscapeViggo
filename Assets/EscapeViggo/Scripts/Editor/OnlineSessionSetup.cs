using EscapeViggo.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class OnlineSessionSetup
    {
        [MenuItem("Escape Viggo/Setup/Apply M17 Online Sessions")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var local = Object.FindFirstObjectByType<LocalNetworkSession>();
            if (local.GetComponent<OnlineNetworkSession>() == null) local.gameObject.AddComponent<OnlineNetworkSession>();
            EditorSceneManager.SaveScene(scene);
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M17 Online Sessions")]
        public static void Validate()
        {
            PrototypeUiSetup.ValidatePlaytest();
            var local = Object.FindFirstObjectByType<LocalNetworkSession>();
            WorksiteValidation.Require(local.GetComponent<OnlineNetworkSession>() != null, "Missing online session connection component.");
            WorksiteValidation.Require(local.Capacity >= 8, "Online play requires space for Viggo and seven electricians.");
            Debug.Log("M17 ASSET VALIDATION PASSED (cloud access/connectivity requires runtime verification).");
        }
    }
}
