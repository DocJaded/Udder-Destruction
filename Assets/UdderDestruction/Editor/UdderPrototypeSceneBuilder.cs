using PixelBattleText;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.LowLevel;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UdderDestruction;

namespace UdderDestruction.Editor
{
    public static class UdderPrototypeSceneBuilder
    {
        [MenuItem("Udder Destruction/Build Playable Prototype Scene")]
        public static void Build()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "SampleScene";

            Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.orthographic = true;
            camera.orthographicSize = 5.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.37f, 0.58f, 0.28f);
            RenderSettings.skybox = null;

            GameObject light = new GameObject("Directional Light");
            var sun = light.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            BuildArena();

            GameObject playerObject = new GameObject("Moolissa, Last Cow Standing");
            playerObject.transform.position = Vector3.zero;
            playerObject.transform.localScale = Vector3.one * 2.45f;
            var playerRenderer = playerObject.AddComponent<SpriteRenderer>();
            playerRenderer.sprite = LoadSprite("Assets/Farming Asset Pack/farming-cow.png", "farming-cow_4");
            playerRenderer.sortingOrder = 5;
            var playerBody = playerObject.AddComponent<Rigidbody2D>();
            playerBody.gravityScale = 0f;
            playerBody.freezeRotation = true;
            playerBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var playerCollider = playerObject.AddComponent<CircleCollider2D>();
            playerCollider.radius = 0.24f;
            var player = playerObject.AddComponent<UdderPlayer>();
            player.downSprite = LoadSprite("Assets/Farming Asset Pack/farming-cow.png", "farming-cow_1");
            player.sideSprite = LoadSprite("Assets/Farming Asset Pack/farming-cow.png", "farming-cow_4");
            player.upSprite = LoadSprite("Assets/Farming Asset Pack/farming-cow.png", "farming-cow_6");
            var cameraFollow = camera.gameObject.AddComponent<UdderCameraFollow>();
            cameraFollow.target = playerObject.transform;

            GameObject gameObject = new GameObject("Udder Destruction Game");
            var game = gameObject.AddComponent<UdderGameController>();
            game.player = player;
            game.worldCamera = camera;
            game.waterCenter = new Vector2(-6.5f, -3.5f);
            game.waterRadii = new Vector2(1.35f, 1.35f);
            game.cowSprite = playerRenderer.sprite;
            game.cowDownSprite = player.downSprite;
            game.cowSideSprite = player.sideSprite;
            game.cowUpSprite = player.upSprite;
            game.bossCowSprite = LoadSprite("Assets/UdderDestruction/MiyamotoMoosashiCow.png", "farming-cow_4");
            game.bossCowDownSprite = LoadSprite("Assets/UdderDestruction/MiyamotoMoosashiCow.png", "farming-cow_1");
            game.bossCowSideSprite = LoadSprite("Assets/UdderDestruction/MiyamotoMoosashiCow.png", "farming-cow_4");
            game.bossCowUpSprite = LoadSprite("Assets/UdderDestruction/MiyamotoMoosashiCow.png", "farming-cow_6");
            game.pigSprite = LoadSprite("Assets/Farming Asset Pack/farming-pig.png", "farming-pig_4");
            game.pigDownSprite = LoadSprite("Assets/Farming Asset Pack/farming-pig.png", "farming-pig_1");
            game.pigSideSprite = LoadSprite("Assets/Farming Asset Pack/farming-pig.png", "farming-pig_4");
            game.pigUpSprite = LoadSprite("Assets/Farming Asset Pack/farming-pig.png", "farming-pig_6");
            game.chickenSprite = LoadSprite("Assets/Farming Asset Pack/farming-chicken.png", "farming-chicken_4");
            game.chickenDownSprite = LoadSprite("Assets/Farming Asset Pack/farming-chicken.png", "farming-chicken_1");
            game.chickenSideSprite = LoadSprite("Assets/Farming Asset Pack/farming-chicken.png", "farming-chicken_4");
            game.chickenUpSprite = LoadSprite("Assets/Farming Asset Pack/farming-chicken.png", "farming-chicken_6");
            game.cheeseSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Miscellaneous/Singles/54_Cotton.png");
            game.bottleSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Potions/Singles/02_Glass_Bottle_A.png");
            game.skullBottleSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Potions/Singles/100_Soul_Trapped_R.png");
            game.creamSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Potions/Singles/102_Essence_Health.png");
            ConfigureSinglePixelSprite("Assets/Farming Asset Pack/Butter_Puddle.png");
            game.butterSprite = LoadFirstSprite("Assets/Farming Asset Pack/Butter_Puddle.png");
            game.spoiledPuddleSprite = LoadFirstSprite("Assets/War/Slime Enemy - Pixel Art/Sprites/Idle/Green/Sprite Sheet - Green Idle.png");
            game.spoiledPuddleIdleController = LoadAsset<RuntimeAnimatorController>("Assets/War/Slime Enemy - Pixel Art/Animation/Idle/Green/Green Idle - Controller.controller");
            game.spoiledPuddleHurtController = LoadAsset<RuntimeAnimatorController>("Assets/War/Slime Enemy - Pixel Art/Animation/Hurt/Green/Green Hurt - Controller.controller");
            game.spoiledPuddleDeathController = LoadAsset<RuntimeAnimatorController>("Assets/War/Slime Enemy - Pixel Art/Animation/Death/Green/Green Death - Controller.controller");
            game.dairyAirSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Skills/Singles/26_Cloud_Element.png");
            game.minorMoonaSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Skills/Singles/151_Omnipotent.png");
            game.normalMoonaSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Skills/Singles/154_Omnipotent.png");
            game.remarkableMoonaSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Skills/Singles/155_Omnipotent.png");
            game.elysianMoonaSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Skills/Singles/156_Omnipotent.png");
            game.cranberrySprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Botany/Singles/03_Cranberry.png");
            game.strawberrySprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Botany/Singles/22_Strawberry.png");
            game.raspberrySprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Botany/Singles/30_Raspberry.png");
            game.blackberriesSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Botany/Singles/49_Blackberries.png");
            game.wholeMilkSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/14_Snail_Slime.png");
            game.buttermilkSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/General/Singles/123_MetalChunk_Gold.png");
            game.rawMilkSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/General/Singles/83_MetalChunk_Silver.png");
            game.rawMilkFlySprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/93_Fly.png");
            game.cottonDeathSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Miscellaneous/Singles/54_Cotton.png");
            game.skullDeathSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Miscellaneous/Singles/01_Skull_Human.png");
            game.prionAngrySprite = LoadSprite("Assets/2D Pixel Art Icons/2D Pixel Art Emotion  Icons/Sprite.png", "21_Very angry_C");
            game.prionProjectileSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/General/Singles/540_Platinum_Gear.png");
            game.dolphinSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Fishing/Singles/70_Mammal_Dolphin.png");
            game.seaUrchinSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Fishing/Singles/114_Echinodermata_SeaUrchin.png");
            game.beeatriceSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/21_Wasp.png");
            game.beeDroneSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/22_Bee_Drone.png");
            game.honeycombSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/30_Honeycomb.png");
            game.damageText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_damage.asset");
            game.critText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_crit.asset");
            game.healText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_healing.asset");
            game.acidText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_pyro.asset");
            game.poisonText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_venom.asset");
            game.koText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_KO.asset");
            game.levelText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_lvlUp.asset");
            ConfigureUiSprite("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Medium Set.png");
            ConfigureUiSprite("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium.png");
            ConfigureUiSprite("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium_Pressed.png");
            ConfigureUiSprite("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Small Set.png");
            Sprite woodenPanel = LoadAsset<Sprite>("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Medium Set.png");
            Sprite woodenButton = LoadAsset<Sprite>("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium.png");
            Sprite woodenButtonPressed = LoadAsset<Sprite>("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium_Pressed.png");
            Sprite woodenBar = LoadAsset<Sprite>("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Small Set.png");
            game.uiPanelSprite = woodenPanel;
            game.uiButtonSprite = woodenButton;
            game.uiButtonPressedSprite = woodenButtonPressed;

            var streamer = gameObject.AddComponent<UdderWorldStreamer>();
            streamer.target = playerObject.transform;
            streamer.game = game;
            streamer.grassSprite = LoadSprite("Assets/Farming Asset Pack/farming-tileset.png", "farming-tileset_0");
            streamer.waterSprite = LoadSprite("Assets/Farming Asset Pack/farming-water.png", "farming-water_0");
            streamer.barnSprite = LoadSprite("Assets/Farming Asset Pack/farming-houses.png", "farming-houses_0");
            streamer.flowerSprites = LoadFlowerSprites();

            TMP_FontAsset uiFont = GetWoodenTmpFontAsset();
            game.uiFont = uiFont;

            var hud = gameObject.AddComponent<UdderHud>();
            hud.game = game;
            hud.player = player;
            hud.font = uiFont;
            hud.panelSprite = woodenPanel;
            hud.buttonSprite = woodenButton;
            hud.buttonPressedSprite = woodenButtonPressed;
            hud.barSprite = woodenBar;
            BuildHudCanvas(hud, game);
            hud.ApplyWoodenSkin();
            BuildEventSystem();
            BuildRuntimeSpawnTemplates(game);

            BuildBattleTextCanvas();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SampleScene.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("Udder Destruction prototype scene built. Press Play and try not to become pasture prime.");
        }

        [MenuItem("Udder Destruction/Build Main Menu In Current Scene")]
        public static void BuildMainMenuInCurrentScene()
        {
            if (!EditorSceneManager.GetActiveScene().isLoaded || string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            UdderGameController game = Object.FindFirstObjectByType<UdderGameController>();
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (!game || !canvas)
            {
                Debug.LogError("Cannot build main menu: scene needs an UdderGameController and HUD Canvas.");
                return;
            }

            Transform existing = canvas.transform.Find("Main Menu Overlay");
            if (existing)
                Object.DestroyImmediate(existing.gameObject);

            game.uiPanelSprite = game.uiPanelSprite ? game.uiPanelSprite : LoadAsset<Sprite>("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/UI board Medium Set.png");
            game.uiButtonSprite = game.uiButtonSprite ? game.uiButtonSprite : LoadAsset<Sprite>("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium.png");
            game.uiButtonPressedSprite = game.uiButtonPressedSprite ? game.uiButtonPressedSprite : LoadAsset<Sprite>("Assets/Fantasy Wooden GUI  Free/normal_ui_set A/TextBTN_Medium_Pressed.png");
            if (!game.uiFont)
                game.uiFont = GetWoodenTmpFontAsset();

            BuildMainMenu(canvas.transform, game);
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Main menu canvas objects built in the current scene.");
        }

        [MenuItem("Udder Destruction/Build Gameplay Prefabs In Current Scene")]
        public static void BuildGameplayPrefabsInCurrentScene()
        {
            if (!EditorSceneManager.GetActiveScene().isLoaded || string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            UdderGameController game = Object.FindFirstObjectByType<UdderGameController>();
            UdderWorldStreamer streamer = Object.FindFirstObjectByType<UdderWorldStreamer>();
            if (!game)
            {
                Debug.LogError("Cannot build gameplay prefabs: scene needs an UdderGameController.");
                return;
            }

            const string folder = "Assets/UdderDestruction/Prefabs";
            Directory.CreateDirectory(folder);

            game.chickenEnemyPrefab = SaveEnemyPrefab($"{folder}/Debt Chicken.prefab", game.chickenSprite, game.chickenDownSprite, game.chickenSideSprite, game.chickenUpSprite, game.rawMilkFlySprite, game.cottonDeathSprite, game.skullDeathSprite, game.prionAngrySprite, false, 0.12f, Vector2.zero, 4);
            game.pigEnemyPrefab = SaveEnemyPrefab($"{folder}/Hostile Ham.prefab", game.pigSprite, game.pigDownSprite, game.pigSideSprite, game.pigUpSprite, game.rawMilkFlySprite, game.cottonDeathSprite, game.skullDeathSprite, game.prionAngrySprite, false, 0.13f, new Vector2(0f, -0.02f), 4);
            game.bossEnemyPrefab = SaveEnemyPrefab($"{folder}/Miyamoto Moosashi.prefab", game.bossCowSprite ? game.bossCowSprite : game.cowSprite, game.bossCowDownSprite, game.bossCowSideSprite, game.bossCowUpSprite, game.rawMilkFlySprite, game.cottonDeathSprite, game.skullDeathSprite, game.prionAngrySprite, true, 0.166f, Vector2.zero, 5);
            game.beeatriceBossPrefab = ConfigureBeeatricePrefab(SaveEnemyPrefab($"{folder}/BEEatrice.prefab", game.beeatriceSprite, game.beeatriceSprite, game.beeatriceSprite, game.beeatriceSprite, game.rawMilkFlySprite, game.cottonDeathSprite, game.skullDeathSprite, game.prionAngrySprite, true, 0.166f, Vector2.zero, 5));
            game.beeDronePrefab = ConfigureBeeDronePrefab(SaveEnemyPrefab($"{folder}/BEEatrice Drone.prefab", game.beeDroneSprite, game.beeDroneSprite, game.beeDroneSprite, game.beeDroneSprite, game.rawMilkFlySprite, game.cottonDeathSprite, game.skullDeathSprite, game.prionAngrySprite, false, 0.12f, Vector2.zero, 5));
            game.wholeMilkProjectilePrefab = SaveProjectilePrefab($"{folder}/Whole Milk Shot.prefab", game.wholeMilkSprite ? game.wholeMilkSprite : game.bottleSprite, Color.white);
            game.buttermilkProjectilePrefab = SaveProjectilePrefab($"{folder}/Buttermilk Shot.prefab", game.buttermilkSprite ? game.buttermilkSprite : game.bottleSprite, Color.white);
            game.spoiledMilkProjectilePrefab = SaveProjectilePrefab($"{folder}/Spoiled Milk Shot.prefab", game.bottleSprite, new Color(0.55f, 1f, 0.45f));
            game.rawMilkProjectilePrefab = SaveProjectilePrefab($"{folder}/Raw Milk Shot.prefab", game.rawMilkSprite ? game.rawMilkSprite : game.bottleSprite, Color.white);
            game.prionProjectilePrefab = SavePrionProjectilePrefab($"{folder}/Prion Infection Gear.prefab", game.prionProjectileSprite);
            game.butterSlickPrefab = SaveButterSlickPrefab($"{folder}/Weaponized Butter Slick.prefab");
            game.butterTilePrefab = SaveSpritePrefab($"{folder}/Butter Tile.prefab", "Butter Tile", game.butterSprite, Color.white, Vector3.one, -1);
            game.dairyAirCloudPrefab = SaveDairyAirCloudPrefab($"{folder}/Dairy Air Cloud.prefab", GetSolidSprite("Assets/UdderDestruction/SolidPixel.png"));
            game.dairyAirPuffPrefab = SaveSpritePrefab($"{folder}/Dairy Air Puff.prefab", "Dairy Air Puff", game.dairyAirSprite, new Color(0.92f, 0.96f, 1f, 0.42f), Vector3.one * 0.72f, 3);
            game.spoiledMilkPuddlePrefab = SaveSpoiledPuddlePrefab($"{folder}/Spoiled Milk Puddle.prefab", game);
            game.dolphinPrefab = SaveDolphinPrefab($"{folder}/Pond Dolphin.prefab", game);
            game.seaUrchinPrefab = SaveSeaUrchinPrefab($"{folder}/Hostile Sea Urchin.prefab", game);
            game.pickupPrefab = SavePickupPrefab($"{folder}/Pickup.prefab", game.creamSprite ? game.creamSprite : game.cheeseSprite);
            game.rawMilkFlyPrefab = SaveSpritePrefab($"{folder}/Raw Milk Fly.prefab", "Raw Milk Fly", game.rawMilkFlySprite, Color.white, Vector3.one * 0.25f, 7);
            game.prionIndicatorPrefab = SaveSpritePrefab($"{folder}/Prion Anger Indicator.prefab", "Prion Anger Indicator", game.prionAngrySprite, Color.white, Vector3.one, 8);

            if (game.player)
                PrefabUtility.SaveAsPrefabAsset(game.player.gameObject, $"{folder}/Moolissa Player.prefab");

            if (streamer)
            {
                streamer.grassTilePrefab = SaveSpritePrefab($"{folder}/Grass Tile.prefab", "Grass Tile", streamer.grassSprite, new Color(0.5f, 0.76f, 0.32f), Vector3.one * streamer.tileScale, -5);
                streamer.pondTilePrefab = SavePondTilePrefab($"{folder}/Pond Tile.prefab", streamer.waterSprite);
                streamer.barnPrefab = SaveSpritePrefab($"{folder}/Barn Prop.prefab", "Barn Prop", streamer.barnSprite, Color.white, Vector3.one * 1.8f, -2);
                EditorUtility.SetDirty(streamer);
            }

            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Gameplay prefabs built and assigned in the current scene.");
        }

        [MenuItem("Udder Destruction/Build Prion Infection Projectile")]
        public static void BuildPrionInfectionProjectile()
        {
            if (!EditorSceneManager.GetActiveScene().isLoaded || string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            UdderGameController game = Object.FindFirstObjectByType<UdderGameController>();
            if (!game)
            {
                Debug.LogError("Cannot build Prion Infection projectile: scene needs an UdderGameController.");
                return;
            }

            const string folder = "Assets/UdderDestruction/Prefabs";
            Directory.CreateDirectory(folder);
            game.prionProjectileSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/General/Singles/540_Platinum_Gear.png");
            game.prionProjectilePrefab = SavePrionProjectilePrefab($"{folder}/Prion Infection Gear.prefab", game.prionProjectileSprite);
            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("Prion Infection projectile prefab built and assigned.");
        }

        [MenuItem("Udder Destruction/Assign Flower Sprites")]
        public static void AssignFlowerSprites()
        {
            if (!EditorSceneManager.GetActiveScene().isLoaded || string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            UdderWorldStreamer streamer = Object.FindFirstObjectByType<UdderWorldStreamer>();
            if (!streamer)
            {
                Debug.LogError("Cannot assign flower sprites: scene needs an UdderWorldStreamer.");
                return;
            }

            streamer.flowerSprites = LoadFlowerSprites();
            EditorUtility.SetDirty(streamer);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log($"Assigned {streamer.flowerSprites.Length} cosmetic flower sprites.");
        }

        [MenuItem("Udder Destruction/Build BEEatrice Assets")]
        public static void BuildBeeatriceAssets()
        {
            if (!EditorSceneManager.GetActiveScene().isLoaded || string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
                EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);

            UdderGameController game = Object.FindFirstObjectByType<UdderGameController>();
            if (!game)
            {
                Debug.LogError("Cannot build BEEatrice assets: scene needs an UdderGameController.");
                return;
            }

            const string folder = "Assets/UdderDestruction/Prefabs";
            Directory.CreateDirectory(folder);
            game.beeatriceSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/21_Wasp.png");
            game.beeDroneSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/22_Bee_Drone.png");
            game.honeycombSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Insects/Singles/30_Honeycomb.png");
            game.beeatriceBossPrefab = ConfigureBeeatricePrefab(SaveEnemyPrefab($"{folder}/BEEatrice.prefab", game.beeatriceSprite, game.beeatriceSprite, game.beeatriceSprite, game.beeatriceSprite, game.rawMilkFlySprite, game.cottonDeathSprite, game.skullDeathSprite, game.prionAngrySprite, true, 0.166f, Vector2.zero, 5));
            game.beeDronePrefab = ConfigureBeeDronePrefab(SaveEnemyPrefab($"{folder}/BEEatrice Drone.prefab", game.beeDroneSprite, game.beeDroneSprite, game.beeDroneSprite, game.beeDroneSprite, game.rawMilkFlySprite, game.cottonDeathSprite, game.skullDeathSprite, game.prionAngrySprite, false, 0.12f, Vector2.zero, 5));

            EditorUtility.SetDirty(game);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            Debug.Log("BEEatrice sprites and prefabs built and assigned.");
        }

        private static Sprite[] LoadFlowerSprites()
        {
            const string folder = "Assets/Admurin's Pixel Items/PixelItems/Flowers/Singles";
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folder });
            var paths = new List<string>(guids.Length);
            foreach (string guid in guids)
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));

            paths.Sort(System.StringComparer.Ordinal);
            var sprites = new List<Sprite>(paths.Count);
            foreach (string path in paths)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite)
                    sprites.Add(sprite);
            }

            return sprites.ToArray();
        }

        private static GameObject ConfigureBeeatricePrefab(GameObject prefab)
        {
            var enemy = prefab.GetComponent<UdderEnemy>();
            ScaleSpriteToHeight(prefab.transform, prefab.GetComponent<SpriteRenderer>().sprite, 1.17f);
            enemy.maxHealth = 480f;
            enemy.speed = 1.55f;
            enemy.contactDamage = 16f;
            enemy.creamValue = 30;
            enemy.enemyKind = UdderEnemyKind.Bee;
            enemy.bossType = UdderBossType.Beeatrice;
            enemy.isFlying = true;
            if (!prefab.GetComponent<UdderBeeSwarmController>())
                prefab.AddComponent<UdderBeeSwarmController>();
            PrefabUtility.SavePrefabAsset(prefab);
            return prefab;
        }

        private static GameObject ConfigureBeeDronePrefab(GameObject prefab)
        {
            var enemy = prefab.GetComponent<UdderEnemy>();
            ScaleSpriteToHeight(prefab.transform, prefab.GetComponent<SpriteRenderer>().sprite, 0.39f);
            enemy.maxHealth = 12f;
            enemy.speed = 1.55f;
            enemy.contactDamage = 7f;
            enemy.creamValue = 1;
            enemy.enemyKind = UdderEnemyKind.Bee;
            enemy.isFlying = true;
            if (!prefab.GetComponent<UdderBeeDrone>())
                prefab.AddComponent<UdderBeeDrone>();
            PrefabUtility.SavePrefabAsset(prefab);
            return prefab;
        }

        private static GameObject SaveProjectilePrefab(string path, Sprite sprite, Color color)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), sprite, color, Vector3.one * 1.8f, 5);
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            var body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            prefab.AddComponent<UdderProjectile>();
            return SavePrefab(path, prefab);
        }

        private static GameObject SavePrionProjectilePrefab(string path, Sprite sprite)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), sprite, Color.white, Vector3.one * 1.8f, 5);
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            var body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            var projectile = prefab.AddComponent<UdderProjectile>();
            projectile.mode = MilkMode.Prion;
            projectile.speed = 5.5f;
            projectile.life = 2f;
            projectile.spinDegreesPerSecond = 240f;
            projectile.homingStrength = 7f;
            return SavePrefab(path, prefab);
        }

        private static GameObject SaveEnemyPrefab(string path, Sprite sprite, Sprite down, Sprite side, Sprite up, Sprite fly, Sprite cotton, Sprite skull, Sprite prion, bool boss, float radius, Vector2 offset, int sortingOrder)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), sprite, Color.white, Vector3.one, sortingOrder);
            var body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.radius = radius;
            collider.offset = offset;
            var enemy = prefab.AddComponent<UdderEnemy>();
            enemy.downSprite = down;
            enemy.sideSprite = side;
            enemy.upSprite = up;
            enemy.rawMilkFlySprite = fly;
            enemy.cottonDeathSprite = cotton;
            enemy.skullDeathSprite = skull;
            enemy.prionAngrySprite = prion;
            enemy.avoidsWater = true;
            enemy.IsBoss = boss;
            return SavePrefab(path, prefab);
        }

        private static GameObject SaveButterSlickPrefab(string path)
        {
            GameObject prefab = new(Path.GetFileNameWithoutExtension(path));
            var collider = prefab.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;
            prefab.AddComponent<UdderButterSlick>();
            return SavePrefab(path, prefab);
        }

        private static GameObject SaveDairyAirCloudPrefab(string path, Sprite solidSprite)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), solidSprite, new Color(1f, 1f, 1f, 0f), Vector3.one, 3);
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.55f;
            prefab.AddComponent<UdderDairyAirCloud>();
            return SavePrefab(path, prefab);
        }

        private static GameObject SaveSpoiledPuddlePrefab(string path, UdderGameController game)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), game.spoiledPuddleSprite ? game.spoiledPuddleSprite : game.wholeMilkSprite, new Color(1f, 0.92f, 0.25f, 0.82f), Vector3.one * 1.7f, 1);
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.75f;
            if (game.spoiledPuddleIdleController)
            {
                var animator = prefab.AddComponent<Animator>();
                animator.runtimeAnimatorController = game.spoiledPuddleIdleController;
            }
            prefab.AddComponent<UdderHazardPool>();
            return SavePrefab(path, prefab);
        }

        private static GameObject SaveDolphinPrefab(string path, UdderGameController game)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), game.dolphinSprite, Color.white, Vector3.one, 4);
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.32f;
            var body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            var surface = prefab.AddComponent<UdderDolphinSurface>();
            surface.cottonDeathSprite = game.cottonDeathSprite;
            surface.skullDeathSprite = game.skullDeathSprite;
            return SavePrefab(path, prefab);
        }

        private static GameObject SaveSeaUrchinPrefab(string path, UdderGameController game)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), game.seaUrchinSprite, Color.white, Vector3.one, 6);
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.18f;
            var body = prefab.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            prefab.AddComponent<UdderSeaUrchin>();
            return SavePrefab(path, prefab);
        }

        private static GameObject SavePickupPrefab(string path, Sprite sprite)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), sprite, Color.white, Vector3.one * 1.8f, 6);
            var collider = prefab.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            prefab.AddComponent<UdderPickup>();
            return SavePrefab(path, prefab);
        }

        private static GameObject SavePondTilePrefab(string path, Sprite sprite)
        {
            GameObject prefab = CreateSpriteObject(Path.GetFileNameWithoutExtension(path), sprite, new Color(0.7f, 0.95f, 1f), Vector3.one * 4f, -2);
            if (sprite)
            {
                var collider = prefab.AddComponent<BoxCollider2D>();
                collider.size = sprite.bounds.size;
                collider.offset = sprite.bounds.center;
            }
            return SavePrefab(path, prefab);
        }

        private static GameObject SaveSpritePrefab(string path, string name, Sprite sprite, Color color, Vector3 scale, int sortingOrder)
        {
            return SavePrefab(path, CreateSpriteObject(name, sprite, color, scale, sortingOrder));
        }

        private static GameObject CreateSpriteObject(string name, Sprite sprite, Color color, Vector3 scale, int sortingOrder)
        {
            GameObject prefab = new(name);
            prefab.transform.localScale = scale;
            var renderer = prefab.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return prefab;
        }

        private static GameObject SavePrefab(string path, GameObject prefab)
        {
            GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefab, path);
            Object.DestroyImmediate(prefab);
            return saved;
        }

        private static void BuildArena()
        {
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-cow.png");
            ConfigurePixelSprite("Assets/UdderDestruction/MiyamotoMoosashiCow.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-pig.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-chicken.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-tileset.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-water.png");
            ConfigureSinglePixelSprite("Assets/Farming Asset Pack/Butter_Puddle.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-houses.png");

            Sprite tile = LoadSprite("Assets/Farming Asset Pack/farming-tileset.png", "farming-tileset_0");
            Sprite water = LoadSprite("Assets/Farming Asset Pack/farming-water.png", "farming-water_0");
            Sprite house = LoadSprite("Assets/Farming Asset Pack/farming-houses.png", "farming-houses_0");
            Sprite solid = GetSolidSprite("Assets/UdderDestruction/SolidPixel.png");

            GameObject baseObject = new GameObject("Continuous Pasture Base");
            baseObject.transform.position = new Vector3(0f, 0f, 1f);
            baseObject.transform.localScale = new Vector3(28f, 18f, 1f);
            var baseRenderer = baseObject.AddComponent<SpriteRenderer>();
            baseRenderer.sprite = solid;
            baseRenderer.color = new Color(0.42f, 0.68f, 0.31f);
            baseRenderer.sortingOrder = -10;

            // Runtime terrain is streamed around the cow by UdderWorldStreamer.
        }

        private static void CreateWaterPatch(Sprite sprite, Vector3 center, Color color, float scale)
        {
            if (!sprite)
                return;

            Vector2 spacing = sprite.bounds.size * scale;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    Vector3 position = center + new Vector3(x * spacing.x * 0.5f, y * spacing.y * 0.5f, 0f);
                    CreateProp("Questionable Pond Tile", sprite, position, color, scale);
                }
            }
        }

        private static void CreateProp(string name, Sprite sprite, Vector3 position, Color color, float scale)
        {
            if (!sprite)
                return;

            GameObject prop = new GameObject(name);
            prop.transform.position = position;
            prop.transform.localScale = Vector3.one * scale;
            var renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = -2;
        }

        private static void BuildBattleTextCanvas()
        {
            GameObject canvasObject = new GameObject("Pixel Battle Text Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            var controller = canvasObject.AddComponent<PixelBattleTextController>();
            controller.canvas = canvasObject.GetComponent<RectTransform>();
            controller.snapToPixelGrid = true;
        }

        private static void BuildEventSystem()
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            var inputModule = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        private static void BuildHudCanvas(UdderHud hud, UdderGameController game)
        {
            GameObject canvasObject = new("Udder HUD Canvas");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            canvasObject.AddComponent<GraphicRaycaster>();

            Image healthFill = CreateHudBar(canvasObject.transform, "HEALTH", new Vector2(22f, -22f), new Color(0.9f, 0.04f, 0.04f), 1f);
            Image bovinityFill = CreateHudBar(canvasObject.transform, "BOVINITY", new Vector2(22f, -58f), new Color(1f, 0.86f, 0.08f), 0f);
            TMP_Text statusText = CreateHudText(canvasObject.transform, "MOOLISSA:    WAVE 1        LVL 1         DD 0", new Vector2(22f, -98f), 18, TextAlignmentOptions.Left);
            TMP_Text hintText = CreateHudText(canvasObject.transform, "WASD/ARROWS MOVE. ATTACKS FIRE ON THEIR OWN TIMERS.", new Vector2(22f, 24f), 15, TextAlignmentOptions.Left);
            hintText.rectTransform.anchorMin = new Vector2(0f, 0f);
            hintText.rectTransform.anchorMax = new Vector2(0f, 0f);
            hintText.rectTransform.pivot = new Vector2(0f, 0f);

            GameObject powerChoicePanel = BuildPowerChoicePanel(canvasObject.transform, out List<Button> powerChoiceButtons);
            hud.BindInspectableElements(healthFill, bovinityFill, statusText, hintText, powerChoicePanel, powerChoiceButtons);
            BuildMainMenu(canvasObject.transform, game);
        }

        private static void BuildMainMenu(Transform parent, UdderGameController game)
        {
            GameObject overlay = new("Main Menu Overlay");
            overlay.transform.SetParent(parent, false);
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.pivot = new Vector2(0.5f, 0.5f);
            overlayRect.anchoredPosition = Vector2.zero;
            overlayRect.sizeDelta = Vector2.zero;
            overlay.AddComponent<CanvasGroup>();

            GameObject panel = new("Main Menu Panel");
            panel.transform.SetParent(overlay.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(560f, 560f);
            var panelImage = panel.AddComponent<Image>();
            panelImage.sprite = game.uiPanelSprite;
            panelImage.type = game.uiPanelSprite ? Image.Type.Sliced : Image.Type.Simple;
            panelImage.color = game.uiPanelSprite ? new Color(1f, 1f, 1f, 0.96f) : new Color(0f, 0f, 0f, 0.79f);

            TMP_Text title = CreateHudText(panel.transform, "Udder Destruction", new Vector2(0f, 190f), 46, TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = new Vector2(0f, 0.5f);
            title.rectTransform.anchorMax = new Vector2(1f, 0.5f);
            title.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            title.rectTransform.sizeDelta = new Vector2(0f, 70f);
            title.color = Color.white;

            game.mainMenuOverlay = overlay;
            game.startGameButton = CreateMainMenuButton(panel.transform, "Start Game", new Vector2(0f, 140f), true, game.uiButtonSprite, game.uiButtonPressedSprite);
            game.soundSettingsButton = CreateMainMenuButton(panel.transform, "Sound Settings", new Vector2(0f, 70f), false, game.uiButtonSprite, game.uiButtonPressedSprite);
            game.infoButton = CreateMainMenuButton(panel.transform, "Info", new Vector2(0f, 0f), false, game.uiButtonSprite, game.uiButtonPressedSprite);
            game.aboutButton = CreateMainMenuButton(panel.transform, "About", new Vector2(0f, -70f), false, game.uiButtonSprite, game.uiButtonPressedSprite);
            game.exitGameButton = CreateMainMenuButton(panel.transform, "Exit Game", new Vector2(0f, -140f), true, game.uiButtonSprite, game.uiButtonPressedSprite);
        }

        private static Button CreateMainMenuButton(Transform parent, string labelText, Vector2 anchoredPosition, bool interactable, Sprite buttonSprite, Sprite pressedSprite)
        {
            GameObject buttonObject = new("Menu " + labelText);
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360f, 52f);

            var image = buttonObject.AddComponent<Image>();
            image.sprite = buttonSprite;
            image.type = buttonSprite ? Image.Type.Sliced : Image.Type.Simple;
            image.color = buttonSprite ? new Color(1f, 1f, 1f, interactable ? 1f : 0.58f) : new Color(0f, 0f, 0f, interactable ? 0.66f : 0.38f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.interactable = interactable;
            ColorBlock colors = button.colors;
            colors.normalColor = interactable ? Color.white : new Color(0.62f, 0.62f, 0.62f, 1f);
            colors.highlightedColor = new Color(1.08f, 1.02f, 0.88f, 1f);
            colors.pressedColor = new Color(0.82f, 0.74f, 0.58f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            if (pressedSprite)
            {
                SpriteState state = button.spriteState;
                state.pressedSprite = pressedSprite;
                state.selectedSprite = buttonSprite ? buttonSprite : image.sprite;
                button.spriteState = state;
            }

            TMP_Text label = CreateHudText(buttonObject.transform, labelText, Vector2.zero, 26, TextAlignmentOptions.Center);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.color = interactable ? Color.white : new Color(0.72f, 0.72f, 0.72f);
            label.outlineWidth = 0f;
            return button;
        }

        private static void BuildRuntimeSpawnTemplates(UdderGameController game)
        {
            GameObject root = new("Runtime Spawn Templates");
            root.SetActive(false);

            CreateProjectileTemplate(root.transform, "Whole Milk Shot Template", game.wholeMilkSprite, Color.white, MilkMode.WholeMilk, 6f);
            CreateProjectileTemplate(root.transform, "Buttermilk Shot Template", game.buttermilkSprite, Color.white, MilkMode.Buttermilk, 9.9f);
            CreateProjectileTemplate(root.transform, "Spoiled Milk Shot Template", game.bottleSprite, new Color(0.55f, 1f, 0.45f), MilkMode.SpoiledMilk, 6f);
            CreateProjectileTemplate(root.transform, "Raw Milk Shot Template", game.rawMilkSprite, Color.white, MilkMode.RawMilk, 5.1f);

            GameObject butter = CreateTemplateSprite(root.transform, "Weaponized Butter Slick Template", game.butterSprite, Color.white, Vector3.one, -1);
            var butterCollider = butter.AddComponent<BoxCollider2D>();
            butterCollider.isTrigger = true;
            butterCollider.size = Vector2.one;
            butter.AddComponent<UdderButterSlick>();

            GameObject dairyAir = CreateTemplateSprite(root.transform, "Dairy Air Cloud Template", GetSolidSprite("Assets/UdderDestruction/SolidPixel.png"), new Color(1f, 1f, 1f, 0f), Vector3.one, 3);
            BuildDairyAirTemplateSprites(dairyAir.transform, game.dairyAirSprite);
            var dairyAirCollider = dairyAir.AddComponent<CircleCollider2D>();
            dairyAirCollider.isTrigger = true;
            dairyAirCollider.radius = 0.55f;
            dairyAir.AddComponent<UdderDairyAirCloud>();

            GameObject pool = CreateTemplateSprite(root.transform, "Spoiled Milk Puddle Template", game.spoiledPuddleSprite ? game.spoiledPuddleSprite : game.wholeMilkSprite, new Color(0.42f, 0.95f, 0.32f, 0.62f), Vector3.one * 1.7f, 1);
            var poolCollider = pool.AddComponent<CircleCollider2D>();
            poolCollider.isTrigger = true;
            poolCollider.radius = 0.75f;
            var poolComponent = pool.AddComponent<UdderHazardPool>();
            poolComponent.life = 3f;
            poolComponent.radius = poolCollider.radius;

            CreateEnemyTemplate(root.transform, "Hostile Ham Template", game, game.pigSprite, game.pigDownSprite, game.pigSideSprite, game.pigUpSprite, 0.78f, false);
            CreateEnemyTemplate(root.transform, "Debt Chicken Template", game, game.chickenSprite, game.chickenDownSprite, game.chickenSideSprite, game.chickenUpSprite, 0.39f, false);
            CreateEnemyTemplate(root.transform, "Miyamoto Moosashi Template", game, game.bossCowSprite, game.bossCowDownSprite, game.bossCowSideSprite, game.bossCowUpSprite, 1.17f, true);

            GameObject dolphin = CreateTemplateSprite(root.transform, "Pond Dolphin Template", game.dolphinSprite, Color.white, Vector3.one, 4);
            dolphin.AddComponent<UdderDolphinSurface>();

            GameObject urchin = CreateTemplateSprite(root.transform, "Hostile Sea Urchin Template", game.seaUrchinSprite, Color.white, Vector3.one, 6);
            var urchinCollider = urchin.AddComponent<CircleCollider2D>();
            urchinCollider.isTrigger = true;
            urchinCollider.radius = 0.18f;
            var urchinBody = urchin.AddComponent<Rigidbody2D>();
            urchinBody.gravityScale = 0f;
            urchinBody.bodyType = RigidbodyType2D.Kinematic;
            urchinBody.freezeRotation = true;
            urchin.AddComponent<UdderSeaUrchin>();

            CreateTemplateSprite(root.transform, "Raw Milk Fly Visual Template", game.rawMilkFlySprite, Color.white, Vector3.one * 0.25f, 7);
            CreateTemplateSprite(root.transform, "Death Cotton Visual Template", game.cottonDeathSprite, Color.white, Vector3.one, 5);
            CreateTemplateSprite(root.transform, "Death Skull Visual Template", game.skullDeathSprite, Color.white, Vector3.one, 5);

            CreatePickupTemplate(root.transform, "Minor MOOna Pickup Template", game.minorMoonaSprite, PickupType.MinorMoona, Color.white, 1);
            CreatePickupTemplate(root.transform, "Normal MOOna Pickup Template", game.normalMoonaSprite, PickupType.NormalMoona, Color.white, 2);
            CreatePickupTemplate(root.transform, "Remarkable MOOna Pickup Template", game.remarkableMoonaSprite, PickupType.RemarkableMoona, Color.white, 5);
            CreatePickupTemplate(root.transform, "Elysian MOOna Pickup Template", game.elysianMoonaSprite, PickupType.ElysianMoona, Color.white, 10);
            CreatePickupTemplate(root.transform, "Cranberry Heal Pickup Template", game.cranberrySprite, PickupType.Cranberry, Color.white, 1);
            CreatePickupTemplate(root.transform, "Strawberry Heal Pickup Template", game.strawberrySprite, PickupType.Strawberry, Color.white, 1);
            CreatePickupTemplate(root.transform, "Raspberry Heal Pickup Template", game.raspberrySprite, PickupType.Raspberry, Color.white, 1);
            CreatePickupTemplate(root.transform, "Blackberries Heal Pickup Template", game.blackberriesSprite, PickupType.Blackberries, Color.white, 1);
            CreatePickupTemplate(root.transform, "Dairy Double Pickup Template", game.cheeseSprite, PickupType.DairyDouble, new Color(1f, 0.92f, 0.25f), 1);
        }

        private static void CreateProjectileTemplate(Transform parent, string name, Sprite sprite, Color color, MilkMode mode, float damage)
        {
            GameObject projectile = CreateTemplateSprite(parent, name, sprite, color, Vector3.one * 1.8f, 5);
            var collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            var body = projectile.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            var component = projectile.AddComponent<UdderProjectile>();
            component.mode = mode;
            component.damage = damage;
        }

        private static void BuildDairyAirTemplateSprites(Transform parent, Sprite sprite)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    GameObject tile = new("Dairy Air Puff");
                    tile.transform.SetParent(parent, false);
                    tile.transform.localPosition = new Vector3(x * 0.34f, y * 0.34f, 0f);
                    tile.transform.localScale = Vector3.one * 0.72f;
                    var renderer = tile.AddComponent<SpriteRenderer>();
                    renderer.sprite = sprite;
                    renderer.color = new Color(0.92f, 0.96f, 1f, 0.42f);
                    renderer.sortingOrder = 3;
                }
            }
        }

        private static void CreateEnemyTemplate(Transform parent, string name, UdderGameController game, Sprite sprite, Sprite downSprite, Sprite sideSprite, Sprite upSprite, float height, bool boss)
        {
            GameObject enemyObject = CreateTemplateSprite(parent, name, sprite, Color.white, Vector3.one, boss ? 5 : 4);
            ScaleSpriteToHeight(enemyObject.transform, sprite, height);
            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = enemyObject.AddComponent<CircleCollider2D>();
            if (boss)
            {
                collider.radius = 0.166f;
            }
            else if (name.Contains("Ham"))
            {
                collider.radius = 0.13f;
                collider.offset = new Vector2(0f, -0.02f);
            }
            else
            {
                collider.radius = 0.12f;
                collider.offset = Vector2.zero;
            }
            var enemy = enemyObject.AddComponent<UdderEnemy>();
            enemy.downSprite = downSprite;
            enemy.sideSprite = sideSprite;
            enemy.upSprite = upSprite;
            enemy.rawMilkFlySprite = game.rawMilkFlySprite;
            enemy.cottonDeathSprite = game.cottonDeathSprite;
            enemy.skullDeathSprite = game.skullDeathSprite;
            enemy.avoidsWater = true;
            enemy.IsBoss = boss;
            enemy.maxHealth = boss ? 520f : 11f;
            enemy.speed = boss ? 1.55f : 1.75f;
            enemy.contactDamage = boss ? 16f : 8f;
            enemy.creamValue = boss ? 30 : 2;
        }

        private static void CreatePickupTemplate(Transform parent, string name, Sprite sprite, PickupType type, Color color, int amount)
        {
            GameObject pickupObject = CreateTemplateSprite(parent, name, sprite, color, Vector3.one * 1.8f, 6);
            var collider = pickupObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            var pickup = pickupObject.AddComponent<UdderPickup>();
            pickup.type = type;
            pickup.amount = amount;
        }

        private static GameObject CreateTemplateSprite(Transform parent, string name, Sprite sprite, Color color, Vector3 scale, int sortingOrder)
        {
            GameObject template = new(name);
            template.transform.SetParent(parent, false);
            template.transform.localScale = scale;
            var renderer = template.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite ? sprite : GetSolidSprite("Assets/UdderDestruction/SolidPixel.png");
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return template;
        }

        private static void ScaleSpriteToHeight(Transform target, Sprite sprite, float worldHeight)
        {
            if (!sprite || sprite.bounds.size.y <= 0f)
            {
                target.localScale = Vector3.one;
                return;
            }

            float scale = worldHeight / sprite.bounds.size.y;
            target.localScale = Vector3.one * scale;
        }

        private static GameObject BuildPowerChoicePanel(Transform parent, out List<Button> powerChoiceButtons)
        {
            GameObject panel = new("Bovinity Power Choices");
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(620f, 240f);

            var back = panel.AddComponent<Image>();
            back.color = new Color(0f, 0f, 0f, 0.82f);

            TMP_Text title = CreateHudText(panel.transform, "BOVINITY LEVEL UP", new Vector2(0f, -36f), 24, TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(0f, 38f);

            powerChoiceButtons = new List<Button>();
            for (int i = 0; i < 3; i++)
                powerChoiceButtons.Add(CreatePowerChoiceButton(panel.transform, new Vector2(-170f + i * 170f, -138f)));

            panel.SetActive(false);
            return panel;
        }

        private static Button CreatePowerChoiceButton(Transform parent, Vector2 anchoredPosition)
        {
            GameObject buttonObject = new("Power Choice");
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(148f, 72f);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(1f, 0.86f, 0.12f, 0.92f);

            var button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(1f, 0.96f, 0.45f, 1f);
            colors.pressedColor = new Color(0.92f, 0.62f, 0.08f, 1f);
            button.colors = colors;

            TMP_Text label = CreateHudText(buttonObject.transform, "POWER", new Vector2(0f, -18f), 15, TextAlignmentOptions.Center);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.rectTransform.anchoredPosition = Vector2.zero;
            label.rectTransform.sizeDelta = Vector2.zero;
            label.color = Color.black;
            label.outlineWidth = 0f;

            return button;
        }

        private static Image CreateHudBar(Transform parent, string label, Vector2 anchoredPosition, Color fillColor, float fillAmount)
        {
            GameObject back = new(label + " Back");
            back.transform.SetParent(parent, false);
            var backRect = back.AddComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0f, 1f);
            backRect.anchorMax = new Vector2(0f, 1f);
            backRect.pivot = new Vector2(0f, 1f);
            backRect.anchoredPosition = anchoredPosition;
            backRect.sizeDelta = new Vector2(300f, 30f);
            var backImage = back.AddComponent<Image>();
            backImage.color = new Color(0f, 0f, 0f, 0.72f);

            GameObject fill = new(label + " Fill");
            fill.transform.SetParent(back.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = new Vector2(0f, 0.5f);
            fillRect.offsetMin = new Vector2(95.55f, 9.61f);
            fillRect.offsetMax = new Vector2(-29.6f, -9.62f);
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.type = Image.Type.Simple;
            fillRect.localScale = new Vector3(fillAmount, 1f, 1f);

            TMP_Text text = CreateHudText(back.transform, label, new Vector2(34f, -7f), 13, TextAlignmentOptions.Left);
            text.rectTransform.anchorMin = new Vector2(0f, 1f);
            text.rectTransform.anchorMax = new Vector2(1f, 1f);
            text.rectTransform.sizeDelta = new Vector2(-68f, 22f);
            text.color = Color.black;
            return fillImage;
        }

        private static TMP_Text CreateHudText(Transform parent, string text, Vector2 anchoredPosition, int size, TextAlignmentOptions alignment)
        {
            GameObject textObject = new("HUD " + text);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(1120f, 34f);

            var tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.font = GetWoodenTmpFontAsset();
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.text = text;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.outlineWidth = 0.18f;
            tmp.outlineColor = Color.black;
            return tmp;
        }

        private static Sprite GetSolidSprite(string path)
        {
            Directory.CreateDirectory("Assets/UdderDestruction");

            if (!File.Exists(path))
            {
                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 1f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            return LoadFirstSprite(path);
        }

        private static TMP_FontAsset GetWoodenTmpFontAsset()
        {
            const string assetPath = "Assets/UdderDestruction/BMYEONSUNG_TMP.asset";
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing && existing.atlasTexture)
                return existing;
            if (existing)
                AssetDatabase.DeleteAsset(assetPath);

            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fantasy Wooden GUI  Free/BMYEONSUNG_ttf.ttf");
            if (!sourceFont)
                return LoadAsset<TMP_FontAsset>("Assets/PixelBattleText/Fonts/Alphapix.asset");

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                90,
                9,
                GlyphRenderMode.SDFAA,
                1024,
                1024,
                AtlasPopulationMode.Dynamic,
                true);
            fontAsset.name = "BMYEONSUNG_TMP";
            fontAsset.TryAddCharacters(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,!?'\"-+:/() %><",
                out _);
            AssetDatabase.CreateAsset(fontAsset, assetPath);
            if (fontAsset.material)
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            if (fontAsset.atlasTexture)
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            AssetDatabase.SaveAssets();
            return fontAsset;
        }

        private static Sprite LoadSprite(string path, string name)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite && sprite.name == name)
                    return sprite;
            }

            return LoadFirstSprite(path);
        }

        private static void ConfigurePixelSprite(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                return;

            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static void ConfigureSinglePixelSprite(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Standalone",
                overridden = true,
                maxTextureSize = 2048,
                format = TextureImporterFormat.RGBA32,
                textureCompression = TextureImporterCompression.Uncompressed
            });
            importer.SaveAndReimport();
        }

        private static void ConfigureUiSprite(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        private static Sprite LoadFirstSprite(string path)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                    return sprite;
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static T LoadAsset<T>(string path) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }
    }
}
