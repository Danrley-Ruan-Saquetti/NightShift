using System;
using UnityEngine;

namespace NightShift
{
    public class EventDirector : MonoBehaviour
    {
        [SerializeField]
        private TaskRegistry tasks;
        [SerializeField]
        private PlayerState playerState;
        [SerializeField]
        private FadeTeleportationProvider teleportationProvider;
        [SerializeField]
        private GameOver gameOver;
        [SerializeField]
        private VisitorEvent visitor;
        [SerializeField]
        private ShadowEvent shadow;
        [SerializeField]
        private float minimumGap = 40f;

        public EventKind? CurrentEvent { get; private set; }

        private EventSchedule schedule;
        private bool isStopped;

        public event Action<EventKind> Queued;
        public event Action<EventKind> Started;
        public event Action<EventKind> Ended;
        public event Action<EventKind, GameOverReason> Failed;

        private void Awake()
        {
            schedule = new EventSchedule(EventSchedule.Default, minimumGap);
            schedule.Queued += OnScheduleQueued;
        }

        private void OnEnable()
        {
            tasks.Progress.TaskCompleted += OnTaskCompleted;
            gameOver.Triggered += OnGameOverTriggered;
            visitor.Succeeded += OnVisitorSucceeded;
            shadow.Succeeded += OnShadowSucceeded;
        }

        private void Start()
        {
            schedule.SetCompletedCount(tasks.Progress.CompletedCount);
        }

        private void Update()
        {
            if (isStopped || schedule.IsRunning)
            {
                return;
            }

            bool canStartShadow = shadow.CanStartIn(playerState.CurrentRoom);

            if (schedule.TryTakeNext(Time.time, teleportationProvider.IsFading, canStartShadow, out EventKind kind))
            {
                StartEvent(kind);
            }
        }

        private void OnDisable()
        {
            tasks.Progress.TaskCompleted -= OnTaskCompleted;
            gameOver.Triggered -= OnGameOverTriggered;
            visitor.Succeeded -= OnVisitorSucceeded;
            shadow.Succeeded -= OnShadowSucceeded;
        }

        public bool RequestMirrorShadow()
        {
            if (isStopped || !shadow.CanStartIn(playerState.CurrentRoom))
            {
                return false;
            }

            if (!schedule.TryTakeMirror(Time.time, teleportationProvider.IsFading))
            {
                return false;
            }

            StartEvent(EventKind.Shadow);

            return true;
        }

        public bool Request(EventKind kind, bool ignoreGap)
        {
            if (isStopped || schedule.IsRunning || teleportationProvider.IsFading)
            {
                return false;
            }

            if (kind == EventKind.Shadow && !shadow.CanStartIn(playerState.CurrentRoom))
            {
                return false;
            }

            if (!ignoreGap && !schedule.CanStart(Time.time, false))
            {
                return false;
            }

            schedule.BeginUnscheduled();
            StartEvent(kind);

            return true;
        }

        private IHouseEvent GetEvent(EventKind kind)
        {
            return kind == EventKind.Visitor ? visitor : shadow;
        }

        private void StartEvent(EventKind kind)
        {
            CurrentEvent = kind;
            Started?.Invoke(kind);
            GetEvent(kind).Begin();
        }

        private void FinishEvent(EventKind kind)
        {
            schedule.End(Time.time);
            CurrentEvent = null;
            Ended?.Invoke(kind);
        }

        private void OnScheduleQueued(EventKind kind)
        {
            Queued?.Invoke(kind);
        }

        private void OnTaskCompleted(string taskId, int total)
        {
            schedule.SetCompletedCount(total);
        }

        private void OnVisitorSucceeded()
        {
            FinishEvent(EventKind.Visitor);
        }

        private void OnShadowSucceeded()
        {
            FinishEvent(EventKind.Shadow);
        }

        private void OnGameOverTriggered(GameOverReason reason)
        {
            isStopped = true;

            if (CurrentEvent == null)
            {
                return;
            }

            EventKind kind = CurrentEvent.Value;
            Failed?.Invoke(kind, reason);
            GetEvent(kind).Cancel();
            CurrentEvent = null;
        }
    }
}
