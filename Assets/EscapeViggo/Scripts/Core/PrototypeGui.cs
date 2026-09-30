using System;
using UnityEngine;

namespace EscapeViggo.Core
{
    // Shared 1280x720 layout keeps the prototype readable across 16:9 windows.
    public readonly struct PrototypeGui : IDisposable
    {
        private readonly Matrix4x4 _matrix;
        private readonly int _labelSize, _buttonSize, _boxSize;
        public static float Width => Screen.width / Scale;
        public static float Height => Screen.height / Scale;
        private static float Scale => Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        public PrototypeGui(bool begin)
        {
            _matrix = GUI.matrix;
            _labelSize = GUI.skin.label.fontSize;
            _buttonSize = GUI.skin.button.fontSize;
            _boxSize = GUI.skin.box.fontSize;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * Scale);
            GUI.skin.label.fontSize = GUI.skin.button.fontSize = GUI.skin.box.fontSize = 18;
        }
        public void Dispose()
        {
            GUI.matrix = _matrix;
            GUI.skin.label.fontSize = _labelSize;
            GUI.skin.button.fontSize = _buttonSize;
            GUI.skin.box.fontSize = _boxSize;
        }
        public static void Bar(Rect rect, float value, string text, Color color)
        {
            GUI.Box(rect, GUIContent.none);
            var previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 3, (rect.width - 6) * Mathf.Clamp01(value), rect.height - 6), Texture2D.whiteTexture);
            GUI.color = previous;
            GUI.Box(rect, text);
        }
    }
}
