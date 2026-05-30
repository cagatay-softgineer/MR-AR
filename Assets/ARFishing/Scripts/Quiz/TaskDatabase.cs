using System.Collections.Generic;
using UnityEngine;

namespace ARFishing.Quiz
{
    [CreateAssetMenu(menuName = "ARFishing/Task Database", fileName = "TaskDatabase")]
    public class TaskDatabase : ScriptableObject
    {
        [SerializeField] TaskDefinition[] m_Tasks;

        public IReadOnlyList<TaskDefinition> All => m_Tasks;
        public int Count => m_Tasks?.Length ?? 0;
    }
}
