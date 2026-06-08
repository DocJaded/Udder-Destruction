using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UdderDestruction
{
    public static class UdderPersistence
    {
        [Serializable]
        private sealed class PersistentData
        {
            public int version = 1;
            public int chickensDefeated;
            public int timesCheesedIt;
            public int chickensSlippedOnButter;
            public int pondsPolluted;
            public int dolphinsDefeated;
            public List<string> defeatedEnemyKinds = new();
            public List<string> unlockedAchievements = new();
        }

        private static PersistentData data;
        private static readonly HashSet<string> defeatedEnemyKinds = new();
        private static readonly HashSet<string> unlockedAchievements = new();
        private static int changesSinceSave;

        public static string SavePath => Path.Combine(Application.persistentDataPath, "udder-persistent-data.json");

        public static bool IsAchievementUnlocked(UdderAchievementId achievement)
        {
            EnsureLoaded();
            return unlockedAchievements.Contains(achievement.ToString());
        }

        public static int GetProgress(UdderAchievementId achievement)
        {
            EnsureLoaded();
            return achievement switch
            {
                UdderAchievementId.EverybodysHerdAboutTheBird => data.chickensDefeated,
                UdderAchievementId.DejaMoo => data.timesCheesedIt,
                UdderAchievementId.ButterChicken => data.chickensSlippedOnButter,
                UdderAchievementId.TheScumAlsoRises => data.pondsPolluted,
                UdderAchievementId.TheyCalledHimFlipper => data.dolphinsDefeated,
                UdderAchievementId.MuensterHunter => GetDefeatedCoreEnemyKindCount(),
                _ => IsAchievementUnlocked(achievement) ? 1 : 0,
            };
        }

        public static void Flush()
        {
            EnsureLoaded();
            Save();
        }

        public static void RecordEnemyDefeated(UdderEnemyKind kind, bool isMiyamotoMoosashi)
        {
            EnsureLoaded();
            defeatedEnemyKinds.Add(kind.ToString());

            if (kind == UdderEnemyKind.DebtChicken)
            {
                data.chickensDefeated++;
                UnlockAtThreshold(UdderAchievementId.EverybodysHerdAboutTheBird, data.chickensDefeated, 1000);
            }

            if (kind == UdderEnemyKind.Dolphin)
            {
                data.dolphinsDefeated++;
                UnlockAtThreshold(UdderAchievementId.TheyCalledHimFlipper, data.dolphinsDefeated, 100);
            }

            if (kind == UdderEnemyKind.Cow)
                Unlock(UdderAchievementId.HayWatchIt);
            if (kind == UdderEnemyKind.AlGore)
                Unlock(UdderAchievementId.CallMeAl);
            if (isMiyamotoMoosashi)
                Unlock(UdderAchievementId.WagyuTalkinAbout);

            if (HasDefeatedEveryEnemyKind())
                Unlock(UdderAchievementId.MuensterHunter);

            MarkDirty();
        }

        public static void RecordVeganWaveCleared()
        {
            EnsureLoaded();
            Unlock(UdderAchievementId.HolaSoyMilk);
            MarkDirty();
        }

        public static void RecordCheesedIt()
        {
            EnsureLoaded();
            data.timesCheesedIt++;
            UnlockAtThreshold(UdderAchievementId.DejaMoo, data.timesCheesedIt, 1000);
            MarkDirty();
        }

        public static void RecordChickenSlippedOnButter()
        {
            EnsureLoaded();
            data.chickensSlippedOnButter++;
            UnlockAtThreshold(UdderAchievementId.ButterChicken, data.chickensSlippedOnButter, 1000);
            MarkDirty();
        }

        public static void RecordPondPolluted()
        {
            EnsureLoaded();
            data.pondsPolluted++;
            UnlockAtThreshold(UdderAchievementId.TheScumAlsoRises, data.pondsPolluted, 100);
            Save();
        }

        private static bool HasDefeatedEveryEnemyKind()
        {
            return GetDefeatedCoreEnemyKindCount() >= 4;
        }

        private static int GetDefeatedCoreEnemyKindCount()
        {
            int count = 0;
            if (defeatedEnemyKinds.Contains(UdderEnemyKind.DebtChicken.ToString()))
                count++;
            if (defeatedEnemyKinds.Contains(UdderEnemyKind.HostileHam.ToString()))
                count++;
            if (defeatedEnemyKinds.Contains(UdderEnemyKind.Cow.ToString()))
                count++;
            if (defeatedEnemyKinds.Contains(UdderEnemyKind.Dolphin.ToString()))
                count++;
            return count;
        }

        private static void UnlockAtThreshold(UdderAchievementId achievement, int progress, int threshold)
        {
            if (progress >= threshold)
                Unlock(achievement);
        }

        private static void Unlock(UdderAchievementId achievement)
        {
            unlockedAchievements.Add(achievement.ToString());
        }

        private static void MarkDirty()
        {
            changesSinceSave++;
            if (changesSinceSave >= 10)
                Save();
        }

        private static void EnsureLoaded()
        {
            if (data != null)
                return;

            try
            {
                data = File.Exists(SavePath)
                    ? JsonUtility.FromJson<PersistentData>(File.ReadAllText(SavePath))
                    : new PersistentData();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not load persistent Udder Destruction data: {exception.Message}");
                data = new PersistentData();
            }

            data ??= new PersistentData();
            data.defeatedEnemyKinds ??= new List<string>();
            data.unlockedAchievements ??= new List<string>();
            defeatedEnemyKinds.Clear();
            unlockedAchievements.Clear();
            defeatedEnemyKinds.UnionWith(data.defeatedEnemyKinds);
            unlockedAchievements.UnionWith(data.unlockedAchievements);
        }

        private static void Save()
        {
            data.defeatedEnemyKinds = new List<string>(defeatedEnemyKinds);
            data.unlockedAchievements = new List<string>(unlockedAchievements);

            try
            {
                string directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);
                File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
                changesSinceSave = 0;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not save persistent Udder Destruction data: {exception.Message}");
            }
        }
    }
}
