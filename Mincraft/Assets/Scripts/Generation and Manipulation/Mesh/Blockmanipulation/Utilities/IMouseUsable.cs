namespace Core.Player
{
    public interface IMouseUsable
    {
        float DesiredTimeUntilAction { get; set; }
        float RaycastDistance { get; set; }
        int MouseButtonIndex { get; set; }
    }
}
