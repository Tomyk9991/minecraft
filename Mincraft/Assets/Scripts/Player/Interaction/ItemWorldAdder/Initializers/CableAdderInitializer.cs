using UnityEngine;

namespace Player.Interaction.ItemWorldAdder.Initializers
{
    [CreateAssetMenu(fileName = "CableAdderInitializer", menuName = "Scriptable Objects/CableAdderInitializer")]
    public class CableAdderInitializer : ScriptableObject
    {
        public GameObject LineRendererPrefab;
    }
}