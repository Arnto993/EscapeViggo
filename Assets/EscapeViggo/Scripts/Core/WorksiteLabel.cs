using UnityEngine;

namespace EscapeViggo.Core
{
    /// <summary>Keeps greybox markers readable when approaching from either route.</summary>
    public sealed class WorksiteLabel : MonoBehaviour
    {
        private Camera _camera;
        private float _nextLookup;

        private void LateUpdate()
        {
            if (_camera == null || !_camera.isActiveAndEnabled)
            {
                if (Time.unscaledTime < _nextLookup) return;
                _camera = Camera.main;
                _nextLookup = Time.unscaledTime + 0.5f;
            }
            if (_camera != null)
                transform.rotation = Quaternion.Euler(0, _camera.transform.eulerAngles.y, 0);
        }
    }
}
