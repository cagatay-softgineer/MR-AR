using System.Collections.Generic;
using System.IO;
using ARFishing.Quiz;
using UnityEditor;
using UnityEngine;

namespace ARFishing.Editor
{
    public static class CreateExampleTasksMenu
    {
        const string ContentDir = "Assets/ARFishing/Content";
        const string TasksDir = "Assets/ARFishing/Content/Tasks";

        [MenuItem("ARFishing/Create Example Tasks")]
        public static void CreateAll()
        {
            EnsureDir(ContentDir);
            EnsureDir(TasksDir);

            var tasks = new List<TaskDefinition>();

            tasks.Add(MakeTask("task-reef-creature",
                "Mercan resifinde yaşayan canlıyı bul.",
                MatchRule.HabitatEquals, "Reef", 1));
            tasks.Add(MakeTask("task-filter-feeder",
                "Süzerek beslenen canlıyı bul.",
                MatchRule.DietEquals, "FilterFeeder", 1));
            tasks.Add(MakeTask("task-camouflage",
                "Kamuflaj yapan canlıyı bul.",
                MatchRule.TraitContains, "kamufle", 2));
            tasks.Add(MakeTask("task-invertebrate",
                "Bir omurgasız canlı bul.",
                MatchRule.CategoryEquals, "Invertebrate", 1));
            tasks.Add(MakeTask("task-predator",
                "Bir avcı canlı bul.",
                MatchRule.EcosystemRoleEquals, "Predator", 1));

            var database = CreateOrLoad<TaskDatabase>($"{ContentDir}/TaskDatabase.asset");
            SetTasks(database, tasks);

            foreach (var t in tasks) EditorUtility.SetDirty(t);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = database;
            EditorGUIUtility.PingObject(database);
            Debug.Log($"[ARFishing] Created {tasks.Count} example tasks and TaskDatabase.");
        }

        static TaskDefinition MakeTask(string id, string prompt, MatchRule rule, string matchValue, int points)
        {
            var path = $"{TasksDir}/{id}.asset";
            var def = CreateOrLoad<TaskDefinition>(path);
            var so = new SerializedObject(def);
            so.FindProperty("m_TaskId").stringValue = id;
            so.FindProperty("m_Prompt").stringValue = prompt;
            so.FindProperty("m_Rule").intValue = (int)rule;
            so.FindProperty("m_MatchValue").stringValue = matchValue;
            so.FindProperty("m_Points").intValue = points;
            so.ApplyModifiedPropertiesWithoutUndo();
            return def;
        }

        static void SetTasks(TaskDatabase database, List<TaskDefinition> tasks)
        {
            var so = new SerializedObject(database);
            var arr = so.FindProperty("m_Tasks");
            arr.arraySize = tasks.Count;
            for (int i = 0; i < tasks.Count; i++)
            {
                arr.GetArrayElementAtIndex(i).objectReferenceValue = tasks[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null) return existing;
            var instance = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }

        static void EnsureDir(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            var parent = Path.GetDirectoryName(assetPath).Replace('\\', '/');
            var leaf = Path.GetFileName(assetPath);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureDir(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }
    }
}
