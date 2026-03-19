using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SledSurfers.Util
{
    
    [RequireComponent(typeof(Camera))]
    public class CameraStackHandler : MonoBehaviour
    {
        [SerializeField] private Camera _uiCamera;
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                Debug.LogError("[CameraStackHandler] Main camera not found!");
                return;
            }

            AddToStack();
        }

        private void OnDestroy()
        {
            RemoveFromStack();
        }

        private void AddToStack()
        {
            var cameraData = _mainCamera.GetUniversalAdditionalCameraData();

            if (!cameraData.cameraStack.Contains(_uiCamera))
            {
                cameraData.cameraStack.Add(_uiCamera);
                Debug.Log("[CameraStackHandler] UI Camera added to stack.");
            }
        }

        private void RemoveFromStack()
        {
            if (_mainCamera == null) return;

            var cameraData = _mainCamera.GetUniversalAdditionalCameraData();

            if (cameraData.cameraStack.Contains(_uiCamera))
            {
                cameraData.cameraStack.Remove(_uiCamera);
                Debug.Log("[CameraStackHandler] UI Camera removed from stack.");
            }
        }
    }
}