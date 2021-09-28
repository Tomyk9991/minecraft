using Core.Player.Interaction;
using Core.Player.Interaction.ItemWorldAdder;
using Player.Interaction.ItemWorldAdder;
using UnityEngine;

public class HandAnimationTrigger : MonoBehaviour
{
    [SerializeField] private Animator animationController = null;

    private readonly int triggerHash = Animator.StringToHash("Place");

    private void Start()
    {
        Adder.OnPlace += () => animationController.SetTrigger(triggerHash);
        RemoveBlock.OnRemove += () => animationController.SetTrigger(triggerHash);
    }
}
