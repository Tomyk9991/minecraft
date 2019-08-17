using Unity.Collections;
using UnityEngine;

namespace Core.Performance.Occlusion
{
    public class OcclusionManager : MonoBehaviour
    {
        [SerializeField] private int rayAmount = 1500;
        [SerializeField] private int rayDistance = 300;
        [SerializeField] private float stayTime = 2f;

        private Camera cam;

        [HideInInspector] public Vector2[] rPoints;

        private void Start()
        {
            cam = Camera.main;
            rPoints = new Vector2 [rayAmount];
            GetPoints();
        }

        private void Update()
        {
    //        CastRays();
            CastRaysJob();
        }

        private void GetPoints()
        {
            float x = 0f, y = 0f;
            float temp = 1f / Mathf.Sqrt(rayAmount);

            for (int i = 0; i < rayAmount; i++)
            {
                if (x > 1)
                {
                    x = 0;
                    y += temp;
                }

                rPoints[i] = new Vector2(x, y);
                x += temp;
            }
        }

        private void CastRays()
        {
            for (int i = 0; i < rayAmount; i++)
            {
                Ray ray = cam.ViewportPointToRay(new Vector3(rPoints[i].x, rPoints[i].y, 0f));
                OcclusionObject obj;

                if (Physics.Raycast(ray, out RaycastHit hit, rayDistance))
                {
                    if (obj = hit.transform.GetComponent<OcclusionObject>())
                    {
                        obj.HitOcclude(stayTime);
                    }
                }
            }
        }

        private void CastRaysJob()
        {
            NativeArray<RaycastHit> results = new NativeArray<RaycastHit>(rayAmount, Allocator.TempJob);
            NativeArray<RaycastCommand> commands = new NativeArray<RaycastCommand>(rayAmount, Allocator.TempJob);

            for (int i = 0; i < rayAmount; i++)
            {
                Ray r = cam.ViewportPointToRay(new Vector3(rPoints[i].x, rPoints[i].y, 0f));
                commands[i] = new RaycastCommand(r.origin, r.direction, rayDistance);
            }

            var handle = RaycastCommand.ScheduleBatch(commands, results, 1);
            handle.Complete();

            for (int i = 0; i < results.Length; i++)
            {
                RaycastHit batchedHit = results[i];
                if (batchedHit.collider == null)
                    continue;

                batchedHit.transform.GetComponent<OcclusionObject>()?.HitOcclude(stayTime);
            }
            
            
            
            commands.Dispose();
            results.Dispose();
        }        
    }
}
