using System.Collections.Generic;
using Extensions;
using UnityEngine;

namespace Core.Player.Interaction
{
    public class DroppedItemsManager : SingletonBehaviour<DroppedItemsManager>
    {
        [Header("Animation")]
        [SerializeField] private float movementSpeed = 2.0f;
        [SerializeField] private float rotationSpeed = 5.0f;
        
        
        [Header("Attraction properties")]
        [SerializeField] private float maxDistanceBeforeMerge = 10.0f;

        private List<GameObject> droppedItems = new List<GameObject>();
        private List<Transform> childTransforms = new List<Transform>(); //For performance reasons, storing references to the child-transforms
        private Vector3 floatingPosition = Vector3.zero;

        private void Update()
        {
            AnimateDroppedItems();
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

        public void Add(GameObject go)
        {
            droppedItems.Add(go);
            childTransforms.Add(go.transform.GetChild(0).transform);
        }
    }
}
