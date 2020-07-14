using System.Collections;
using UnityEngine;

namespace Core.UI.MainMenu
{
    public class CameraMovementRoutine : MonoBehaviour
    {
        [SerializeField] private CameraRoutineInformation[] _cameraRoutineInformation = null;
        
        private WaitForEndOfFrame waiter = new WaitForEndOfFrame();

        private float yRot = 0f;
        private bool animating = false;
        
        private void Start()
        {
            yRot = transform.rotation.eulerAngles.y;
        }

        private void Update()
        {
            if (!animating)
            {
                transform.rotation = Quaternion.Euler(
                    transform.rotation.eulerAngles.x, 
                    yRot + Mathf.Sin(Time.time),
                    transform.rotation.eulerAngles.z);
            }
        }
        
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
                animating = true;
                transform.position = Vector3.Lerp(transform.position, targetPos, percentTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(targetRot), percentTime);
                percentTime = (percentTime + Time.deltaTime);
                yield return waiter;
            }

            yRot = transform.rotation.eulerAngles.y;
            animating = false;
        }
    }

    [System.Serializable]
    public struct CameraRoutineInformation
    {
        public Vector3 Position;
        public Vector3 Rotation;
    }
}
