using ARFishing.Core;
using ARFishing.Creatures;
using ARFishing.Marker;
using ARFishing.Quiz;
using UnityEngine;
using UnityEngine.UI;

namespace ARFishing.Teacher
{
    public class ProjectionPanelController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] ActivityController m_Controller;
        [SerializeField] FocusResolver m_FocusResolver;
        [SerializeField] QuizController m_Quiz;
        [SerializeField] SessionScannedTracker m_SessionTracker;

        [Header("Sections")]
        [SerializeField] GameObject m_ScanningSection;
        [SerializeField] GameObject m_QuizSection;
        [SerializeField] GameObject m_SummarySection;
        [SerializeField] GameObject m_IdleSection;

        [Header("Scanning section")]
        [SerializeField] Image m_LargeIcon;
        [SerializeField] Text m_LargeName;
        [SerializeField] Text m_LargeCategory;

        [Header("Quiz section")]
        [SerializeField] Text m_QuizPromptText;
        [SerializeField] Text m_QuizProgressText;

        [Header("Summary section")]
        [SerializeField] Text m_SummaryScoreText;
        [SerializeField] Transform m_ScannedGridParent;
        [SerializeField] GameObject m_ScannedChipPrefab;

        void Awake()
        {
            if (m_Controller == null) ServiceLocator.TryGet(out m_Controller);
            if (m_FocusResolver == null) ServiceLocator.TryGet(out m_FocusResolver);
            if (m_Quiz == null) ServiceLocator.TryGet(out m_Quiz);
            if (m_SessionTracker == null) ServiceLocator.TryGet(out m_SessionTracker);
        }

        void OnEnable()
        {
            if (m_Controller != null) m_Controller.StateChanged += HandleStateChanged;
            if (m_FocusResolver != null) m_FocusResolver.FocusChanged += HandleFocusChanged;
            if (m_Quiz != null) m_Quiz.TaskPresented += HandleTaskPresented;
            if (m_SessionTracker != null)
            {
                m_SessionTracker.ScannedAdded += HandleScannedAdded;
                m_SessionTracker.SessionReset += HandleSessionReset;
            }

            ApplyState(m_Controller != null ? m_Controller.Current : ActivityState.Bootstrap);
        }

        void OnDisable()
        {
            if (m_Controller != null) m_Controller.StateChanged -= HandleStateChanged;
            if (m_FocusResolver != null) m_FocusResolver.FocusChanged -= HandleFocusChanged;
            if (m_Quiz != null) m_Quiz.TaskPresented -= HandleTaskPresented;
            if (m_SessionTracker != null)
            {
                m_SessionTracker.ScannedAdded -= HandleScannedAdded;
                m_SessionTracker.SessionReset -= HandleSessionReset;
            }
        }

        void HandleStateChanged(ActivityState previous, ActivityState next)
        {
            ApplyState(next);
        }

        void ApplyState(ActivityState state)
        {
            SetSection(m_IdleSection, state == ActivityState.Idle);
            SetSection(m_ScanningSection,
                state == ActivityState.Scanning || state == ActivityState.Viewing);
            SetSection(m_QuizSection, state == ActivityState.Quiz);
            SetSection(m_SummarySection, state == ActivityState.Summary);

            if (state == ActivityState.Summary)
            {
                PopulateSummary();
            }
        }

        void HandleTaskPresented(TaskDefinition task, int index, int total)
        {
            if (m_QuizPromptText != null && task != null) m_QuizPromptText.text = task.Prompt;
            if (m_QuizProgressText != null) m_QuizProgressText.text = $"{index + 1} / {Mathf.Max(1, total)}";
        }

        void HandleFocusChanged(CreatureDefinition previous, CreatureDefinition next)
        {
            PopulateFocus(next);
        }

        void PopulateFocus(CreatureDefinition def)
        {
            if (m_LargeIcon != null)
            {
                m_LargeIcon.sprite = def != null ? def.Icon : null;
                m_LargeIcon.enabled = def != null && def.Icon != null;
            }
            if (m_LargeName != null) m_LargeName.text = def != null ? def.DisplayName : "";
            if (m_LargeCategory != null) m_LargeCategory.text = def != null ? def.Category.ToTurkish() : "";
        }

        void HandleScannedAdded(CreatureDefinition def)
        {
            if (m_Controller != null && m_Controller.Current != ActivityState.Summary) return;
            AppendScannedChip(def);
        }

        void HandleSessionReset()
        {
            if (m_ScannedGridParent == null) return;
            for (int i = m_ScannedGridParent.childCount - 1; i >= 0; i--)
            {
                Destroy(m_ScannedGridParent.GetChild(i).gameObject);
            }
        }

        void PopulateSummary()
        {
            if (m_SummaryScoreText != null)
            {
                int score = m_Quiz != null ? m_Quiz.RunningScore : 0;
                m_SummaryScoreText.text = $"Toplam puan: {score}";
            }

            if (m_ScannedGridParent != null && m_SessionTracker != null)
            {
                for (int i = m_ScannedGridParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(m_ScannedGridParent.GetChild(i).gameObject);
                }
                foreach (var def in m_SessionTracker.Scanned)
                {
                    AppendScannedChip(def);
                }
            }
        }

        void AppendScannedChip(CreatureDefinition def)
        {
            if (m_ScannedGridParent == null || m_ScannedChipPrefab == null || def == null) return;

            var chip = Instantiate(m_ScannedChipPrefab, m_ScannedGridParent);
            var image = chip.GetComponentInChildren<Image>(true);
            if (image != null && def.Icon != null) image.sprite = def.Icon;
            var text = chip.GetComponentInChildren<Text>(true);
            if (text != null) text.text = def.DisplayName;
        }

        static void SetSection(GameObject section, bool visible)
        {
            if (section != null) section.SetActive(visible);
        }
    }
}
