using UnityEngine;
using System;

namespace SignalMenu.SignalSafety
{
    public static class ButtonMapper
    {
        public enum MenuButton
        {
            Y_Left,
            B_Right,
            X_Left,
            A_Right,
            PrimaryTrigger,
            SecondaryTrigger
        }

        public static MenuButton CurrentButton
        {
            get { return SafetyConfig.MenuOpenButton; }
            set { SafetyConfig.MenuOpenButton = value; }
        }

        public static bool IsMenuButtonPressed()
        {
            if (ControllerInputPoller.instance == null) return false;

            return CurrentButton switch
            {
                MenuButton.Y_Left => ControllerInputPoller.instance.leftControllerSecondaryButton,
                MenuButton.B_Right => ControllerInputPoller.instance.rightControllerSecondaryButton,
                MenuButton.X_Left => ControllerInputPoller.instance.leftControllerPrimaryButton,
                MenuButton.A_Right => ControllerInputPoller.instance.rightControllerPrimaryButton,
                MenuButton.PrimaryTrigger => 
                    ControllerInputPoller.instance.leftControllerGripFloat > 0.5f || 
                    ControllerInputPoller.instance.rightControllerGripFloat > 0.5f,
                MenuButton.SecondaryTrigger => 
                    ControllerInputPoller.instance.leftControllerIndexFloat > 0.5f || 
                    ControllerInputPoller.instance.rightControllerIndexFloat > 0.5f,
                _ => false
            };
        }

        public static string GetButtonName(MenuButton btn)
        {
            return btn switch
            {
                MenuButton.Y_Left => "Y (Left)",
                MenuButton.B_Right => "B (Right)",
                MenuButton.X_Left => "X (Left)",
                MenuButton.A_Right => "A (Right)",
                MenuButton.PrimaryTrigger => "Grip Trigger",
                MenuButton.SecondaryTrigger => "Index Trigger",
                _ => "Unknown"
            };
        }
    }
}
