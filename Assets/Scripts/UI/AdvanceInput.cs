using UnityEngine.InputSystem;

// Shared "advance" input for narrative screens: left click, Enter, Space, or gamepad south button.
public static class AdvanceInput
{
    public static bool Pressed()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        var pad = Gamepad.current;
        return (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
            || (mouse != null && mouse.leftButton.wasPressedThisFrame)
            || (pad != null && pad.buttonSouth.wasPressedThisFrame);
    }
}
