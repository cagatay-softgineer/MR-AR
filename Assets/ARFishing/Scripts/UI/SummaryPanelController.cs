using ARFishing.Core;
using ARFishing.Quiz;
using UnityEngine;
using UnityEngine.UI;

namespace ARFishing.UI
{
    public class SummaryPanelController : MonoBehaviour
    {
        [Header("Wiring")]
        [SerializeField] ActivityController m_Controller;
        [SerializeField] QuizController m_Quiz;

        [Header("Panel root")]
        [SerializeField] GameObject m_PanelRoot;
        [SerializeField] CanvasGroup m_PanelCanvasGroup;
        [SerializeField] PanelTween m_Tween;

        [Header("Text")]
        [SerializeField] Text m_ScoreText;
        [SerializeField] Text m_ClosingPromptText;
        [SerializeField] ScoreTextAnimator m_ScoreAnimator;

        [Header("Buttons")]
        [SerializeField] Button m_RestartButton;

        const string DefaultClosingPrompt =
            "Etkinlik tamamlandı. Eğer bu canlılardan biri yok olursa deniz ekosistemi nasıl etkilenir?";

        void Awake()
        {
            if (m_Controller == null) ServiceLocator.TryGet(out m_Controller);
            if (m_Quiz == null) ServiceLocator.TryGet(out m_Quiz);
            Hide();
        }

        void OnEnable()
        {
            if (m_Controller != null) m_Controller.StateChanged += HandleStateChanged;
            if (m_RestartButton != null) m_RestartButton.onClick.AddListener(OnRestartClicked);
        }

        void OnDisable()
        {
            if (m_Controller != null) m_Controller.StateChanged -= HandleStateChanged;
            if (m_RestartButton != null) m_RestartButton.onClick.RemoveListener(OnRestartClicked);
        }

        void HandleStateChanged(ActivityState previous, ActivityState next)
        {
            if (next == ActivityState.Summary) ShowSummary();
            else if (previous == ActivityState.Summary) Hide();
        }

        void ShowSummary()
        {
            SetVisible(true);
            int score = m_Quiz != null ? m_Quiz.RunningScore : 0;

            if (m_ScoreAnimator != null)
            {
                m_ScoreAnimator.Animate(0, score);
            }
            else if (m_ScoreText != null)
            {
                m_ScoreText.text = $"Toplam puan: {score}";
            }

            if (m_ClosingPromptText != null && string.IsNullOrEmpty(m_ClosingPromptText.text))
            {
                m_ClosingPromptText.text = DefaultClosingPrompt;
            }
        }

        void Hide()
        {
            SetVisible(false);
        }

        void SetVisible(bool visible)
        {
            if (m_Tween != null)
            {
                if (visible) m_Tween.Show();
                else m_Tween.Hide();
                return;
            }

            if (visible && m_PanelRoot != null && !m_PanelRoot.activeSelf)
                m_PanelRoot.SetActive(true);

            if (m_PanelCanvasGroup != null)
            {
                m_PanelCanvasGroup.alpha = visible ? 1f : 0f;
                m_PanelCanvasGroup.interactable = visible;
                m_PanelCanvasGroup.blocksRaycasts = visible;
            }
        }

        void OnRestartClicked()
        {
            if (m_Controller == null) return;
            if (m_Controller.Current == ActivityState.Summary)
            {
                m_Controller.TryTransition(ActivityState.Idle);
            }
        }
    }
}
