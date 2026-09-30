using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class NetworkPackageCheck
    {
        private static SearchRequest _request;
        private static int _index;
        private static readonly string[] Packages = { "com.unity.netcode.gameobjects", "com.unity.transport", "com.unity.services.multiplayer" };

        public static void Query()
        {
            _index = 0;
            _request = Client.Search(Packages[_index]);
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!_request.IsCompleted) return;
            if (_request.Status == StatusCode.Failure)
            {
                Debug.LogError(_request.Error.message);
                EditorApplication.update -= Poll;
                if (Application.isBatchMode) EditorApplication.Exit(1);
                return;
            }
            foreach (var package in _request.Result)
                Debug.Log($"PACKAGE COMPATIBILITY {package.name}: recommended={package.versions.recommended}; latestCompatible={package.versions.latestCompatible}; compatible={string.Join(",", package.versions.compatible)}");
            if (++_index < Packages.Length) _request = Client.Search(Packages[_index]);
            else
            {
                EditorApplication.update -= Poll;
                if (Application.isBatchMode) EditorApplication.Exit(0);
            }
        }
    }
}
