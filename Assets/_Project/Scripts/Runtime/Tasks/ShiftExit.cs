using System;
using UnityEngine;

namespace NightShift
{
    public class ShiftExit : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [SerializeField]
        private TaskRegistry tasks;
        [SerializeField]
        private EventDirector director;
        [SerializeField]
        private GameOver gameOver;
        [SerializeField]
        private HingedDoor door;
        [SerializeField]
        private EntranceDoorCloser entranceCloser;
        [SerializeField]
        private Renderer led;
        [SerializeField]
        private Color lockedColor = new(0.5f, 0.03f, 0.03f);
        [SerializeField]
        private Color unlockedColor = new(0.1f, 1f, 0.2f);
        [SerializeField]
        private AudioSource lockSource;
        [SerializeField]
        private AudioClip unlockClip;
        [SerializeField]
        private float exitAngle = 15f;
        [SerializeField]
        private string finalSceneName = "TelaFinal";

        private ShiftCompletion completion;
        private MaterialPropertyBlock propertyBlock;

        private ShiftCompletion Completion => completion ??= new ShiftCompletion(exitAngle);

        public bool IsUnlocked => Completion.IsUnlocked;

        public event Action Unlocked;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            tasks.Progress.TaskCompleted += OnTaskCompleted;
            director.Ended += OnEventEnded;
            ApplyLed(Completion.IsUnlocked);
        }

        private void OnDisable()
        {
            tasks.Progress.TaskCompleted -= OnTaskCompleted;
            director.Ended -= OnEventEnded;
        }

        private void Update()
        {
            if (!Completion.IsUnlocked)
            {
                return;
            }

            SceneTransition transition = SceneTransition.Instance;

            if (transition == null)
            {
                Debug.LogError("SceneTransition is missing, the player cannot leave the house", this);
                return;
            }

            if (transition.IsTransitioning)
            {
                return;
            }

            if (Completion.TryExit(door.Angle))
            {
                transition.LoadScene(finalSceneName);
            }
        }

        private void TryUnlock()
        {
            if (gameOver.IsRunning)
            {
                return;
            }

            TaskProgress progress = tasks.Progress;

            if (!Completion.TryUnlock(progress.CompletedCount, progress.TotalCount, director.CurrentEvent != null))
            {
                return;
            }

            entranceCloser.Disarm();
            door.Unlock();
            ApplyLed(true);
            lockSource.PlayOneShot(unlockClip);
            Unlocked?.Invoke();
        }

        private void ApplyLed(bool isUnlocked)
        {
            propertyBlock.SetColor(BaseColorId, isUnlocked ? unlockedColor : lockedColor);
            led.SetPropertyBlock(propertyBlock);
        }

        private void OnTaskCompleted(string _, int __)
        {
            TryUnlock();
        }

        private void OnEventEnded(EventKind _)
        {
            TryUnlock();
        }
    }
}
