using System;
using UnityEngine;

namespace ARFishing.Core
{
    public class ActivityController : MonoBehaviour
    {
        public event Action<ActivityState, ActivityState> StateChanged;

        public ActivityState Current { get; private set; } = ActivityState.Bootstrap;

        void Awake()
        {
            ServiceLocator.Register(this);
        }

        void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }

        public bool TryTransition(ActivityState next)
        {
            if (!IsLegal(Current, next))
            {
                Debug.LogWarning($"[ActivityController] Illegal transition {Current} -> {next}");
                return false;
            }

            var previous = Current;
            Current = next;
            StateChanged?.Invoke(previous, next);
            return true;
        }

        static bool IsLegal(ActivityState from, ActivityState to)
        {
            return (from, to) switch
            {
                (ActivityState.Bootstrap, ActivityState.Idle) => true,
                (ActivityState.Idle, ActivityState.Scanning) => true,
                (ActivityState.Scanning, ActivityState.Viewing) => true,
                (ActivityState.Viewing, ActivityState.Scanning) => true,
                (ActivityState.Scanning, ActivityState.Quiz) => true,
                (ActivityState.Viewing, ActivityState.Quiz) => true,
                (ActivityState.Quiz, ActivityState.Summary) => true,
                (ActivityState.Summary, ActivityState.Idle) => true,
                // Restart escapes: teacher panel can drop back to Idle from anywhere except Bootstrap.
                (ActivityState.Scanning, ActivityState.Idle) => true,
                (ActivityState.Viewing, ActivityState.Idle) => true,
                (ActivityState.Quiz, ActivityState.Idle) => true,
                _ => false,
            };
        }

        [ContextMenu("Force: Bootstrap -> Idle")]
        void ForceIdle() => TryTransition(ActivityState.Idle);

        [ContextMenu("Force: Idle -> Scanning")]
        void ForceScanning() => TryTransition(ActivityState.Scanning);

        [ContextMenu("Force: -> Quiz")]
        void ForceQuiz() => TryTransition(ActivityState.Quiz);

        [ContextMenu("Force: Quiz -> Summary")]
        void ForceSummary() => TryTransition(ActivityState.Summary);
    }
}
