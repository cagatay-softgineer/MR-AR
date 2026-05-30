using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ARFishing.UI
{
    public class ScoreTextAnimator : MonoBehaviour
    {
        [SerializeField] Text m_Target;

        [SerializeField, Tooltip("{0} is replaced with the current integer value during animation.")]
        string m_Format = "Toplam puan: {0}";

        [SerializeField, Min(0.1f)] float m_Duration = 1.2f;

        Coroutine m_Active;

        public void Animate(int from, int to)
        {
            if (m_Active != null) StopCoroutine(m_Active);
            m_Active = null;

            if (!gameObject.activeInHierarchy)
            {
                SetInstant(to);
                return;
            }
            m_Active = StartCoroutine(CountTo(from, to));
        }

        public void SetInstant(int value)
        {
            if (m_Active != null) StopCoroutine(m_Active);
            m_Active = null;
            ApplyText(value);
        }

        IEnumerator CountTo(int from, int to)
        {
            float t = 0f;
            int last = -1;

            while (t < m_Duration)
            {
                t += Time.unscaledDeltaTime;
                float u = EaseOutCubic(Mathf.Clamp01(t / m_Duration));
                int value = Mathf.RoundToInt(Mathf.Lerp(from, to, u));
                if (value != last)
                {
                    ApplyText(value);
                    last = value;
                }
                yield return null;
            }

            ApplyText(to);
            m_Active = null;
        }

        void ApplyText(int value)
        {
            if (m_Target == null) return;
            m_Target.text = string.Format(m_Format, value);
        }

        static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    }
}
