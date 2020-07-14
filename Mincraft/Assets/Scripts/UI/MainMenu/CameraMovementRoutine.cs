using System.Collections;
using UnityEngine;

namespace Core.UI.MainMenu
{
    public class CameraMovementRoutine : MonoBehaviour
    {
        [SerializeField] private CameraRoutineInformation[] _cameraRoutineInformation = null;
        
        private WaitForEndOfFrame waiter = new WaitForEndOfFrame();
        
        //Gets called from OnClick- Unity
        public void SetCameraRoutine(int routine)
        {
            StopAllCoroutines();
            StartCoroutine(nameof(MoveCamera), routine);
        }

        private IEnumerator MoveCamera(int routine)
        {
            Vector3 targetPos = _cameraRoutineInformation[routine].Position;
            Vector3 targetRot = _cameraRoutineInformation[routine].Rotation;
            float percentTime = 0.0f;
            
            
            while ((targetPos - transform.position).sqrMagnitude > 0.005f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, percentTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRot), percentTime);
                percentTime = (percentTime + Time.deltaTime);
                yield return waiter;
            }
        }
    }

    [System.Serializable]
    public struct CameraRoutineInformation
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }
}
