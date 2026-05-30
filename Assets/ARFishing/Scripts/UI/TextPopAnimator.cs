using System.Collections;
using UnityEngine;

namespace ARFishing.UI
{
    public class TextPopAnimator : MonoBehaviour
    {
        [SerializeField] RectTransform m_Target;
        [SerializeField, Min(0.05f)] float m_Duration = 0.3f;
        [SerializeField, Min(1f)] float m_OvershootScale = 1.15f;
        [SerializeField, Range(0f, 1f)] float m_StartScale = 0.55f;

        Vector3 m_BaseScale = Vector3.one;
        bool m_Initialized;
        Coroutine m_Active;

        void Initialize()
        {
            if (m_Initialized) return;
            if (m_Target == null) m_Target = transform as RectTransform;
            if (m_Target != null) m_BaseScale = m_Target.localScale;
            m_Initialized = true;
        }

        public void Pop()
        {
            Initialize();
            if (m_Target == null) return;
            if (!gameObject.activeInHierarchy)
            {
                m_Target.localScale = m_BaseScale;
                return;
            }

            if (m_Active != null) StopCoroutine(m_Active);
            m_Active = StartCoroutine(PopRoutine());
        }

        public void ResetScale()
        {
            Initialize();
            if (m_Active != null) StopCoroutine(m_Active);
            m_Active = null;
            if (m_Target != null) m_Target.localScale = m_BaseScale;
        }

        IEnumerator PopRoutine()
        {
            float t = 0f;
            while (t < m_Duration)
            {
                t += Time.unscaledDeltaTime;
                float u = Mathf.Clamp01(t / m_Duration);
                float scale;

                if (u < 0.5f)
                {
                    float a = u * 2f;
                    scale = Mathf.Lerp(m_StartScale, m_OvershootScale, EaseOutCubic(a));
                }
                else
                {
                    float a = (u - 0.5f) * 2f;
                    scale = Mathf.Lerp(m_OvershootScale, 1f, EaseOutCubic(a));
                }

                m_Target.localScale = m_BaseScale * scale;
                yield return null;
            }

            m_Target.localScale = m_BaseScale;
            m_Active = null;
        }

        static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
    }
}
