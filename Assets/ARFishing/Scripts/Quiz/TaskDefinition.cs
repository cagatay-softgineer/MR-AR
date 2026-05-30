using System;
using ARFishing.Creatures;
using UnityEngine;

namespace ARFishing.Quiz
{
    [CreateAssetMenu(menuName = "ARFishing/Task Definition", fileName = "task-id")]
    public class TaskDefinition : ScriptableObject
    {
        [SerializeField] string m_TaskId;
        [SerializeField, TextArea(2, 4)] string m_Prompt;
        [SerializeField] MatchRule m_Rule;

        [SerializeField, Tooltip("Comparison target. Enum rules: pass the enum name (e.g. 'Reef', 'Carnivore'). TraitContains: pass a substring.")]
        string m_MatchValue;

        [SerializeField, Min(0)] int m_Points = 1;

        public string TaskId => m_TaskId;
        public string Prompt => m_Prompt;
        public MatchRule Rule => m_Rule;
        public string MatchValue => m_MatchValue;
        public int Points => m_Points;

        public bool Matches(CreatureDefinition candidate)
        {
            if (candidate == null || string.IsNullOrEmpty(m_MatchValue)) return false;

            return m_Rule switch
            {
                MatchRule.CategoryEquals => EnumNameEquals(candidate.Category, m_MatchValue),
                MatchRule.HabitatEquals => EnumNameEquals(candidate.Habitat, m_MatchValue),
                MatchRule.DietEquals => EnumNameEquals(candidate.Diet, m_MatchValue),
                MatchRule.EcosystemRoleEquals => EnumNameEquals(candidate.EcosystemRole, m_MatchValue),
                MatchRule.TraitContains => candidate.InterestingTrait != null
                    && candidate.InterestingTrait.IndexOf(m_MatchValue, StringComparison.OrdinalIgnoreCase) >= 0,
                _ => false,
            };
        }

        static bool EnumNameEquals<T>(T value, string expected) where T : Enum
        {
            return string.Equals(value.ToString(), expected, StringComparison.OrdinalIgnoreCase);
        }
    }
}
