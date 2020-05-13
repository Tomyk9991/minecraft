using System.Text;
using Core.Chunks;
using Core.Chunks.Threading;
using Core.Math;
using Core.Player;
using UnityEngine;

namespace Testing
{
    public class CreateTreeOnClickTest : MonoBehaviour
    {
        private Int2 playerPos = Int2.Zero;
        private Vector3 chunkPos = Vector3.zero;

        private int drawDistance;
        private const int CHUNKSIZE = 0x10;

        //private MeshJobManager _meshJobManager;
        private JobManager _jobManager;

        private void Start()
        {
            PlayerMovementTracker.OnChunkPositionChanged += (x, y) =>
            {
                playerPos.X = x;
                playerPos.Y = y;
            };

            _jobManager = JobManager.JobManagerUpdaterInstance;
            //_meshJobManager = MeshJobManager.MeshJobManagerUpdaterInstance;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                StringBuilder builder = new StringBuilder("{");
                ChunkColumn column = ChunkBuffer.GetChunkColumn(0, 0);
                for (int i = 0; i < column.chunks.Length; i++)
                {
                    builder.Append(i == column.chunks.Length - 1 ? column[i].GlobalPosition.ToString() : column[i].GlobalPosition + ", ");
                }

                builder.Append("}");

                Debug.Log(builder.ToString());
            }
        }
    }
}