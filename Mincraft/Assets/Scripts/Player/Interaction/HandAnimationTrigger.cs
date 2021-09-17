using Core.Player.Interaction;
using UnityEngine;

public class HandAnimationTrigger : MonoBehaviour
{
    [SerializeField] private Animator animationController = null;

    private readonly int triggerHash = Animator.StringToHash("Place");

    private void Start()
    {
        AddBlock.OnAdd += () => animationController.SetTrigger(triggerHash);
        RemoveBlock.OnRemove += () => animationController.SetTrigger(triggerHash);
    }
}
