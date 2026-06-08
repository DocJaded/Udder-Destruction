namespace UdderDestruction
{
    public enum UdderAchievementId
    {
        CallMeAl,
        MuensterHunter,
        HayWatchIt,
        HolaSoyMilk,
        EverybodysHerdAboutTheBird,
        WagyuTalkinAbout,
        DejaMoo,
        ButterChicken,
        TheScumAlsoRises,
        TheyCalledHimFlipper,
    }

    public enum UdderEnemyKind
    {
        DebtChicken,
        HostileHam,
        Cow,
        Dolphin,
        AlGore,
        Bee,
    }

    public enum UdderGameMode
    {
        Standard,
        Vegan,
    }

    public static class UdderAchievementCatalog
    {
        public static string GetName(UdderAchievementId achievement)
        {
            return achievement switch
            {
                UdderAchievementId.CallMeAl => "Call me Al",
                UdderAchievementId.MuensterHunter => "Muenster Hunter",
                UdderAchievementId.HayWatchIt => "Hay, watch it!",
                UdderAchievementId.HolaSoyMilk => "Hola, soy milk",
                UdderAchievementId.EverybodysHerdAboutTheBird => "Everybody's herd about the bird",
                UdderAchievementId.WagyuTalkinAbout => "Wagyu talkin' about?",
                UdderAchievementId.DejaMoo => "Déjà Moo",
                UdderAchievementId.ButterChicken => "Butter Chicken",
                UdderAchievementId.TheScumAlsoRises => "The Scum Also Rises",
                UdderAchievementId.TheyCalledHimFlipper => "They CALLED him Flipper",
                _ => achievement.ToString(),
            };
        }

        public static string GetDescription(UdderAchievementId achievement)
        {
            return achievement switch
            {
                UdderAchievementId.CallMeAl => "Defeat Al Gore.",
                UdderAchievementId.MuensterHunter => "Defeat one of each type of enemy.",
                UdderAchievementId.HayWatchIt => "Defeat another cow.",
                UdderAchievementId.HolaSoyMilk => "Beat one wave in Vegan mode.",
                UdderAchievementId.EverybodysHerdAboutTheBird => "Kill 1000 chickens.",
                UdderAchievementId.WagyuTalkinAbout => "Defeat Miyamoto MOOsashi.",
                UdderAchievementId.DejaMoo => "Cheese it 1000 times.",
                UdderAchievementId.ButterChicken => "Have 1000 chickens slip on butter.",
                UdderAchievementId.TheScumAlsoRises => "Pollute ponds 100 times with spoiled milk.",
                UdderAchievementId.TheyCalledHimFlipper => "Kill 100 dolphins.",
                _ => string.Empty,
            };
        }

        public static int GetTarget(UdderAchievementId achievement)
        {
            return achievement switch
            {
                UdderAchievementId.MuensterHunter => 4,
                UdderAchievementId.EverybodysHerdAboutTheBird => 1000,
                UdderAchievementId.DejaMoo => 1000,
                UdderAchievementId.ButterChicken => 1000,
                UdderAchievementId.TheScumAlsoRises => 100,
                UdderAchievementId.TheyCalledHimFlipper => 100,
                _ => 1,
            };
        }
    }
}
