using PixelBattleText;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
            game.butterSprite = GetSolidSprite();
            game.dolphinSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Fishing/Singles/70_Mammal_Dolphin.png");
            game.seaUrchinSprite = LoadFirstSprite("Assets/Admurin's Pixel Items/PixelItems/Fishing/Singles/114_Echinodermata_SeaUrchin.png");
            game.damageText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_damage.asset");
            game.critText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_crit.asset");
            game.healText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_healing.asset");
            game.acidText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_pyro.asset");
            game.poisonText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_venom.asset");
            game.koText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_KO.asset");
            game.levelText = LoadAsset<TextAnimation>("Assets/PixelBattleText/Animation Presets/textAnim_lvlUp.asset");

            var streamer = gameObject.AddComponent<UdderWorldStreamer>();
            streamer.target = playerObject.transform;
            streamer.game = game;
            streamer.grassSprite = LoadSprite("Assets/Farming Asset Pack/farming-tileset.png", "farming-tileset_0");
            streamer.waterSprite = LoadSprite("Assets/Farming Asset Pack/farming-water.png", "farming-water_0");
            streamer.barnSprite = LoadSprite("Assets/Farming Asset Pack/farming-houses.png", "farming-houses_0");

            var hud = gameObject.AddComponent<UdderHud>();
            hud.game = game;
            hud.player = player;
            hud.font = LoadAsset<TMP_FontAsset>("Assets/PixelBattleText/Fonts/Alphapix.asset");

            BuildBattleTextCanvas();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/SampleScene.unity");
            AssetDatabase.SaveAssets();
            Debug.Log("Udder Destruction prototype scene built. Press Play and try not to become pasture prime.");
        }

        private static void BuildArena()
        {
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-cow.png");
            ConfigurePixelSprite("Assets/UdderDestruction/MiyamotoMoosashiCow.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-pig.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-chicken.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-tileset.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-water.png");
            ConfigurePixelSprite("Assets/Farming Asset Pack/farming-houses.png");

            Sprite tile = LoadSprite("Assets/Farming Asset Pack/farming-tileset.png", "farming-tileset_0");
            Sprite water = LoadSprite("Assets/Farming Asset Pack/farming-water.png", "farming-water_0");
            Sprite house = LoadSprite("Assets/Farming Asset Pack/farming-houses.png", "farming-houses_0");
            Sprite solid = GetSolidSprite();

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

        private static Sprite GetSolidSprite()
        {
            const string path = "Assets/UdderDestruction/SolidPixel.png";
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
            importer.spritePixelsPerUnit = 1f;
            importer.filterMode = FilterMode.Point;
            importer.SaveAndReimport();

            return LoadFirstSprite(path);
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
