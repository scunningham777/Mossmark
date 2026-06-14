using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Mossmark.Attention
{
    public class AttentionInput : MonoBehaviour
    {
        public event Action HoldStarted;
        public event Action HoldReleased;

        private InputAction attendAction;

        private void Awake()
        {
            var gameplay = InputSystem.actions.FindActionMap("Gameplay");
            gameplay.Enable();
            attendAction = gameplay.FindAction("Attend");
        }

        private void OnEnable()
        {
            attendAction.started += OnAttendStarted;
            attendAction.canceled += OnAttendCanceled;
        }

        private void OnDisable()
        {
            attendAction.started -= OnAttendStarted;
            attendAction.canceled -= OnAttendCanceled;
        }

        // The Hold interaction's "started" fires on press and "canceled" fires on release
        // regardless of its configured duration, so raw press/release timing is preserved
        // here and the actual hold-duration check is owned by AttentionManager.
        private void OnAttendStarted(InputAction.CallbackContext context) => HoldStarted?.Invoke();
        private void OnAttendCanceled(InputAction.CallbackContext context) => HoldReleased?.Invoke();
    }
}
