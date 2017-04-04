using System;
using UnityEngine;

namespace BMW.Verification.CloudRayTracing
{
    public class KeyboardShortcut : MonoBehaviour
    {
        public bool Alt;
        public bool Control;
        public KeyCode Key;
        public bool Shift;

        private void OnGUI()
        {
            if (this.Pressed(Event.current))
            {
                base.SendMessage("ToggleCollapsed", SendMessageOptions.DontRequireReceiver);
            }
        }

        public bool Pressed(Event currentEvent)
        {
            base.useGUILayout = false;
            if ((currentEvent.type != EventType.KeyUp) || (currentEvent.keyCode != this.Key))
            {
                return false;
            }
            return ((!(this.Control ^ currentEvent.control) && !(this.Shift ^ currentEvent.shift)) && (this.Alt == currentEvent.alt));
        }
    }
}

