using System.Collections.Generic;
using System.IO;
using ARFishing.Creatures;
using ARFishing.Quiz;
using UnityEditor;
using UnityEngine;

namespace ARFishing.Editor
{
    public static class CreateMvpContentMenu
    {
        const string ContentDir = "Assets/ARFishing/Content";
        const string CreaturesDir = "Assets/ARFishing/Content/Creatures";
        const string TasksDir = "Assets/ARFishing/Content/Tasks";

        [MenuItem("ARFishing/Create MVP Content (20 creatures + 8 tasks)")]
        public static void CreateAll()
        {
            EnsureDir(CreaturesDir);
            EnsureDir(TasksDir);

            var creatures = new List<CreatureDefinition>();

            // 1. Existing example creatures (re-scaffolded with full metadata).
            creatures.Add(MakeCreature("octopus", "Ahtapot",
                CreatureCategory.Invertebrate, Habitat.Reef, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "avlanma", "habitat kaybı" },
                "Renk değiştirerek kamufle olabilir ve dar yerlere sığabilir."));

            creatures.Add(MakeCreature("moon-jellyfish", "Ay Denizanası",
                CreatureCategory.Invertebrate, Habitat.OpenSea, DietType.FilterFeeder, EcosystemRole.Predator,
                new[] { "deniz kirliliği", "iklim değişikliği" },
                "Bazı türleri ışık saçabilir; dokunaçları küçük canlıları yakalar."));

            // 2. Fish family.
            creatures.Add(MakeCreature("clownfish", "Palyaço Balığı",
                CreatureCategory.Fish, Habitat.Reef, DietType.Omnivore, EcosystemRole.Prey,
                new[] { "mercan beyazlaması", "habitat kaybı" },
                "Anemonlarla ortak yaşar ve anemonun zehrinden etkilenmez."));

            creatures.Add(MakeCreature("great-white-shark", "Beyaz Köpekbalığı",
                CreatureCategory.Fish, Habitat.OpenSea, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "avlanma", "plastik kirliliği" },
                "Kokuyu kilometrelerce uzaktan algılayabilir; çok güçlü bir yüzücüdür."));

            creatures.Add(MakeCreature("seahorse", "Denizatı",
                CreatureCategory.Fish, Habitat.Reef, DietType.FilterFeeder, EcosystemRole.Prey,
                new[] { "avlanma", "habitat kaybı" },
                "Erkekler yumurtaları kendi kesesinde taşır ve yavruları doğurur."));

            creatures.Add(MakeCreature("moray-eel", "Müren",
                CreatureCategory.Fish, Habitat.Reef, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "avlanma", "habitat kaybı" },
                "Kayalıkların arasında saklanır; iki çene yapısıyla avını yakalar."));

            creatures.Add(MakeCreature("stingray", "Vatoz",
                CreatureCategory.Fish, Habitat.Seabed, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "avlanma", "yanlışlıkla ağa takılma" },
                "Kuyruğundaki dikende zehir bulunur; kumun altına saklanabilir."));

            creatures.Add(MakeCreature("anglerfish", "Fener Balığı",
                CreatureCategory.DeepSea, Habitat.DeepSea, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "derin deniz balıkçılığı" },
                "Başındaki ışıkla av çeker; güneş ışığı görmeyen derinliklerde yaşar."));

            creatures.Add(MakeCreature("parrotfish", "Papağan Balığı",
                CreatureCategory.Fish, Habitat.Reef, DietType.Herbivore, EcosystemRole.Cleaner,
                new[] { "mercan beyazlaması", "avlanma" },
                "Mercanları kemirir ve sindirdiklerinden ince kum üretir."));

            // 3. Other invertebrates.
            creatures.Add(MakeCreature("crab", "Yengeç",
                CreatureCategory.Invertebrate, Habitat.Seabed, DietType.Omnivore, EcosystemRole.Cleaner,
                new[] { "avlanma", "habitat kaybı" },
                "Yanlamasına yürür; sert kabuğu zırh gibi koruma sağlar."));

            creatures.Add(MakeCreature("starfish", "Deniz Yıldızı",
                CreatureCategory.Invertebrate, Habitat.Seabed, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "kirlilik", "deniz yıldızı hastalıkları" },
                "Kopan bir kolunu yeniden büyütebilir; midyeleri açıp yiyebilir."));

            creatures.Add(MakeCreature("mussel", "Midye",
                CreatureCategory.Invertebrate, Habitat.Seabed, DietType.FilterFeeder, EcosystemRole.Cleaner,
                new[] { "kirlilik", "aşırı toplama" },
                "Saatte litrelerce suyu süzerek deniz suyunu temiz tutar."));

            creatures.Add(MakeCreature("squid", "Mürekkep Balığı",
                CreatureCategory.Invertebrate, Habitat.OpenSea, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "avlanma", "plastik kirliliği" },
                "Tehlike anında mürekkep bulutu salar ve hızla kaçar."));

            creatures.Add(MakeCreature("sea-urchin", "Deniz Kestanesi",
                CreatureCategory.Invertebrate, Habitat.Seabed, DietType.Herbivore, EcosystemRole.Cleaner,
                new[] { "asit kirliliği", "habitat kaybı" },
                "Dikenleriyle kendini korur ve algleri yiyerek resifi temiz tutar."));

            // 4. Reef / Coral.
            creatures.Add(MakeCreature("coral", "Mercan",
                CreatureCategory.Coral, Habitat.Reef, DietType.FilterFeeder, EcosystemRole.ShelterProvider,
                new[] { "mercan beyazlaması", "iklim değişikliği", "kirlilik" },
                "Binlerce canlıya ev sahipliği yapan canlı kayalıklardır."));

            // 5. Producers.
            creatures.Add(MakeCreature("green-algae", "Yeşil Alg",
                CreatureCategory.Producer, Habitat.Surface, DietType.Photosynthesizer, EcosystemRole.Producer,
                new[] { "kirlilik", "aşırı çoğalma" },
                "Güneş ışığıyla kendi besinini üretir ve denize oksijen verir."));

            creatures.Add(MakeCreature("seagrass", "Deniz Çayırı",
                CreatureCategory.Producer, Habitat.Seabed, DietType.Photosynthesizer, EcosystemRole.ShelterProvider,
                new[] { "gemi çapaları", "kirlilik" },
                "Sualtı çayırları küçük canlılara saklanma yeri sağlar."));

            creatures.Add(MakeCreature("plankton", "Plankton",
                CreatureCategory.Producer, Habitat.Surface, DietType.Photosynthesizer, EcosystemRole.Producer,
                new[] { "deniz sıcaklığı", "kirlilik" },
                "Gözle görülmeyecek kadar küçüktür ama deniz besin zincirinin temelidir."));

            // 6. Endangered + special categories.
            creatures.Add(MakeCreature("sea-turtle", "Deniz Kaplumbağası",
                CreatureCategory.Reptile, Habitat.OpenSea, DietType.Omnivore, EcosystemRole.Prey,
                new[] { "plastik kirliliği", "avlanma", "kıyı kaybı" },
                "Binlerce kilometre göç eder ve doğduğu kıyıya yumurtlamaya döner."));

            creatures.Add(MakeCreature("dolphin", "Yunus",
                CreatureCategory.Mammal, Habitat.OpenSea, DietType.Carnivore, EcosystemRole.Predator,
                new[] { "ağa takılma", "plastik kirliliği", "gürültü kirliliği" },
                "Sesle birbiriyle iletişim kurar; grup hâlinde avlanır."));

            var creatureDb = CreateOrLoad<CreatureDatabase>($"{ContentDir}/CreatureDatabase.asset");
            SetCreaturesArray(creatureDb, creatures);

            // Task set (8 total).
            var tasks = new List<TaskDefinition>();
            tasks.Add(MakeTask("task-reef-creature",
                "Mercan resifinde yaşayan canlıyı bul.", MatchRule.HabitatEquals, "Reef", 1));
            tasks.Add(MakeTask("task-filter-feeder",
                "Süzerek beslenen canlıyı bul.", MatchRule.DietEquals, "FilterFeeder", 1));
            tasks.Add(MakeTask("task-camouflage",
                "Kamuflaj yapan canlıyı bul.", MatchRule.TraitContains, "kamufle", 2));
            tasks.Add(MakeTask("task-invertebrate",
                "Bir omurgasız canlı bul.", MatchRule.CategoryEquals, "Invertebrate", 1));
            tasks.Add(MakeTask("task-predator",
                "Bir avcı canlı bul.", MatchRule.EcosystemRoleEquals, "Predator", 1));
            tasks.Add(MakeTask("task-producer",
                "Üretici bir canlı bul.", MatchRule.CategoryEquals, "Producer", 1));
            tasks.Add(MakeTask("task-deep-sea",
                "Derin denizde yaşayan canlıyı bul.", MatchRule.HabitatEquals, "DeepSea", 2));
            tasks.Add(MakeTask("task-shelter-provider",
                "Diğer canlılara ev sahipliği yapan bir canlı bul.",
                MatchRule.EcosystemRoleEquals, "ShelterProvider", 2));

            var taskDb = CreateOrLoad<TaskDatabase>($"{ContentDir}/TaskDatabase.asset");
            SetTasksArray(taskDb, tasks);

            foreach (var c in creatures) EditorUtility.SetDirty(c);
            foreach (var t in tasks) EditorUtility.SetDirty(t);
            EditorUtility.SetDirty(creatureDb);
            EditorUtility.SetDirty(taskDb);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = creatureDb;
            EditorGUIUtility.PingObject(creatureDb);
            Debug.Log($"[ARFishing] MVP content scaffolded: {creatures.Count} creatures, {tasks.Count} tasks.");
        }

        static CreatureDefinition MakeCreature(string id, string displayName,
            CreatureCategory category, Habitat habitat, DietType diet, EcosystemRole role,
            string[] threats, string trait)
        {
            var path = $"{CreaturesDir}/{id}.asset";
            var def = CreateOrLoad<CreatureDefinition>(path);
            var so = new SerializedObject(def);
            so.FindProperty("m_CreatureId").stringValue = id;
            so.FindProperty("m_DisplayName").stringValue = displayName;
            so.FindProperty("m_Category").intValue = (int)category;
            so.FindProperty("m_Habitat").intValue = (int)habitat;
            so.FindProperty("m_Diet").intValue = (int)diet;
            so.FindProperty("m_EcosystemRole").intValue = (int)role;

            var threatsArr = so.FindProperty("m_Threats");
            threatsArr.arraySize = threats?.Length ?? 0;
            if (threats != null)
            {
                for (int i = 0; i < threats.Length; i++)
                {
                    threatsArr.GetArrayElementAtIndex(i).stringValue = threats[i];
                }
            }

            so.FindProperty("m_InterestingTrait").stringValue = trait;
            so.FindProperty("m_ReferenceImageName").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            return def;
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

        static void SetCreaturesArray(CreatureDatabase database, List<CreatureDefinition> creatures)
        {
            var so = new SerializedObject(database);
            var arr = so.FindProperty("m_Creatures");
            arr.arraySize = creatures.Count;
            for (int i = 0; i < creatures.Count; i++)
            {
                arr.GetArrayElementAtIndex(i).objectReferenceValue = creatures[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetTasksArray(TaskDatabase database, List<TaskDefinition> tasks)
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
