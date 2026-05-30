using ARFishing.Creatures;
using ARFishing.Quiz;
using UnityEngine;
using UnityEngine.UI;

namespace ARFishing.UI
{
    public class QuizPanelController : MonoBehaviour
    {
        [Header("Panel root")]
        [SerializeField] GameObject m_PanelRoot;
        [SerializeField] CanvasGroup m_PanelCanvasGroup;
        [SerializeField] PanelTween m_Tween;

        [Header("Text fields")]
        [SerializeField] Text m_PromptText;
        [SerializeField] Text m_ProgressText;

        [Header("Feedback")]
        [SerializeField] GameObject m_FeedbackRoot;
        [SerializeField] Text m_FeedbackText;
        [SerializeField] TextPopAnimator m_FeedbackPop;

        void Awake()
        {
            Hide();
        }

        public void Show()
        {
            SetVisible(true);
            if (m_FeedbackRoot != null) m_FeedbackRoot.SetActive(false);
        }

        public void Hide()
        {
            SetVisible(false);
            if (m_FeedbackRoot != null) m_FeedbackRoot.SetActive(false);
        }

        public void ShowPrompt(TaskDefinition task, int index, int total)
        {
            if (task == null) return;
            if (m_PromptText != null) m_PromptText.text = task.Prompt;
            if (m_ProgressText != null) m_ProgressText.text = $"{index + 1} / {total}";
            if (m_FeedbackRoot != null) m_FeedbackRoot.SetActive(false);
        }

        public void ShowFeedback(bool correct, TaskDefinition task, CreatureDefinition scanned)
        {
            if (m_FeedbackRoot != null) m_FeedbackRoot.SetActive(true);

            if (m_FeedbackText != null)
            {
                var scannedName = scanned != null ? scanned.DisplayName : "Bu kart";
                m_FeedbackText.text = correct
                    ? $"Doğru! {scannedName} aradığımız canlıydı."
                    : $"Tam değil. {scannedName} bu görevi karşılamıyor.";
            }

            if (m_FeedbackPop != null) m_FeedbackPop.Pop();
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
    }
}
