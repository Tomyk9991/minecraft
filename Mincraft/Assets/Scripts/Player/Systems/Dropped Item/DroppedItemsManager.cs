using System;
using System.Collections.Generic;
using Core.Builder;
using Core.UI.Console;
using Extensions;
using UnityEngine;
using Utilities;

namespace Core.Player.Interaction
{
    public class DroppedItemsManager : SingletonBehaviour<DroppedItemsManager>
    {
        [Header("Animation")] 
        [SerializeField] private float movementSpeed = 2.0f;
        [SerializeField] private float rotationSpeed = 5.0f;

        [Header("References")] 
        [SerializeField] private Transform droppedItemsParent = null;
        [SerializeField] private GameObject littleBlockPrefab = null;
        
        private List<GameObject> droppedItems = new List<GameObject>();
        //For performance reasons, storing references to the child-transforms
        private List<Transform> childTransforms = new List<Transform>();
        private Pool<GameObject> droppedBlocksPool;
        private List<BoxColliderEnabler> boxColliderEnablers = new List<BoxColliderEnabler>();

        private Vector3 floatingPosition = Vector3.zero;
        private Transform playerPosition;

        private void Start()
        {
            droppedBlocksPool = new Pool<GameObject>(50,
                () =>
                {
                    GameObject gameObject = Instantiate(littleBlockPrefab, Vector3.zero, Quaternion.identity,
                        droppedItemsParent);

                    gameObject.SetActive(false);

                    return gameObject;
                });

            playerPosition = PlayerMovementTracker.Instance.transform;
        }

        private void Update()
        {
            AnimateDroppedItems();
            HandleDisabledBoxColliders();
        }

        private void HandleDisabledBoxColliders()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < boxColliderEnablers.Count; i++)
            {
                if (boxColliderEnablers[i].Timer.TimeElapsed(deltaTime))
                {
                    boxColliderEnablers[i].Collider.enabled = true;
                    boxColliderEnablers.RemoveAt(i);
                }
            }
        }

        private void AnimateDroppedItems()
        {
            float t = Mathf.Sin(Time.time * movementSpeed);
            floatingPosition.y = (t + 1) / 2.0f;

            float yRot = Time.deltaTime * rotationSpeed;

            foreach (Transform trans in childTransforms)
            {
                trans.localPosition = floatingPosition;
                trans.Rotate(0f, yRot, 0f);
            }
        }

        public void AddBoxColliderHandle(BoxCollider boxCollider)
        {
            boxCollider.enabled = false;
            boxColliderEnablers.Add(new BoxColliderEnabler(boxCollider));
        }

        public void AddNewItem(GameObject go)
        {
            droppedItems.Add(go);
            childTransforms.Add(go.transform.GetChild(0).transform);
        }

        public GameObject GetNextBlock()
        {
            GameObject go = droppedBlocksPool.GetNext();
            go.SetActive(true);

            return go;
        }

        public void AddToPool(GameObject t)
        {
            t.SetActive(false);
            droppedBlocksPool.Add(t);
        }
        
        private class BoxColliderEnabler
        {
            public Timer Timer;
            public BoxCollider Collider;

            public BoxColliderEnabler(BoxCollider collider)
            {
                Timer = new Timer(0.5f);
                Collider = collider;
            }
        }

        [ConsoleMethod(nameof(SpawnItem), "Spawns an item with the given id")]
        private void SpawnItem(int itemID, int amount)
        {
            if (itemID <= 0 || itemID >= Enum.GetValues(typeof(BlockUV)).Length || amount <= 0)
            {
                ConsoleInputer.WriteToOutput("not valid arguments");
                return;
            }
            
            ConsoleInputer.WriteToOutput(amount == 1
                ? $"Spawning {amount} {(BlockUV) itemID}block"
                : $"Spawning {amount} {(BlockUV) itemID}blocks");
            
            for (int i = 0; i < amount; i++)
            {
                GameObject go = GetNextBlock();
                    
                go.transform.position = playerPosition.position + playerPosition.forward;
                go.GetComponent<DroppedItemInformation>().FromBlock(new Block((BlockUV) itemID), amount);
                go.GetComponent<Rigidbody>().AddForce(playerPosition.forward, ForceMode.Impulse);
                
                AddNewItem(go);
                AddBoxColliderHandle(go.transform.GetChild(0).GetComponent<BoxCollider>());
            }
        }
    }
}