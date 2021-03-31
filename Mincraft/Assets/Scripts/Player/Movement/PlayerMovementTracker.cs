using System;
using Core.Math;
using Core.Saving;
using Core.UI.Console;
using Extensions;
using UnityEngine;

namespace Core.Player
{
    public class PlayerMovementTracker : SingletonBehaviour<PlayerMovementTracker>
    {
        [Header("References")] 
        [SerializeField] private Transform cameraTransform = null;

        [Space]
        [SerializeField] private float DistanceThreshold = 16.0f;
        public static event Action<Direction> OnDirectionModified;

        //Variablen für Initialisierung in ChunkUpdater
        public int xPlayerPos = 0;
        public int zPlayerPos = 0;
        [Space] private Int3 latestPlayerPosition;
        private Int3 prevLatestPlayerPosition;

        private Int3 deltaResult;

        private void Start()
        {
            if (ResourceIO.LoadCached<PlayerMovementTracker>(new PlayerFileIdentifier(), out OutputContext context))
            {
                var ctx = (PlayerIOContext) context;
                
                this.transform.position = ctx.PlayerPosition;
                this.transform.rotation = ctx.playerRotation;
                this.cameraTransform.localRotation = ctx.cameraRotation;
            }
            
            UpdateLatestPlayerPosition();

            xPlayerPos = latestPlayerPosition.X;
            zPlayerPos = latestPlayerPosition.Z;


            prevLatestPlayerPosition.X = latestPlayerPosition.X;
            prevLatestPlayerPosition.Z = latestPlayerPosition.Z;
        }

        private void Update()
        {
            UpdateLatestPlayerPosition();
            
            deltaResult = latestPlayerPosition - prevLatestPlayerPosition;
            
            if (System.Math.Abs(deltaResult.X) >= DistanceThreshold)
            {
                OnDirectionModified?.Invoke(deltaResult.X > 0 ? Direction.Right : Direction.Left);
                prevLatestPlayerPosition.X = latestPlayerPosition.X;
            }

            if (System.Math.Abs(deltaResult.Z) >= DistanceThreshold)
            {
                OnDirectionModified?.Invoke(deltaResult.Z > 0 ? Direction.Forward : Direction.Back);
                prevLatestPlayerPosition.Z = latestPlayerPosition.Z;
            }
        }

        private void UpdateLatestPlayerPosition()
        {
            latestPlayerPosition.X = Mathf.RoundToInt(transform.position.x);
            latestPlayerPosition.Y = Mathf.RoundToInt(transform.position.y);
            latestPlayerPosition.Z = Mathf.RoundToInt(transform.position.z);
        }

        public string PlayerPos()
        {
            return latestPlayerPosition.ToString();
        }

        [ConsoleMethod(nameof(MovePlayer), "Teleports the player to the given x, y, z location")]
        private void MovePlayer(int x, int y, int z)
        {
            UpdateLatestPlayerPosition();
            transform.position = new Vector3(x, y, z);
            OnDirectionModified?.Invoke(Direction.Teleported);
        }
        
        [ConsoleMethod(nameof(MovePlayerRelative), "Teleports the player relative to the current position")]
        private void MovePlayerRelative(int x, int y, int z)
        {
            UpdateLatestPlayerPosition();
            transform.position += new Vector3(x, y, z);
            OnDirectionModified?.Invoke(Direction.Teleported);
        }
        
        public void OnApplicationQuit()
        {
            bool usedGravity = FirstPersonController.Instance.useGravity;
            var ctx = new PlayerIOContext(transform.position, transform.rotation, cameraTransform.localRotation, usedGravity);
            ResourceIO.Save<PlayerMovementTracker>(ctx);
        }
    }

    public enum Direction : sbyte
    {
        None = 0,
        Left = 1,
        Right = 2,
        Forward = 4,
        Back = 8,
        Teleported = 16
    }
}