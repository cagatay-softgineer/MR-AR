using System;
using System.Collections;
using System.Collections.Generic;
using ARFishing.Core;
using ARFishing.Creatures;
using ARFishing.Marker;
using ARFishing.Narration;
using ARFishing.UI;
using ARFishing.Viewer;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace ARFishing.Quiz
{
    public class QuizController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] ActivityController m_Controller;
        [SerializeField] MarkerTracker m_Tracker;
        [SerializeField] CreatureViewer m_Viewer;
        [SerializeField] NarrationPlayer m_Narration;
        [SerializeField] QuizPanelController m_Panel;
        [SerializeField] TaskDatabase m_TaskDatabase;

        [Header("Tuning")]
        [SerializeField, Min(1)] int m_TasksPerSession = 3;
        [SerializeField, Min(0.25f)] float m_FeedbackDuration = 2f;

        readonly List<TaskDefinition> m_CurrentTasks = new();
        int m_TaskIndex;
        int m_RunningScore;
        bool m_AwaitingAnswer;

        public int RunningScore => m_RunningScore;
        public int CurrentTaskIndex => m_TaskIndex;
        public int CurrentTaskCount => m_CurrentTasks.Count;

        public event Action<TaskDefinition, int, int> TaskPresented;
        public event Action<bool, TaskDefinition, CreatureDefinition> AnswerEvaluated;

        void Awake()
        {
            if (m_Controller == null) ServiceLocator.TryGet(out m_Controller);
            if (m_Tracker == null) ServiceLocator.TryGet(out m_Tracker);
            if (m_Viewer == null) ServiceLocator.TryGet(out m_Viewer);
            if (m_Narration == null) ServiceLocator.TryGet(out m_Narration);
            if (m_TaskDatabase == null) ServiceLocator.TryGet(out m_TaskDatabase);
            ServiceLocator.Register(this);
        }

        void OnEnable()
        {
            if (m_Controller != null) m_Controller.StateChanged += HandleStateChanged;
        }

        void OnDisable()
        {
            if (m_Controller != null) m_Controller.StateChanged -= HandleStateChanged;
        }

        void OnDestroy()
        {
            ServiceLocator.Unregister(this);
        }

        void HandleStateChanged(ActivityState previous, ActivityState next)
        {
            if (next == ActivityState.Quiz) BeginQuiz();
            else if (previous == ActivityState.Quiz) EndQuiz();
        }

        void BeginQuiz()
        {
            m_RunningScore = 0;
            m_CurrentTasks.Clear();
            PickTasks(m_TasksPerSession, m_CurrentTasks);
            m_TaskIndex = 0;

            if (m_Viewer != null) m_Viewer.ShowModels = false;
            if (m_Narration != null) m_Narration.Stop();
            if (m_Tracker != null) m_Tracker.Spotted += HandleAnswerSpotted;
            if (m_Panel != null) m_Panel.Show();

            if (m_CurrentTasks.Count == 0)
            {
                Debug.LogWarning("[QuizController] TaskDatabase empty or null; skipping straight to Summary.");
                if (m_Controller != null) m_Controller.TryTransition(ActivityState.Summary);
                return;
            }

            PresentCurrentTask();
        }

        void EndQuiz()
        {
            if (m_Tracker != null) m_Tracker.Spotted -= HandleAnswerSpotted;
            if (m_Viewer != null) m_Viewer.ShowModels = true;
            if (m_Panel != null) m_Panel.Hide();
            m_AwaitingAnswer = false;
        }

        void HandleAnswerSpotted(CreatureDefinition def, ARTrackedImage image)
        {
            if (!m_AwaitingAnswer) return;
            if (m_TaskIndex < 0 || m_TaskIndex >= m_CurrentTasks.Count) return;

            var task = m_CurrentTasks[m_TaskIndex];
            bool correct = task.Matches(def);
            if (correct) m_RunningScore += task.Points;

            m_AwaitingAnswer = false;
            if (m_Panel != null) m_Panel.ShowFeedback(correct, task, def);
            AnswerEvaluated?.Invoke(correct, task, def);
            StartCoroutine(AdvanceAfterDelay());
        }

        IEnumerator AdvanceAfterDelay()
        {
            yield return new WaitForSeconds(m_FeedbackDuration);
            m_TaskIndex++;
            if (m_TaskIndex >= m_CurrentTasks.Count)
            {
                if (m_Controller != null) m_Controller.TryTransition(ActivityState.Summary);
            }
            else
            {
                PresentCurrentTask();
            }
        }

        void PresentCurrentTask()
        {
            var task = m_CurrentTasks[m_TaskIndex];
            if (m_Panel != null) m_Panel.ShowPrompt(task, m_TaskIndex, m_CurrentTasks.Count);
            TaskPresented?.Invoke(task, m_TaskIndex, m_CurrentTasks.Count);
            m_AwaitingAnswer = true;
        }

        void PickTasks(int count, List<TaskDefinition> destination)
        {
            destination.Clear();
            if (m_TaskDatabase == null || m_TaskDatabase.Count == 0) return;

            var pool = new List<TaskDefinition>(m_TaskDatabase.Count);
            for (int i = 0; i < m_TaskDatabase.Count; i++)
            {
                var task = m_TaskDatabase.All[i];
                if (task != null) pool.Add(task);
            }

            int picks = Mathf.Min(count, pool.Count);
            for (int i = 0; i < picks; i++)
            {
                int idx = UnityEngine.Random.Range(0, pool.Count);
                destination.Add(pool[idx]);
                pool.RemoveAt(idx);
            }
        }
    }
}
