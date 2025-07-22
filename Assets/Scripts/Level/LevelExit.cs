using Interactable;

namespace Level
{
    public class LevelExit : InteractableObject
    {
        public ExitEvent onExitEvent = new ExitEvent();

        public override void Interact()
        {
            onExitEvent.Invoke();
        }
    }
}