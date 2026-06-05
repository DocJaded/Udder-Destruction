using System.Collections;
using System.Collections.Generic;
using PixelBattleText;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace UdderDestruction
{
    public sealed class UdderGameController : MonoBehaviour
    {
        [Header("Scene References")]
        public UdderPlayer player;
        public Camera worldCamera;
        public Vector2 waterCenter = new(-6.5f, -3.5f);
        public Vector2 waterRadii = new(1.35f, 1.35f);

        [Header("Sprites")]
        public Sprite cowSprite;
        public Sprite cowDownSprite;
        public Sprite cowSideSprite;
        public Sprite cowUpSprite;
        public Sprite bossCowSprite;
        public Sprite bossCowDownSprite;
        public Sprite bossCowSideSprite;
        public Sprite bossCowUpSprite;
        public Sprite pigSprite;
        public Sprite pigDownSprite;
        public Sprite pigSideSprite;
        public Sprite pigUpSprite;
        public Sprite chickenSprite;
        public Sprite chickenDownSprite;
        public Sprite chickenSideSprite;
        public Sprite chickenUpSprite;
        public Sprite cheeseSprite;
        public Sprite bottleSprite;
        public Sprite skullBottleSprite;
        public Sprite creamSprite;
        public Sprite butterSprite;
        public Sprite spoiledPuddleSprite;
        public RuntimeAnimatorController spoiledPuddleIdleController;
        public RuntimeAnimatorController spoiledPuddleHurtController;
        public RuntimeAnimatorController spoiledPuddleDeathController;
        public Sprite dairyAirSprite;
        public Sprite minorMoonaSprite;
        public Sprite normalMoonaSprite;
        public Sprite remarkableMoonaSprite;
        public Sprite elysianMoonaSprite;
        public Sprite cranberrySprite;
        public Sprite strawberrySprite;
        public Sprite raspberrySprite;
        public Sprite blackberriesSprite;
        public Sprite wholeMilkSprite;
        public Sprite buttermilkSprite;
        public Sprite tresLechesSprite;
        public Sprite rawMilkSprite;
        public Sprite rawMilkFlySprite;
        public Sprite cottonDeathSprite;
        public Sprite skullDeathSprite;
        public Sprite prionAngrySprite;
        public Sprite prionProjectileSprite;
        public Sprite dolphinSprite;
        public Sprite seaUrchinSprite;
        public Sprite beeatriceSprite;
        public Sprite beeDroneSprite;
        public Sprite honeycombSprite;
        public Sprite milkshakeSprite;

        [Header("Battle Text")]
        public TextAnimation damageText;
        public TextAnimation critText;
        public TextAnimation healText;
        public TextAnimation acidText;
        public TextAnimation poisonText;
        public TextAnimation koText;
        public TextAnimation levelText;
        public TMP_FontAsset uiFont;

        [Header("Wooden UI")]
        public Sprite uiPanelSprite;
        public Sprite uiButtonSprite;
        public Sprite uiButtonPressedSprite;

        [Header("Main Menu")]
        public GameObject mainMenuOverlay;
        public Button startGameButton;
        public Button soundSettingsButton;
        public Button infoButton;
        public Button aboutButton;
        public Button exitGameButton;

        [Header("Game Rules")]
        public UdderGameMode gameMode = UdderGameMode.Standard;
        public bool testSpawnScene;

        [Header("Gameplay Prefabs")]
        public GameObject chickenEnemyPrefab;
        public GameObject pigEnemyPrefab;
        public GameObject bossEnemyPrefab;
        public GameObject beeatriceBossPrefab;
        public GameObject beeDronePrefab;
        public GameObject wholeMilkProjectilePrefab;
        public GameObject buttermilkProjectilePrefab;
        public GameObject spoiledMilkProjectilePrefab;
        public GameObject rawMilkProjectilePrefab;
        public GameObject prionProjectilePrefab;
        public GameObject butterSlickPrefab;
        public GameObject butterTilePrefab;
        public GameObject dairyAirCloudPrefab;
        public GameObject dairyAirPuffPrefab;
        public GameObject spoiledMilkPuddlePrefab;
        public GameObject dolphinPrefab;
        public GameObject seaUrchinPrefab;
        public GameObject pickupPrefab;
        public GameObject rawMilkFlyPrefab;
        public GameObject prionIndicatorPrefab;
        public GameObject milkshakePrefab;

        private readonly List<UdderEnemy> enemies = new();
        private readonly List<UdderSeaUrchin> seaUrchins = new();
        private readonly List<UdderAlliedCow> alliedCows = new();
        private readonly List<Vector2> waterBodyCenters = new();
        private readonly List<Vector2> waterBodyRadii = new();
        private readonly List<Vector2> contaminatedWaterCenters = new();
        private readonly List<TextReservation> textReservations = new();
        private readonly HashSet<UdderBossType> encounteredBosses = new();
        private float runTimer;
        private float nextWaveTimer = 1.2f;
        private float waveSpawnTimer;
        private float dolphinTimer = 5f;
        private int groundedSeaUrchins;
        private int enemiesLeftInWave;
        private int enemiesPendingSpawn;
        private int wave = 1;
        private int cream;
        private bool finished;
        private bool bossPending;
        private bool bossActive;
        private int bossWaveCompleted;
        private UdderBossType pendingBossType;
        private bool choosingPower;
        private int bankedDairyDoubles;
        private Sprite runtimeSolidSprite;
        private UdderHud hud;
        private GameObject gameOverOverlay;
        private GameObject pauseOverlay;
        private UdderDairyAirCloud carbonHoofprintAura;
        private UdderMilkshake activeMilkshake;
        private bool playerTookDamageThisWave;
        private bool spawningMilkshakeRush;
        private static TMP_FontAsset sharedUiFont;
        private bool mainMenuActive = true;
        private bool paused;

        public float CritChance => 0.11f + wave * 0.005f;
        public int Wave => wave;
        public int Cream => cream;
        public int GroundedSeaUrchins => groundedSeaUrchins;
        public int BankedDairyDoubles => bankedDairyDoubles;
        public string WaveStatusText => enemiesLeftInWave > 0 ? $"ENEMIES {enemiesLeftInWave}" : $"NEXT {Mathf.CeilToInt(nextWaveTimer)}S";
        public bool HasAutoAimTarget => HasLivingAutoAimTarget();

        public float GetMilkShotRange(MilkMode mode)
        {
            float speed = mode switch
            {
                MilkMode.SpoiledMilk => 6.2f,
                MilkMode.RawMilk => 5.8f,
                MilkMode.TresLeches => 7.4f,
                _ => 7.4f,
            };
            float life = mode switch
            {
                MilkMode.SpoiledMilk => 0.425f,
                MilkMode.RawMilk => 0.6f,
                MilkMode.TresLeches => 0.55f,
                _ => 0.55f,
            };
            return speed * life;
        }

        private void Start()
        {
            if (!worldCamera)
                worldCamera = Camera.main;

            hud = GetComponent<UdderHud>();
            sharedUiFont = uiFont ? uiFont : hud ? hud.font : null;

            if (player)
                player.Init(this);

            if (testSpawnScene)
            {
                if (mainMenuOverlay)
                    mainMenuOverlay.SetActive(false);
                mainMenuActive = false;
                nextWaveTimer = float.PositiveInfinity;
                return;
            }

            ShowMainMenu();
        }

        private void OnApplicationPause(bool pausedBySystem)
        {
            if (pausedBySystem)
                UdderPersistence.Flush();
        }

        private void OnApplicationQuit()
        {
            UdderPersistence.Flush();
        }

        private void Update()
        {
            if (mainMenuActive)
                return;

            if (!finished && !choosingPower && IsPausePressed())
                TogglePause();

            if (paused)
                return;

            if (finished || choosingPower || !player || !player.IsAlive)
                return;

            runTimer += Time.deltaTime;

            CleanupEnemyList();
            CleanupAlliedCows();
            CleanupSeaUrchins();
            RepositionStrandedEnemies();

            if (testSpawnScene)
                return;

            UpdateDolphin();
            UpdateWaveSpawning();

            if (bossPending)
            {
                nextWaveTimer -= Time.deltaTime;
                if (nextWaveTimer <= 0f)
                    SpawnBoss(pendingBossType);
                return;
            }

            if (!bossActive && enemiesPendingSpawn <= 0 && IsWaveCleared())
            {
                nextWaveTimer -= Time.deltaTime;
                if (nextWaveTimer <= 0f)
                    StartWave();
            }
        }

        public void FireMilk(Vector3 origin, Vector2 direction, MilkMode mode, float damage, int powerLevel = 1, int condensedMilkLevel = 0)
        {
            GameObject projectilePrefab = GetProjectilePrefab(mode);
            bool fromPrefab = projectilePrefab;
            GameObject shot = InstantiateOrCreate(projectilePrefab, mode + " Shot");
            shot.transform.position = origin + (Vector3)(direction.normalized * 0.9f);
            shot.transform.localScale = Vector3.one * 1.8f;
            var renderer = EnsureComponent<SpriteRenderer>(shot);
            renderer.sprite = GetProjectileSprite(mode);
            renderer.color = mode switch
            {
                MilkMode.SpoiledMilk => new Color(0.55f, 1f, 0.45f),
                MilkMode.TresLeches => new Color(1f, 0.96f, 0.72f),
                _ => Color.white,
            };
            renderer.sortingOrder = 5;

            var collider = EnsureComponent<CircleCollider2D>(shot);
            collider.isTrigger = true;
            if (!fromPrefab)
                collider.radius = 0.22f;
            var body = EnsureComponent<Rigidbody2D>(shot);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;

            var projectile = EnsureComponent<UdderProjectile>(shot);
            projectile.game = this;
            projectile.mode = mode;
            projectile.powerLevel = Mathf.Max(1, powerLevel);
            projectile.condensedMilkLevel = condensedMilkLevel;
            projectile.sleepDuration = mode == MilkMode.WholeMilk && player ? player.GetPastureBedtimeSleepDuration() : 0f;
            projectile.condensedMilkDamageMultiplier = mode == MilkMode.TresLeches ? 2f * (1f + Mathf.Max(0, powerLevel - 1) * 0.1f) : 1f;
            projectile.condensedMilkDurationMultiplier = mode == MilkMode.TresLeches ? 2f * (1f + Mathf.Max(0, powerLevel - 1) * 0.1f) : 1f;
            projectile.damage = mode switch
            {
                MilkMode.Buttermilk => damage * 1.15f,
                MilkMode.RawMilk => damage * 0.45f,
                MilkMode.TresLeches => damage,
                _ => damage,
            };
            projectile.speed = mode switch
            {
                MilkMode.SpoiledMilk => 6.2f,
                MilkMode.RawMilk => 5.8f,
                MilkMode.TresLeches => 7.4f,
                _ => 7.4f,
            };
            projectile.life = mode switch
            {
                MilkMode.SpoiledMilk => 0.425f,
                MilkMode.RawMilk => 0.6f,
                MilkMode.TresLeches => 0.55f,
                _ => 0.55f,
            };
            projectile.Fire(direction);
        }

        private GameObject GetProjectilePrefab(MilkMode mode)
        {
            return mode switch
            {
                MilkMode.WholeMilk => wholeMilkProjectilePrefab,
                MilkMode.Buttermilk => buttermilkProjectilePrefab,
                MilkMode.TresLeches => wholeMilkProjectilePrefab,
                MilkMode.SpoiledMilk => spoiledMilkProjectilePrefab,
                MilkMode.RawMilk => rawMilkProjectilePrefab,
                _ => wholeMilkProjectilePrefab,
            };
        }

        private Sprite GetProjectileSprite(MilkMode mode)
        {
            return mode switch
            {
                MilkMode.WholeMilk => wholeMilkSprite ? wholeMilkSprite : bottleSprite,
                MilkMode.Buttermilk => buttermilkSprite ? buttermilkSprite : bottleSprite,
                MilkMode.TresLeches => tresLechesSprite ? tresLechesSprite : wholeMilkSprite ? wholeMilkSprite : bottleSprite,
                MilkMode.RawMilk => rawMilkSprite ? rawMilkSprite : bottleSprite,
                _ => bottleSprite,
            };
        }

        public void DeployButter(Vector3 origin, int powerLevel)
        {
            float size = GetButterWorldSize(powerLevel);
            float life = GetButterLife(powerLevel);
            Vector3 position = new(Mathf.Round(origin.x), Mathf.Round(origin.y), 0f);
            GameObject slick = InstantiateOrCreate(butterSlickPrefab, "Weaponized Butter Slick");
            slick.transform.position = position;

            BuildButterVisual(slick.transform, size);

            var collider = EnsureComponent<BoxCollider2D>(slick);
            collider.isTrigger = true;
            Vector2 baseColliderSize = collider.size.sqrMagnitude > 0.01f ? collider.size : Vector2.one;
            collider.size = new Vector2(baseColliderSize.x * size, baseColliderSize.y * size);

            var butter = EnsureComponent<UdderButterSlick>(slick);
            butter.life = life;
        }

        private void BuildButterVisual(Transform parent, float size)
        {
            Sprite sprite = butterSprite ? butterSprite : GetRuntimeSolidSprite();
            Transform existingTile = parent.Find("Butter Tile");
            GameObject tile = existingTile ? existingTile.gameObject : InstantiateOrCreate(butterTilePrefab, "Butter Tile");
            tile.transform.SetParent(parent, false);
            tile.transform.localPosition = Vector3.zero;
            ScaleSpriteToWorldSize(tile.transform, sprite, Vector2.one * size);

            var tileRenderer = EnsureComponent<SpriteRenderer>(tile);
            tileRenderer.sprite = sprite;
            tileRenderer.color = Color.white;
            tileRenderer.sortingOrder = -1;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Transform child = parent.GetChild(i);
                if (child != tile.transform && child.name == "Butter Tile")
                    Destroy(child.gameObject);
            }
        }

        public void DeployDairyAir(Vector3 origin, int powerLevel, float interval)
        {
            bool fromPrefab = dairyAirCloudPrefab;
            GameObject cloud = InstantiateOrCreate(dairyAirCloudPrefab, "Dairy Air Cloud");
            cloud.transform.position = origin;

            var renderer = EnsureComponent<SpriteRenderer>(cloud);
            renderer.sprite = GetRuntimeSolidSprite();
            renderer.color = new Color(1f, 1f, 1f, 0f);
            renderer.sortingOrder = 3;

            var collider = EnsureComponent<CircleCollider2D>(cloud);
            collider.isTrigger = true;
            if (!fromPrefab)
                collider.radius = 0.55f;

            BuildDairyAirSprites(cloud.transform);
            var dairyAir = EnsureComponent<UdderDairyAirCloud>(cloud);
            dairyAir.life = GetDairyAirCloudLife(powerLevel, interval);
            dairyAir.permanent = false;
            dairyAir.followTarget = null;
            dairyAir.radiusMultiplier = 1f;
        }

        public void UpdateCarbonHoofprintAura(Transform target, int powerLevel)
        {
            if (!target)
                return;

            if (!carbonHoofprintAura)
            {
                bool fromPrefab = dairyAirCloudPrefab;
                GameObject cloud = InstantiateOrCreate(dairyAirCloudPrefab, "Carbon Hoofprint Aura");
                var renderer = EnsureComponent<SpriteRenderer>(cloud);
                renderer.sprite = GetRuntimeSolidSprite();
                renderer.color = new Color(1f, 1f, 1f, 0f);
                renderer.sortingOrder = 3;

                var collider = EnsureComponent<CircleCollider2D>(cloud);
                collider.isTrigger = true;
                if (!fromPrefab)
                    collider.radius = 0.55f;

                BuildDairyAirSprites(cloud.transform);
                carbonHoofprintAura = EnsureComponent<UdderDairyAirCloud>(cloud);
            }

            carbonHoofprintAura.transform.position = target.position;
            carbonHoofprintAura.permanent = true;
            carbonHoofprintAura.followTarget = target;
            carbonHoofprintAura.life = float.PositiveInfinity;
            carbonHoofprintAura.radiusMultiplier = 1f + Mathf.Max(0, powerLevel - 1) * 0.05f;
        }

        private void BuildDairyAirSprites(Transform parent)
        {
            Sprite sprite = dairyAirSprite ? dairyAirSprite : GetRuntimeSolidSprite();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    GameObject tile = InstantiateOrCreate(dairyAirPuffPrefab, "Dairy Air Puff");
                    tile.transform.SetParent(parent, false);
                    tile.transform.localPosition = new Vector3(x * 0.34f, y * 0.34f, 0f);
                    tile.transform.localScale = Vector3.one * 0.72f;
                    var tileRenderer = EnsureComponent<SpriteRenderer>(tile);
                    tileRenderer.sprite = sprite;
                    tileRenderer.color = new Color(0.92f, 0.96f, 1f, 0.42f);
                    tileRenderer.sortingOrder = 3;
                }
            }
        }

        public void KnockEnemyBack(UdderEnemy enemy)
        {
            if (!enemy || !player || !worldCamera)
                return;

            Vector2 away = enemy.transform.position - player.transform.position;
            if (away.sqrMagnitude < 0.01f)
                away = Random.insideUnitCircle.normalized;

            float distance = Random.Range(1.2f, 3.8f);
            Vector3 target = enemy.transform.position + (Vector3)(away.normalized * distance);
            enemy.transform.position = ClampToVisibleField(target, 0.8f);
        }

        private Vector3 ClampToVisibleField(Vector3 position, float padding)
        {
            if (!worldCamera)
                return position;

            float halfHeight = worldCamera.orthographicSize;
            float halfWidth = halfHeight * worldCamera.aspect;
            Vector3 cameraPosition = worldCamera.transform.position;

            position.x = Mathf.Clamp(position.x, cameraPosition.x - halfWidth + padding, cameraPosition.x + halfWidth - padding);
            position.y = Mathf.Clamp(position.y, cameraPosition.y - halfHeight + padding, cameraPosition.y + halfHeight - padding);
            return position;
        }

        public void SetWaterBodies(IReadOnlyList<Vector2> centers, IReadOnlyList<Vector2> radii)
        {
            waterBodyCenters.Clear();
            waterBodyRadii.Clear();

            for (int i = 0; i < centers.Count && i < radii.Count; i++)
            {
                waterBodyCenters.Add(centers[i]);
                waterBodyRadii.Add(radii[i]);
            }

            if (waterBodyCenters.Count == 0)
            {
                waterBodyCenters.Add(waterCenter);
                waterBodyRadii.Add(waterRadii);
            }
        }

        public Vector2 GetAutoAimDirection(Vector3 origin, Vector2 fallback)
        {
            UdderEnemy nearest = null;
            UdderSeaUrchin nearestUrchin = null;
            float nearestDistance = float.PositiveInfinity;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i] || !enemies[i].IsAlive)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if (enemies[i].IsInvulnerable)
                    continue;

                float distance = (enemies[i].transform.position - origin).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemies[i];
                }
            }

            for (int i = seaUrchins.Count - 1; i >= 0; i--)
            {
                if (!seaUrchins[i] || !seaUrchins[i].IsAlive)
                {
                    seaUrchins.RemoveAt(i);
                    continue;
                }

                float distance = (seaUrchins[i].transform.position - origin).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = null;
                    nearestUrchin = seaUrchins[i];
                }
            }

            if (!nearest && !nearestUrchin)
                return fallback.sqrMagnitude > 0.01f ? fallback.normalized : Vector2.right;

            if (nearestUrchin)
                return ((Vector2)(nearestUrchin.transform.position - origin)).normalized;

            return ((Vector2)(nearest.transform.position - origin)).normalized;
        }

        public bool TryGetAutoAimDirectionInRange(Vector3 origin, Vector2 fallback, float range, bool includeDolphins, out Vector2 direction)
        {
            direction = fallback.sqrMagnitude > 0.01f ? fallback.normalized : Vector2.right;
            float rangeSqr = range * range;
            float nearestDistance = float.PositiveInfinity;
            Vector3 nearestPosition = Vector3.zero;

            if (includeDolphins)
            {
                UdderDolphinSurface[] dolphins = Object.FindObjectsByType<UdderDolphinSurface>(FindObjectsSortMode.None);
                foreach (UdderDolphinSurface dolphin in dolphins)
                {
                    if (!dolphin || !dolphin.CanBeSpoiledMilkTarget)
                        continue;

                    float distance = (dolphin.transform.position - origin).sqrMagnitude;
                    if (distance <= rangeSqr && distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestPosition = dolphin.transform.position;
                    }
                }

                if (!float.IsPositiveInfinity(nearestDistance))
                {
                    direction = ((Vector2)(nearestPosition - origin)).normalized;
                    return direction.sqrMagnitude > 0.01f;
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i] || !enemies[i].IsAlive)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if (enemies[i].IsInvulnerable)
                    continue;

                float distance = (enemies[i].transform.position - origin).sqrMagnitude;
                if (distance <= rangeSqr && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPosition = enemies[i].transform.position;
                }
            }

            for (int i = seaUrchins.Count - 1; i >= 0; i--)
            {
                if (!seaUrchins[i] || !seaUrchins[i].IsAlive)
                {
                    seaUrchins.RemoveAt(i);
                    continue;
                }

                float distance = (seaUrchins[i].transform.position - origin).sqrMagnitude;
                if (distance <= rangeSqr && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPosition = seaUrchins[i].transform.position;
                }
            }

            if (float.IsPositiveInfinity(nearestDistance))
                return false;

            direction = ((Vector2)(nearestPosition - origin)).normalized;
            return direction.sqrMagnitude > 0.01f;
        }

        public bool TryGetBeachPosition(Vector2 dolphinPosition, Vector2 threatPosition, out Vector2 beachPosition)
        {
            Vector2 center = GetNearestWaterCenter(dolphinPosition);
            Vector2 radii = GetWaterRadii(center);
            Vector2 inward = -center;
            if (inward.sqrMagnitude < 0.01f)
                inward = threatPosition - dolphinPosition;
            if (inward.sqrMagnitude < 0.01f)
                inward = Vector2.down;
            inward.Normalize();

            if (Mathf.Abs(inward.x) > Mathf.Abs(inward.y) * 1.35f)
                beachPosition = new Vector2(center.x + Mathf.Sign(inward.x) * (radii.x + 0.75f), center.y);
            else if (Mathf.Abs(inward.y) > Mathf.Abs(inward.x) * 1.35f)
                beachPosition = new Vector2(center.x, center.y + Mathf.Sign(inward.y) * (radii.y + 0.75f));
            else
                beachPosition = center + inward * (Mathf.Max(radii.x, radii.y) + 0.85f);

            beachPosition = ClampToVisibleField(beachPosition, 0.6f);
            return true;
        }

        public Vector2 GetWaterBlockedMove(Vector2 position, Vector2 desiredMove)
        {
            return GetWaterBlockedMove(position, desiredMove, 0.12f, 0);
        }

        public Vector2 GetPlayerWaterBlockedMove(Vector2 position, Vector2 desiredMove)
        {
            return GetWaterBlockedMove(position, desiredMove, 0.18f, 0);
        }

        public Vector2 GetArenaConstrainedMove(Vector2 position, Vector2 desiredMove)
        {
            const float halfGrass = 50f;
            const float pondBand = 24f;
            const float padding = 0.35f;
            float min = -halfGrass - pondBand + padding;
            float max = halfGrass + pondBand - padding;
            Vector2 target = position + desiredMove;
            target.x = Mathf.Clamp(target.x, min, max);
            target.y = Mathf.Clamp(target.y, min, max);
            return target - position;
        }

        public Vector2 GetWaterBlockedMove(Vector2 position, Vector2 desiredMove, float moverRadius, int routeSign)
        {
            if (desiredMove.sqrMagnitude <= 0.000001f)
                return desiredMove;

            if (TryGetBlockingWater(position, position, moverRadius, out Vector2 currentCenter, out Vector2 currentRadii))
            {
                Vector2 escapeDirection = GetWaterEscapeDirection(position, currentCenter, currentRadii);
                if (Vector2.Dot(desiredMove, escapeDirection) > 0f)
                    return desiredMove;

                return escapeDirection * desiredMove.magnitude;
            }

            Vector2 next = position + desiredMove;
            if (!TryGetBlockingWater(position, next, moverRadius, out Vector2 center, out Vector2 radii))
                return desiredMove;

            Vector2 slideMove = GetWaterSlideMove(position, desiredMove, center, radii, routeSign);
            if (slideMove.sqrMagnitude > 0.000001f && !TryGetBlockingWater(position, position + slideMove, moverRadius, out _, out _))
                return slideMove;

            Vector2 xOnly = new(desiredMove.x, 0f);
            if (Mathf.Abs(xOnly.x) > 0.0001f && !TryGetBlockingWater(position, position + xOnly, moverRadius, out _, out _))
                return xOnly;

            Vector2 yOnly = new(0f, desiredMove.y);
            if (Mathf.Abs(yOnly.y) > 0.0001f && !TryGetBlockingWater(position, position + yOnly, moverRadius, out _, out _))
                return yOnly;

            return Vector2.zero;
        }

        private static Vector2 GetWaterSlideMove(Vector2 position, Vector2 desiredMove, Vector2 center, Vector2 radii, int routeSign)
        {
            Vector2 delta = position - center;
            float edgeX = Mathf.Abs(Mathf.Abs(delta.x) - radii.x);
            float edgeY = Mathf.Abs(Mathf.Abs(delta.y) - radii.y);
            bool horizontalSide = edgeY <= edgeX;

            if (Mathf.Abs(edgeX - edgeY) <= 0.035f)
            {
                if (routeSign != 0)
                {
                    Vector2 away = delta.sqrMagnitude > 0.0001f ? delta.normalized : Vector2.up;
                    Vector2 tangent = new Vector2(-away.y, away.x) * Mathf.Sign(routeSign);
                    horizontalSide = Mathf.Abs(tangent.x) >= Mathf.Abs(tangent.y);
                }
                else
                {
                    horizontalSide = Mathf.Abs(desiredMove.x) >= Mathf.Abs(desiredMove.y);
                }
            }

            return horizontalSide ? new Vector2(desiredMove.x, 0f) : new Vector2(0f, desiredMove.y);
        }

        private static Vector2 GetWaterEscapeDirection(Vector2 position, Vector2 center, Vector2 radii)
        {
            Vector2 delta = position - center;
            float distanceToVerticalEdge = radii.x - Mathf.Abs(delta.x);
            float distanceToHorizontalEdge = radii.y - Mathf.Abs(delta.y);

            if (Mathf.Abs(distanceToVerticalEdge - distanceToHorizontalEdge) <= 0.02f)
            {
                if (delta.sqrMagnitude > 0.0001f)
                    return delta.normalized;
                return Vector2.up;
            }

            if (distanceToVerticalEdge < distanceToHorizontalEdge)
                return new Vector2(delta.x >= 0f ? 1f : -1f, 0f);

            return new Vector2(0f, delta.y >= 0f ? 1f : -1f);
        }

        public bool TryGetWaterAvoidance(Vector2 position, Vector2 desiredDirection, out Vector2 adjustedDirection)
        {
            return TryGetWaterAvoidance(position, desiredDirection, 0, out adjustedDirection, out _);
        }

        public bool TryGetWaterAvoidance(Vector2 position, Vector2 desiredDirection, int routeSign, out Vector2 adjustedDirection, out Vector2 water)
        {
            if (desiredDirection.sqrMagnitude <= 0.0001f)
            {
                adjustedDirection = desiredDirection;
                water = Vector2.zero;
                return false;
            }

            const float enemyWaterPadding = 0.12f;
            Vector2 direction = desiredDirection.normalized;
            Vector2 lookAhead = position + direction * 0.72f;
            if (!IsInsideWater(lookAhead, enemyWaterPadding) && !IsInsideWater(position, enemyWaterPadding))
            {
                adjustedDirection = desiredDirection;
                water = Vector2.zero;
                return false;
            }

            water = GetNearestWaterCenter(lookAhead);
            Vector2 away = position - water;
            if (away.sqrMagnitude < 0.01f)
                away = Vector2.up;

            Vector2 tangent = new(-away.y, away.x);
            if (routeSign != 0)
                tangent *= Mathf.Sign(routeSign);
            else if (Vector2.Dot(tangent, direction) < 0f)
                tangent = -tangent;

            adjustedDirection = (tangent.normalized * 0.96f + away.normalized * 0.16f).normalized;
            return true;
        }

        public Vector2 GetEnemySeparation(UdderEnemy self, Vector2 position)
        {
            Vector2 separation = Vector2.zero;
            const float radius = 0.28f;
            float radiusSqr = radius * radius;

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                UdderEnemy other = enemies[i];
                if (!other || other == self || !other.IsAlive)
                    continue;

                Vector2 delta = position - (Vector2)other.transform.position;
                float distanceSqr = delta.sqrMagnitude;
                if (distanceSqr <= 0.0001f || distanceSqr > radiusSqr)
                    continue;

                float distance = Mathf.Sqrt(distanceSqr);
                separation += delta / distance * (1f - distance / radius);
            }

            return separation.sqrMagnitude > 1f ? separation.normalized : separation;
        }

        public bool IsInWater(Vector2 position, float padding)
        {
            return IsInsideWater(position, padding);
        }

        private bool TryGetBlockingWater(Vector2 from, Vector2 to, float moverRadius, out Vector2 center, out Vector2 radii)
        {
            center = Vector2.zero;
            radii = Vector2.zero;
            if (waterBodyCenters.Count == 0)
                SetWaterBodies(System.Array.Empty<Vector2>(), System.Array.Empty<Vector2>());

            float nearestDistance = float.PositiveInfinity;
            bool blocked = false;
            for (int i = 0; i < waterBodyCenters.Count; i++)
            {
                Vector2 expandedRadii = waterBodyRadii[i] + Vector2.one * Mathf.Max(0f, moverRadius);
                if (!IsInsideWaterRect(to, waterBodyCenters[i], expandedRadii))
                    continue;

                float distance = ((from + to) * 0.5f - waterBodyCenters[i]).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    center = waterBodyCenters[i];
                    radii = expandedRadii;
                    blocked = true;
                }
            }

            return blocked;
        }

        public bool IsWaterContaminated(Vector2 position)
        {
            for (int i = 0; i < contaminatedWaterCenters.Count; i++)
            {
                if ((contaminatedWaterCenters[i] - position).sqrMagnitude <= 1.9f * 1.9f)
                    return true;
            }

            return false;
        }

        public void ContaminateWaterAt(Vector2 position)
        {
            Vector2 center = GetNearestWaterCenter(position);
            if (IsWaterContaminated(center))
                return;

            contaminatedWaterCenters.Add(center);
            UdderPersistence.RecordPondPolluted();
        }

        public void AttractNearbyDrops(Vector3 position, float radius)
        {
            float radiusSqr = radius * radius;
            UdderPickup[] pickups = Object.FindObjectsByType<UdderPickup>(FindObjectsSortMode.None);
            foreach (UdderPickup pickup in pickups)
            {
                if (!pickup)
                    continue;

                Vector3 delta = position - pickup.transform.position;
                if (delta.sqrMagnitude > radiusSqr)
                    continue;

                if (player)
                    pickup.AttractTo(player.transform);
            }
        }

        public void AttractAllDropsToPlayer()
        {
            if (!player)
                return;

            UdderPickup[] pickups = Object.FindObjectsByType<UdderPickup>(FindObjectsSortMode.None);
            foreach (UdderPickup pickup in pickups)
            {
                if (pickup && pickup.type != PickupType.Honeycomb)
                    pickup.AttractTo(player.transform);
            }
        }

        public bool TryStartPrionPulse(Vector3 origin, int level)
        {
            UdderEnemy target = null;
            float rangeSqr = 6.5f * 6.5f;
            float nearestDistance = float.PositiveInfinity;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                UdderEnemy enemy = enemies[i];
                if (!enemy || !enemy.IsAlive)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if (enemy.IsBoss || enemy.IsPrionInfected)
                    continue;

                float distance = (enemy.transform.position - origin).sqrMagnitude;
                if (distance <= rangeSqr && distance < nearestDistance)
                {
                    target = enemy;
                    nearestDistance = distance;
                }
            }

            if (!target)
                return false;

            FirePrionProjectile(origin, target, level);
            return true;
        }

        private void FirePrionProjectile(Vector3 origin, UdderEnemy target, int level)
        {
            Vector2 direction = target.transform.position - origin;
            bool fromPrefab = prionProjectilePrefab;
            GameObject shot = InstantiateOrCreate(prionProjectilePrefab, "Prion Infection Gear");
            shot.transform.position = origin + (Vector3)(direction.normalized * 0.9f);

            var renderer = EnsureComponent<SpriteRenderer>(shot);
            if (!fromPrefab)
            {
                shot.transform.localScale = Vector3.one * 1.8f;
                renderer.sprite = prionProjectileSprite ? prionProjectileSprite : bottleSprite;
                renderer.color = Color.white;
                renderer.sortingOrder = 5;
            }

            var collider = EnsureComponent<CircleCollider2D>(shot);
            collider.isTrigger = true;
            if (!fromPrefab)
                collider.radius = 0.22f;

            var body = EnsureComponent<Rigidbody2D>(shot);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;

            var projectile = EnsureComponent<UdderProjectile>(shot);
            projectile.game = this;
            projectile.mode = MilkMode.Prion;
            if (!fromPrefab)
            {
                projectile.speed = 5.5f;
                projectile.life = 2f;
                projectile.spinDegreesPerSecond = 240f;
                projectile.homingStrength = 7f;
            }
            projectile.ConfigurePrionTarget(target, GetPrionDamagePerSecond(level), GetPrionSpreadChance(level));
            projectile.Fire(direction);
        }

        public UdderEnemy FindNearestPrionTarget(UdderEnemy source)
        {
            if (!source)
                return null;

            UdderEnemy nearest = null;
            float nearestDistance = float.PositiveInfinity;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                UdderEnemy enemy = enemies[i];
                if (!enemy || !enemy.IsAlive)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if (enemy == source)
                    continue;

                float distance = (enemy.transform.position - source.transform.position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = enemy;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        public void SpreadPrionOnDeath(UdderEnemy source, Vector3 position, float radius, float damagePerSecond, float spreadChance)
        {
            float rangeSqr = radius * radius;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                UdderEnemy enemy = enemies[i];
                if (!enemy || !enemy.IsAlive)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if (enemy == source || enemy.IsBoss || enemy.IsPrionInfected)
                    continue;

                float distance = (enemy.transform.position - position).sqrMagnitude;
                if (distance <= rangeSqr && Random.value < spreadChance)
                    enemy.ApplyPrionPulse(damagePerSecond, spreadChance);
            }
        }

        private static float GetPrionDamagePerSecond(int level)
        {
            return 1f + Mathf.Max(0, level - 1) * 0.25f;
        }

        private static float GetPrionSpreadChance(int level)
        {
            return Mathf.Clamp01(0.1f + Mathf.Max(0, level - 1) * 0.01f);
        }

        private bool IsInsideWater(Vector2 position, float padding)
        {
            if (waterBodyCenters.Count == 0)
                SetWaterBodies(System.Array.Empty<Vector2>(), System.Array.Empty<Vector2>());

            for (int i = 0; i < waterBodyCenters.Count; i++)
            {
                Vector2 radii = waterBodyRadii[i] + Vector2.one * padding;
                if (IsInsideWaterRect(position, waterBodyCenters[i], radii))
                    return true;
            }

            return false;
        }

        private static bool IsInsideWaterRect(Vector2 position, Vector2 center, Vector2 radii)
        {
            Vector2 delta = position - center;
            return Mathf.Abs(delta.x) <= radii.x && Mathf.Abs(delta.y) <= radii.y;
        }

        public void RegisterGroundedSeaUrchin(UdderSeaUrchin seaUrchin)
        {
            if (!seaUrchin || !seaUrchins.Contains(seaUrchin))
                return;

            groundedSeaUrchins = Mathf.Min(5, groundedSeaUrchins + 1);
        }

        public void UnregisterSeaUrchin(UdderSeaUrchin seaUrchin)
        {
            if (seaUrchin && seaUrchin.IsGrounded)
                groundedSeaUrchins = Mathf.Max(0, groundedSeaUrchins - 1);

            seaUrchins.Remove(seaUrchin);
        }

        public void SpawnSpoiledPool(Vector3 position, int powerLevel)
        {
            float sizeMultiplier = 1f + Mathf.Max(0, powerLevel - 1) * 0.1f;
            bool fromPrefab = spoiledMilkPuddlePrefab;
            GameObject pool = InstantiateOrCreate(spoiledMilkPuddlePrefab, "Spoiled Milk Puddle");
            pool.transform.position = position;
            pool.transform.localScale = Vector3.one * (1.7f * sizeMultiplier);
            var renderer = EnsureComponent<SpriteRenderer>(pool);
            renderer.sprite = spoiledPuddleSprite ? spoiledPuddleSprite : wholeMilkSprite ? wholeMilkSprite : creamSprite ? creamSprite : GetRuntimeSolidSprite();
            renderer.color = spoiledPuddleIdleController ? new Color(1f, 0.92f, 0.25f, 0.82f) : new Color(1f, 0.84f, 0.12f, 0.62f);
            renderer.sortingOrder = 1;
            if (spoiledPuddleIdleController)
            {
                var animator = EnsureComponent<Animator>(pool);
                animator.runtimeAnimatorController = spoiledPuddleIdleController;
            }
            var collider = EnsureComponent<CircleCollider2D>(pool);
            collider.isTrigger = true;
            if (!fromPrefab)
                collider.radius = 0.75f;
            var puddle = EnsureComponent<UdderHazardPool>(pool);
            puddle.life = 5f;
            puddle.radius = collider.radius;
            puddle.idleController = spoiledPuddleIdleController;
            puddle.hurtController = spoiledPuddleHurtController;
            puddle.deathController = spoiledPuddleDeathController;
            puddle.deathDuration = GetAnimationLength(spoiledPuddleDeathController, 0.8f);
            ShowCowText(position, "YUCK!");
        }

        private static float GetAnimationLength(RuntimeAnimatorController controller, float fallback)
        {
            if (!controller || controller.animationClips == null || controller.animationClips.Length == 0)
                return fallback;

            return Mathf.Max(0.05f, controller.animationClips[0].length);
        }

        public void RegisterEnemyDefeated(UdderEnemy enemy)
        {
            bool wasTrackedWaveEnemy = enemies.Remove(enemy);
            if (!wasTrackedWaveEnemy && !enemy.IsBoss)
                return;

            bool defeatedBoss = enemy.IsBoss;
            UdderPersistence.RecordEnemyDefeated(
                enemy.enemyKind,
                defeatedBoss && enemy.bossType == UdderBossType.MiyamotoMoosashi);
            enemiesLeftInWave = Mathf.Max(0, enemiesLeftInWave - 1);
            cream += enemy.creamValue;
            AddBovinity(enemy.rewardWave > 0 ? enemy.rewardWave : wave);
            if (defeatedBoss)
                ShowAuLaitText(enemy.transform.position + Vector3.up * 0.95f);

            if (defeatedBoss)
            {
                bossActive = false;
                bossWaveCompleted = wave;
                if (enemy.bossType == UdderBossType.Beeatrice)
                    SpawnPickup(enemy.transform.position, PickupType.Honeycomb);
            }
            else
            {
                float roll = Random.value / GetDropChanceMultiplier();
                if (roll < 0.2f)
                    SpawnPickup(enemy.transform.position, PickupType.MinorMoona);
                else if (roll < 0.29f)
                    SpawnPickup(enemy.transform.position, PickupType.NormalMoona);
                else if (roll < 0.33f)
                    SpawnPickup(enemy.transform.position, PickupType.RemarkableMoona);
                else if (roll < 0.34f)
                    SpawnPickup(enemy.transform.position, PickupType.ElysianMoona);
                else if (roll < 0.375f)
                    SpawnPickup(enemy.transform.position, PickupType.Cranberry);
                else if (roll < 0.4f)
                    SpawnPickup(enemy.transform.position, PickupType.Strawberry);
                else if (roll < 0.42f)
                    SpawnPickup(enemy.transform.position, PickupType.Raspberry);
                else if (roll < 0.44f)
                    SpawnPickup(enemy.transform.position, PickupType.Blackberries);
                else if (roll < 0.47f)
                    SpawnPickup(enemy.transform.position, PickupType.DairyDouble);
            }

            TryQueueNextWave();
        }

        public void AddBovinity(float amount)
        {
            if (!player)
                return;

            player.AddBovinity(amount);
            TryStartBovinityChoice();
        }

        public void BankDairyDouble()
        {
            bankedDairyDoubles++;
        }

        private void TryStartBovinityChoice()
        {
            if (choosingPower || !player || !player.CanLevelBovinity)
                return;

            List<UdderPower> choices = GetRandomPowerChoices();
            if (choices.Count == 0)
            {
                player.CompleteBovinityLevelUp();
                choosingPower = false;
                Time.timeScale = 1f;
                return;
            }

            choosingPower = true;
            Time.timeScale = 0f;
            if (!hud)
                hud = GetComponent<UdderHud>();

            if (hud)
            {
                hud.ShowPowerChoices(choices, ChoosePower);
                return;
            }

            choosingPower = false;
            Time.timeScale = 1f;
        }

        private void TogglePause()
        {
            paused = !paused;
            Time.timeScale = paused ? 0f : 1f;

            if (paused)
            {
                ShowPauseOverlay();
                return;
            }

            if (pauseOverlay)
            {
                Destroy(pauseOverlay);
                pauseOverlay = null;
            }
        }

        private void ShowPauseOverlay()
        {
            if (pauseOverlay)
                return;

            if (mainMenuOverlay)
            {
                pauseOverlay = Instantiate(mainMenuOverlay, mainMenuOverlay.transform.parent);
                pauseOverlay.name = "Pause Menu Overlay";
                pauseOverlay.SetActive(true);
                ConfigurePauseMenuClone();
                return;
            }

            pauseOverlay = new GameObject("Pause Overlay");
            var canvas = pauseOverlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 180;
            var scaler = pauseOverlay.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            GameObject panel = CreateWoodenOverlayPanel(pauseOverlay.transform, new Vector2(820f, 150f));
            CreateGameOverText(
                panel.transform,
                "Paused - Press escape again to resume playing",
                Vector2.zero,
                34f);
            CreateMainMenuButton(panel.transform, "Resume Game", new Vector2(0f, -45f), ResumeGame);
            CreateMainMenuButton(panel.transform, "End Game", new Vector2(0f, -105f), ReturnToMainMenu);
        }

        private void ConfigurePauseMenuClone()
        {
            Button[] buttons = pauseOverlay.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                switch (button.gameObject.name)
                {
                    case "Menu Start Game":
                        ConfigurePauseButton(button, "Resume Game", ResumeGame);
                        break;
                    case "Menu Exit Game":
                        ConfigurePauseButton(button, "End Game", ReturnToMainMenu);
                        break;
                    default:
                        button.onClick.RemoveAllListeners();
                        break;
                }
            }
        }

        private static void ConfigurePauseButton(Button button, string labelText, UnityEngine.Events.UnityAction onClick)
        {
            if (!button)
                return;

            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label)
                label.text = labelText;
        }

        private void ResumeGame()
        {
            paused = false;
            Time.timeScale = 1f;
            if (pauseOverlay)
            {
                Destroy(pauseOverlay);
                pauseOverlay = null;
            }
        }

        private static bool IsPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.escapeKey.wasPressedThisFrame;
#endif
            return Input.GetKeyDown(KeyCode.Escape);
        }

        private void ShowMainMenu()
        {
            Time.timeScale = 0f;
            mainMenuActive = true;

            if (mainMenuOverlay)
            {
                mainMenuOverlay.SetActive(true);
                WireMainMenuButtons();
                return;
            }

            mainMenuOverlay = new GameObject("Main Menu Overlay");
            var canvas = mainMenuOverlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 190;
            var scaler = mainMenuOverlay.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            mainMenuOverlay.AddComponent<GraphicRaycaster>();

            var panel = new GameObject("Main Menu Panel");
            panel.transform.SetParent(mainMenuOverlay.transform, false);
            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(560f, 560f);
            var panelImage = panel.AddComponent<Image>();
            ApplyWoodenPanel(panelImage, 0.96f);

            CreateGameOverText(panel.transform, "Udder Destruction", new Vector2(0f, 190f), 58f);
            CreateMainMenuButton(panel.transform, "Start Game", new Vector2(0f, 140f), StartGame);
            CreateMainMenuButton(panel.transform, "Sound Settings", new Vector2(0f, 70f), null);
            CreateMainMenuButton(panel.transform, "Info", new Vector2(0f, 0f), null);
            CreateMainMenuButton(panel.transform, "About", new Vector2(0f, -70f), null);
            CreateMainMenuButton(panel.transform, "Exit Game", new Vector2(0f, -140f), ExitGame);
        }

        private void WireMainMenuButtons()
        {
            ConfigureMainMenuButton(startGameButton, true, StartGame);
            ConfigureMainMenuButton(soundSettingsButton, false, null);
            ConfigureMainMenuButton(infoButton, false, null);
            ConfigureMainMenuButton(aboutButton, false, null);
            ConfigureMainMenuButton(exitGameButton, true, ExitGame);
        }

        private static void ConfigureMainMenuButton(Button button, bool interactable, UnityEngine.Events.UnityAction onClick)
        {
            if (!button)
                return;

            button.interactable = interactable;
            button.onClick.RemoveAllListeners();
            if (onClick != null)
                button.onClick.AddListener(onClick);
        }

        private void StartGame()
        {
            mainMenuActive = false;
            Time.timeScale = 1f;

            if (mainMenuOverlay)
            {
                mainMenuOverlay.SetActive(false);
            }
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private Button CreateMainMenuButton(Transform parent, string text, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new("Menu " + text);
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(360f, 52f);

            var image = buttonObject.AddComponent<Image>();
            ApplyWoodenButtonImage(image, onClick == null ? 0.58f : 1f);

            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.interactable = onClick != null;
            ColorBlock colors = button.colors;
            colors.normalColor = onClick == null ? new Color(0.62f, 0.62f, 0.62f, 1f) : Color.white;
            colors.highlightedColor = new Color(1.08f, 1.02f, 0.88f, 1f);
            colors.pressedColor = new Color(0.82f, 0.74f, 0.58f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;
            if (uiButtonPressedSprite)
            {
                SpriteState state = button.spriteState;
                state.pressedSprite = uiButtonPressedSprite;
                state.selectedSprite = uiButtonSprite ? uiButtonSprite : image.sprite;
                button.spriteState = state;
            }
            if (onClick != null)
                button.onClick.AddListener(onClick);

            TMP_Text label = CreateGameOverText(buttonObject.transform, text, Vector2.zero, 26f);
            label.color = onClick == null ? new Color(0.72f, 0.72f, 0.72f) : Color.white;
            label.rectTransform.sizeDelta = rect.sizeDelta;
            return button;
        }

        private void ApplyWoodenPanel(Image image, float alpha = 1f)
        {
            if (!image)
                return;

            image.sprite = uiPanelSprite;
            image.type = uiPanelSprite ? Image.Type.Sliced : Image.Type.Simple;
            image.color = uiPanelSprite ? new Color(1f, 1f, 1f, alpha) : new Color(0f, 0f, 0f, 0.82f * alpha);
        }

        private void ApplyWoodenButtonImage(Image image, float alpha = 1f)
        {
            if (!image)
                return;

            image.sprite = uiButtonSprite;
            image.type = uiButtonSprite ? Image.Type.Sliced : Image.Type.Simple;
            image.color = uiButtonSprite ? new Color(1f, 1f, 1f, alpha) : new Color(0f, 0f, 0f, 0.66f * alpha);
        }

        private List<UdderPower> GetRandomPowerChoices()
        {
            List<UdderPower> choices = new();
            foreach (UdderPower power in System.Enum.GetValues(typeof(UdderPower)))
            {
                if (CanOfferPowerChoice(power))
                    choices.Add(power);
            }

            for (int i = 0; i < choices.Count; i++)
            {
                int swap = Random.Range(i, choices.Count);
                (choices[i], choices[swap]) = (choices[swap], choices[i]);
            }

            if (choices.Count > 3)
                choices.RemoveRange(3, choices.Count - 3);

            return choices;
        }

        private bool CanOfferPowerChoice(UdderPower power)
        {
            if (!player || player.IsPowerDisabled(power) || player.GetRawPowerLevel(power) >= UdderPlayer.MaxPowerLevelValue)
                return false;

            return !UdderPlayer.IsCombinationPower(power) || player.AreCombinationPrerequisitesMet(power);
        }

        private void ChoosePower(UdderPower power)
        {
            int level = player.GetRawPowerLevel(power);
            int gain = 1;

            if (bankedDairyDoubles > 0 && level + 2 <= UdderPlayer.MaxPowerLevelValue)
            {
                bankedDairyDoubles--;
                gain = 2;
                DisplayBottomScreenText("DAIRY DOUBLE!");
            }

            int roll = Random.Range(1, 101);
            if (roll == 100)
            {
                if (gain == 1 && level + 2 <= UdderPlayer.MaxPowerLevelValue)
                {
                    gain = 2;
                    DisplayBottomScreenText("DAIRY DOUBLE!");
                }
                else
                {
                    bankedDairyDoubles++;
                    DisplayBottomScreenText("DAIRY DOUBLE BANKED!");
                }
            }

            player.AddPowerLevel(power, gain);
            DisplayBottomScreenText(UdderPlayer.GetPowerLabel(power).ToUpperInvariant() + " +" + gain);
            player.CompleteBovinityLevelUp();

            if (hud)
                hud.HidePowerChoices();

            choosingPower = false;
            if (player.CanLevelBovinity)
            {
                TryStartBovinityChoice();
                return;
            }

            Time.timeScale = 1f;
        }

        public void AddCream(int amount)
        {
            cream += amount;
        }

        public void ShowDamageText(Vector3 worldPosition, float amount, MilkMode mode, bool crit)
        {
            return;

#pragma warning disable CS0162
            string label = crit ? "CRIT " + Mathf.CeilToInt(amount) : Mathf.CeilToInt(amount).ToString();
            TextAnimation animation = mode switch
            {
                MilkMode.Buttermilk => acidText,
                MilkMode.TresLeches => acidText,
                MilkMode.SpoiledMilk => poisonText,
                MilkMode.RawMilk => poisonText,
                MilkMode.Prion => poisonText,
                MilkMode.Aura => healText,
                _ => crit ? critText : damageText,
            };
            DisplayText(label, animation, worldPosition + Vector3.up * 0.45f, true);
#pragma warning restore CS0162
        }

        public void ShowCowText(Vector3 worldPosition, string text)
        {
            DisplayText(text, healText, worldPosition + Vector3.up * 0.8f, true);
        }

        public void ShowEnemyDebuffText(Transform enemy, string text)
        {
            if (!enemy)
                return;

            GameObject textObject = new("Enemy Debuff Text");
            textObject.transform.SetParent(enemy, false);
            Vector3 enemyScale = enemy.lossyScale;
            textObject.transform.localPosition = new Vector3(
                0f,
                enemyScale.y != 0f ? 0.72f / Mathf.Abs(enemyScale.y) : 0.72f,
                0f);
            textObject.transform.localScale = new Vector3(
                enemyScale.x != 0f ? 1f / Mathf.Abs(enemyScale.x) : 1f,
                enemyScale.y != 0f ? 1f / Mathf.Abs(enemyScale.y) : 1f,
                1f);

            var label = textObject.AddComponent<TextMeshPro>();
            ApplyPixelFont(label);
            label.text = text;
            label.color = new Color(1f, 0.86f, 0.12f);
            label.fontSize = 0.42f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 20;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.outlineWidth = 0.18f;
            label.outlineColor = Color.black;

            var timedText = textObject.AddComponent<UdderTimedWorldText>();
            timedText.label = label;
            timedText.lingerTime = 1.1f;
            timedText.fadeTime = 0.7f;
            timedText.bobAmount = 0.025f;
            timedText.pulseAmount = 0.04f;
        }

        public void ShowHealText(Vector3 worldPosition, float amount)
        {
            string label = amount > 0f ? "+" + Mathf.CeilToInt(amount).ToString() : "HP Full!";
            DisplayText(label, healText, worldPosition + Vector3.up * 0.8f, true);
        }

        public void ShowCheesedItText(Transform cow)
        {
            UdderPersistence.RecordCheesedIt();
            if (!cow)
                return;

            GameObject textObject = new("Cheesed It Text");
            textObject.transform.SetParent(cow, false);
            textObject.transform.localPosition = new Vector3(0f, 1.05f, 0f);

            var label = textObject.AddComponent<TextMeshPro>();
            ApplyPixelFont(label);
            label.text = "Cheesed it!";
            label.color = new Color(1f, 0.86f, 0.08f);
            label.fontSize = 0.64f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 20;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.outlineWidth = 0.24f;
            label.outlineColor = Color.black;

            var timedText = textObject.AddComponent<UdderTimedWorldText>();
            timedText.label = label;
            timedText.lingerTime = 3f;
            timedText.fadeTime = 0f;
            timedText.bobAmount = 0.02f;
            timedText.pulseAmount = 0.04f;
        }

        public void ShowDrowningText(Vector3 worldPosition)
        {
            GameObject textObject = new("Drowning Text");
            textObject.transform.position = worldPosition + Vector3.up * 0.95f;

            var label = textObject.AddComponent<TextMeshPro>();
            ApplyPixelFont(label);
            label.text = "YOU'RE DROWNING!";
            label.color = new Color(0.35f, 0.88f, 1f);
            label.fontSize = 0.62f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 22;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.outlineWidth = 0.18f;
            label.outlineColor = Color.black;

            var timedText = textObject.AddComponent<UdderTimedWorldText>();
            timedText.label = label;
            timedText.lingerTime = 0.15f;
            timedText.fadeTime = 0.65f;
            timedText.bobAmount = 0.02f;
            timedText.pulseAmount = 0.04f;
        }

        private void ShowAuLaitText(Vector3 worldPosition)
        {
            GameObject textObject = new("Au Lait Text");
            textObject.transform.position = worldPosition;

            var label = textObject.AddComponent<TextMeshPro>();
            ApplyPixelFont(label);
            label.richText = true;
            label.text = "<color=#ff1b12>A</color><color=#ffe21a>U</color> <color=#ff1b12>L</color><color=#ffe21a>A</color><color=#ff1b12>I</color><color=#ffe21a>T</color><color=#ff1b12>!</color>";
            label.fontSize = 1.15f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 25;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.outlineWidth = 0.24f;
            label.outlineColor = Color.black;

            var timedText = textObject.AddComponent<UdderTimedWorldText>();
            timedText.label = label;
            timedText.lingerTime = 0f;
            timedText.fadeTime = 5f;
            timedText.bobAmount = 0.04f;
            timedText.pulseAmount = 0.08f;
        }

        public void GameOver()
        {
            finished = true;
            StartCoroutine(ShowGameOverSequence());
        }

        private IEnumerator ShowGameOverSequence()
        {
            if (gameOverOverlay)
                Destroy(gameOverOverlay);

            gameOverOverlay = new GameObject("Game Over Overlay");
            var canvas = gameOverOverlay.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;
            var scaler = gameOverOverlay.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            GameObject panel = CreateWoodenOverlayPanel(gameOverOverlay.transform, new Vector2(780f, 360f));
            Button mainMenuButton = CreateMainMenuButton(panel.transform, "Main Menu", new Vector2(0f, -138f), ReturnToMainMenu);
            mainMenuButton.gameObject.SetActive(false);

            TMP_Text dontCry = CreateGameOverText(panel.transform, "Don't cry", new Vector2(0f, 86f), 62);
            yield return FadeOutGameOverText(dontCry, 1f);

            CreateGameOverText(panel.transform, "Over", new Vector2(0f, 0f), 96);
            mainMenuButton.gameObject.SetActive(true);

            TMP_Text spilledMilk = CreateGameOverText(panel.transform, "Spilled Milk", new Vector2(0f, -92f), 62);
            yield return FadeOutGameOverText(spilledMilk, 1f);

            CreateGameOverText(panel.transform, "Game", new Vector2(0f, 98f), 96);
        }

        private static void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private GameObject CreateWoodenOverlayPanel(Transform parent, Vector2 size)
        {
            GameObject panel = new("Wooden Overlay Panel");
            panel.transform.SetParent(parent, false);
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            ApplyWoodenPanel(panel.AddComponent<Image>(), 0.96f);
            return panel;
        }

        private static TMP_Text CreateGameOverText(Transform parent, string text, Vector2 anchoredPosition, float fontSize)
        {
            GameObject textObject = new("Game Over " + text);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            Vector2 parentSize = parent is RectTransform parentRect && parentRect.sizeDelta.sqrMagnitude > 0f
                ? parentRect.sizeDelta
                : new Vector2(960f, 130f);
            rect.sizeDelta = new Vector2(Mathf.Max(120f, parentSize.x - 160f), Mathf.Min(130f, Mathf.Max(44f, parentSize.y - 70f)));

            var label = textObject.AddComponent<TextMeshProUGUI>();
            ApplyPixelFont(label);
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.outlineWidth = 0.22f;
            label.outlineColor = Color.black;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static void ApplyPixelFont(TMP_Text label)
        {
            if (label && sharedUiFont)
                label.font = sharedUiFont;
        }

        private static IEnumerator FadeOutGameOverText(TMP_Text label, float duration)
        {
            float elapsed = 0f;
            Color color = label.color;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                color.a = 1f - Mathf.Clamp01(elapsed / duration);
                label.color = color;
                yield return null;
            }

            if (label)
                Destroy(label.gameObject);
        }

        private void SpawnEnemy()
        {
            SpawnEnemy(wave);
        }

        private void SpawnEnemy(int sourceWave)
        {
            bool isChicken = Random.value >= GetPigWaveRatio(sourceWave);
            GameObject enemyPrefab = isChicken ? chickenEnemyPrefab : pigEnemyPrefab;
            bool fromPrefab = enemyPrefab;
            GameObject enemyObject = InstantiateOrCreate(enemyPrefab, isChicken ? "Debt Chicken" : "Hostile Ham");
            enemyObject.transform.position = GetEnemySpawnPositionNearPlayer();
            var renderer = EnsureComponent<SpriteRenderer>(enemyObject);
            renderer.sprite = isChicken ? chickenSprite : pigSprite;
            renderer.sortingOrder = 4;
            renderer.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.55f), wave * 0.04f);
            ScaleSpriteToHeight(enemyObject.transform, renderer.sprite, isChicken ? 0.39f : 0.78f);

            var body = EnsureComponent<Rigidbody2D>(enemyObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = EnsureComponent<CircleCollider2D>(enemyObject);
            if (!fromPrefab)
            {
                collider.radius = isChicken ? 0.12f : 0.13f;
                collider.offset = isChicken ? Vector2.zero : new Vector2(0f, -0.02f);
            }

            var enemy = EnsureComponent<UdderEnemy>(enemyObject);
            enemy.enemyKind = isChicken ? UdderEnemyKind.DebtChicken : UdderEnemyKind.HostileHam;
            enemy.rewardWave = sourceWave;
            enemy.downSprite = isChicken ? chickenDownSprite : pigDownSprite;
            enemy.sideSprite = isChicken ? chickenSideSprite : pigSideSprite;
            enemy.upSprite = isChicken ? chickenUpSprite : pigUpSprite;
            enemy.rawMilkFlySprite = rawMilkFlySprite;
            enemy.cottonDeathSprite = cottonDeathSprite;
            enemy.skullDeathSprite = skullDeathSprite;
            enemy.prionAngrySprite = prionAngrySprite;
            enemy.prionIndicatorPrefab = prionIndicatorPrefab;
            enemy.avoidsWater = true;
            enemy.maxHealth = isChicken ? 8f : Random.value < 0.25f ? 22f : 14f;
            enemy.speed = isChicken ? Random.Range(1.05f, 1.35f) : Random.Range(1.45f, 1.85f);
            enemy.creamValue = isChicken ? Random.Range(1, 3) : Random.Range(2, 5);
            enemy.Init(this, player, 1f + sourceWave * 0.13f, GetEnemySpeedScale(sourceWave));
            enemies.Add(enemy);
        }

        public void SpawnTestUnit(UdderTestSpawnUnit unit)
        {
            if (!player)
                return;

            switch (unit)
            {
                case UdderTestSpawnUnit.DebtChicken:
                    SpawnTestEnemy(true, GetTestSpawnPosition(), "Test Debt Chicken");
                    break;
                case UdderTestSpawnUnit.HostileHam:
                    SpawnTestEnemy(false, GetTestSpawnPosition(), "Test Hostile Ham");
                    break;
                case UdderTestSpawnUnit.EnemyCow:
                    SpawnBossEscort(GetTestSpawnPosition(), false, 0, 1);
                    break;
                case UdderTestSpawnUnit.BeeDrone:
                    SpawnTestBeeDrone(GetTestSpawnPosition());
                    break;
                case UdderTestSpawnUnit.PondDolphin:
                    SpawnDolphinSurface(GetTestSpawnPosition());
                    break;
                case UdderTestSpawnUnit.HostileSeaUrchin:
                    LaunchSeaUrchin(GetTestSpawnPosition(), player.transform.position);
                    break;
                case UdderTestSpawnUnit.MiyamotoMoosashi:
                    SpawnTestBoss(UdderBossType.MiyamotoMoosashi);
                    break;
                case UdderTestSpawnUnit.Lidia:
                    SpawnTestBoss(UdderBossType.Lidia);
                    break;
                case UdderTestSpawnUnit.BobMoorley:
                    SpawnTestBoss(UdderBossType.BobMoorley);
                    break;
                case UdderTestSpawnUnit.HughHoofner:
                    SpawnTestBoss(UdderBossType.HughHoofner);
                    break;
                case UdderTestSpawnUnit.HolyCow:
                    SpawnTestBoss(UdderBossType.HolyCow);
                    break;
                case UdderTestSpawnUnit.Ruminator:
                    SpawnTestBoss(UdderBossType.Ruminator);
                    break;
                case UdderTestSpawnUnit.Beeatrice:
                    SpawnTestBoss(UdderBossType.Beeatrice);
                    break;
            }
        }

        private Vector3 GetTestSpawnPosition()
        {
            Vector3 center = player ? player.transform.position : Vector3.zero;
            Vector3 candidate = center + Vector3.right * 3.4f + Vector3.up * Random.Range(-1.4f, 1.4f);
            return ClampToVisibleField(candidate, 0.9f);
        }

        private void SpawnTestBoss(UdderBossType bossType)
        {
            bossPending = false;
            bossActive = false;
            enemiesPendingSpawn = 0;
            enemiesLeftInWave = 0;
            SpawnBoss(bossType);
        }

        private void SpawnTestEnemy(bool isChicken, Vector3 position, string name)
        {
            GameObject enemyPrefab = isChicken ? chickenEnemyPrefab : pigEnemyPrefab;
            bool fromPrefab = enemyPrefab;
            GameObject enemyObject = InstantiateOrCreate(enemyPrefab, name);
            enemyObject.transform.position = position;
            var renderer = EnsureComponent<SpriteRenderer>(enemyObject);
            renderer.sprite = isChicken ? chickenSprite : pigSprite;
            renderer.sortingOrder = 4;
            renderer.color = Color.white;
            ScaleSpriteToHeight(enemyObject.transform, renderer.sprite, isChicken ? 0.39f : 0.78f);

            var body = EnsureComponent<Rigidbody2D>(enemyObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = EnsureComponent<CircleCollider2D>(enemyObject);
            if (!fromPrefab)
            {
                collider.radius = isChicken ? 0.12f : 0.13f;
                collider.offset = isChicken ? Vector2.zero : new Vector2(0f, -0.02f);
            }

            var enemy = EnsureComponent<UdderEnemy>(enemyObject);
            enemy.enemyKind = isChicken ? UdderEnemyKind.DebtChicken : UdderEnemyKind.HostileHam;
            enemy.downSprite = isChicken ? chickenDownSprite : pigDownSprite;
            enemy.sideSprite = isChicken ? chickenSideSprite : pigSideSprite;
            enemy.upSprite = isChicken ? chickenUpSprite : pigUpSprite;
            enemy.rawMilkFlySprite = rawMilkFlySprite;
            enemy.cottonDeathSprite = cottonDeathSprite;
            enemy.skullDeathSprite = skullDeathSprite;
            enemy.prionAngrySprite = prionAngrySprite;
            enemy.prionIndicatorPrefab = prionIndicatorPrefab;
            enemy.avoidsWater = true;
            enemy.maxHealth = isChicken ? 8f : 18f;
            enemy.speed = isChicken ? 1.2f : 1.65f;
            enemy.creamValue = isChicken ? 1 : 3;
            enemy.Init(this, player, 1f, 1f);
            enemies.Add(enemy);
            enemiesLeftInWave++;
        }

        private void SpawnTestBeeDrone(Vector3 position)
        {
            bool fromPrefab = beeDronePrefab;
            GameObject droneObject = InstantiateOrCreate(beeDronePrefab, "Test BEEatrice Drone");
            droneObject.transform.position = position;

            var renderer = EnsureComponent<SpriteRenderer>(droneObject);
            if (!fromPrefab)
            {
                renderer.sprite = beeDroneSprite ? beeDroneSprite : beeatriceSprite;
                renderer.color = Color.white;
                renderer.sortingOrder = 5;
                ScaleSpriteToHeight(droneObject.transform, renderer.sprite, 0.39f);
            }

            var body = EnsureComponent<Rigidbody2D>(droneObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = EnsureComponent<CircleCollider2D>(droneObject);
            if (!fromPrefab)
            {
                collider.radius = 0.12f;
                collider.offset = Vector2.zero;
            }

            var enemy = EnsureComponent<UdderEnemy>(droneObject);
            enemy.enemyKind = UdderEnemyKind.Bee;
            enemy.downSprite = beeDroneSprite;
            enemy.sideSprite = beeDroneSprite;
            enemy.upSprite = beeDroneSprite;
            enemy.rawMilkFlySprite = rawMilkFlySprite;
            enemy.cottonDeathSprite = cottonDeathSprite;
            enemy.skullDeathSprite = skullDeathSprite;
            enemy.prionAngrySprite = prionAngrySprite;
            enemy.prionIndicatorPrefab = prionIndicatorPrefab;
            enemy.avoidsWater = true;
            enemy.isFlying = true;
            enemy.IsBoss = false;
            enemy.maxHealth = 12f;
            enemy.speed = 1.55f;
            enemy.contactDamage = 7f;
            enemy.creamValue = 1;
            enemy.Init(this, player, 1f, 1f);
            enemies.Add(enemy);
            enemiesLeftInWave++;
        }

        private Vector3 GetEnemySpawnPositionNearPlayer()
        {
            Vector2 center = player ? player.transform.position : Vector3.zero;

            for (int i = 0; i < 16; i++)
            {
                Vector2 direction = Random.insideUnitCircle.normalized;
                if (direction.sqrMagnitude < 0.01f)
                    direction = Vector2.right;

                Vector3 candidate = center + direction * Random.Range(6.5f, 8.5f);
                candidate = ClampToVisibleField(candidate, 0.9f);
                if (!IsInWater(candidate, 0.45f))
                    return candidate;
            }

            return ClampToVisibleField(center + Random.insideUnitCircle.normalized * 6.5f, 0.9f);
        }

        private void UpdateDolphin()
        {
            if (bossPending || bossActive)
            {
                dolphinTimer = 5f;
                return;
            }

            if (groundedSeaUrchins >= 5)
            {
                dolphinTimer = 5f;
                return;
            }

            dolphinTimer -= Time.deltaTime;
            if (dolphinTimer > 0f)
                return;

            dolphinTimer = 5f;
            if (!TryGetDolphinWaterCenter(player ? player.transform.position : waterCenter, out Vector2 center))
                return;

            Vector3 surfacePosition = new(center.x, center.y, 0f);

            SpawnDolphinSurface(surfacePosition);

            if (player && ((Vector2)player.transform.position - center).sqrMagnitude <= 5f * 5f)
                LaunchSeaUrchin(surfacePosition, player.transform.position);
        }

        private void SpawnDolphinSurface(Vector3 position)
        {
            bool fromPrefab = dolphinPrefab;
            GameObject dolphin = InstantiateOrCreate(dolphinPrefab, "Pond Dolphin");
            dolphin.transform.position = position;
            var renderer = EnsureComponent<SpriteRenderer>(dolphin);
            renderer.sprite = dolphinSprite ? dolphinSprite : GetRuntimeSolidSprite();
            renderer.color = dolphinSprite ? Color.white : new Color(0.4f, 0.85f, 1f);
            renderer.sortingOrder = 4;
            ScaleSpriteToHeight(dolphin.transform, renderer.sprite, 1.2f);
            var collider = EnsureComponent<CircleCollider2D>(dolphin);
            collider.isTrigger = true;
            if (!fromPrefab)
                collider.radius = 0.32f;
            var body = EnsureComponent<Rigidbody2D>(dolphin);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            var surface = EnsureComponent<UdderDolphinSurface>(dolphin);
            surface.game = this;
            surface.cottonDeathSprite = cottonDeathSprite;
            surface.skullDeathSprite = skullDeathSprite;
        }

        private void LaunchSeaUrchin(Vector3 origin, Vector3 target)
        {
            bool fromPrefab = seaUrchinPrefab;
            GameObject urchinObject = InstantiateOrCreate(seaUrchinPrefab, "Hostile Sea Urchin");
            urchinObject.transform.position = origin;
            var renderer = EnsureComponent<SpriteRenderer>(urchinObject);
            renderer.sprite = seaUrchinSprite ? seaUrchinSprite : GetRuntimeSolidSprite();
            renderer.color = seaUrchinSprite ? Color.white : new Color(0.2f, 0.05f, 0.25f);
            renderer.sortingOrder = 6;
            ScaleSpriteToHeight(urchinObject.transform, renderer.sprite, 0.65f);

            var collider = EnsureComponent<CircleCollider2D>(urchinObject);
            collider.isTrigger = true;
            if (!fromPrefab)
                collider.radius = 0.18f;
            var body = EnsureComponent<Rigidbody2D>(urchinObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;

            var seaUrchin = EnsureComponent<UdderSeaUrchin>(urchinObject);
            seaUrchin.Init(this, target);
            seaUrchins.Add(seaUrchin);
            ShowCowText(origin, "URCHIN!");
        }

        private void StartWave()
        {
            contaminatedWaterCenters.Clear();
            RefreshStampedeAllies();
            playerTookDamageThisWave = false;
            spawningMilkshakeRush = false;
            int count = GetWaveEnemyCount(wave);
            enemiesLeftInWave = count;
            enemiesPendingSpawn = count;
            waveSpawnTimer = 0f;
            nextWaveTimer = 0f;
            DisplayText("WAVE " + wave, levelText, player.transform.position + Vector3.up * 1.2f);
        }

        private void RefreshStampedeAllies()
        {
            CleanupAlliedCows();
            int targetCount = player ? player.GetPowerLevel(UdderPower.Stampede) : 0;
            for (int i = alliedCows.Count; i < targetCount; i++)
                SpawnStampedeAlly(i);
        }

        private void SpawnStampedeAlly(int index)
        {
            if (!player)
                return;

            GameObject cowObject = new("Stampede Cow");
            float angle = index * 137.5f * Mathf.Deg2Rad;
            cowObject.transform.position = player.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 1.4f;

            var renderer = EnsureComponent<SpriteRenderer>(cowObject);
            renderer.sprite = cowSprite ? cowSprite : GetRuntimeSolidSprite();
            renderer.color = new Color(0.92f, 1f, 0.9f);
            renderer.sortingOrder = 4;
            ScaleSpriteToHeight(cowObject.transform, renderer.sprite, 0.95f);

            var body = EnsureComponent<Rigidbody2D>(cowObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = EnsureComponent<CircleCollider2D>(cowObject);
            collider.radius = 0.2f;

            var ally = EnsureComponent<UdderAlliedCow>(cowObject);
            ally.Init(this, player, cowDownSprite, cowSideSprite ? cowSideSprite : cowSprite, cowUpSprite);
            alliedCows.Add(ally);
        }

        private void UpdateWaveSpawning()
        {
            if (bossPending || enemiesPendingSpawn <= 0)
                return;

            waveSpawnTimer -= Time.deltaTime;
            if (waveSpawnTimer > 0f)
                return;

            SpawnEnemy();
            enemiesPendingSpawn--;
            waveSpawnTimer = GetWaveSpawnInterval();
        }

        private bool IsWaveCleared()
        {
            CleanupEnemyList();
            return enemiesLeftInWave <= 0 && enemies.Count == 0;
        }

        private void TryQueueNextWave()
        {
            if (!IsWaveCleared())
                return;

            if (TryStartMilkshakeRush())
                return;

            if (IsBossWave(wave) && bossWaveCompleted != wave)
            {
                pendingBossType = ChooseBossForWave(wave);
                bossPending = true;
                nextWaveTimer = 3f;
                dolphinTimer = 5f;
                DisplayText(GetBossName(pendingBossType).ToUpperInvariant() + " APPROACHES!", levelText, player.transform.position + Vector3.up * 1.1f);
                return;
            }

            nextWaveTimer = 5f;
            if (gameMode == UdderGameMode.Vegan)
                UdderPersistence.RecordVeganWaveCleared();
            wave++;
            DisplayTopScreenText("WAVE CLEARED!");
        }

        private bool TryStartMilkshakeRush()
        {
            if (spawningMilkshakeRush || bossPending || bossActive || !player || !player.IsAtFullHealth || playerTookDamageThisWave || IsBossWave(wave))
                return false;

            int nextBossWave = GetNextBossWave(wave);
            int firstRushWave = wave + 1;
            int lastRushWave = nextBossWave - 1;
            if (firstRushWave > lastRushWave)
                return false;

            spawningMilkshakeRush = true;
            SpawnMilkshake(player.transform.position, player.Health);

            int totalCount = 0;
            for (int rushWave = firstRushWave; rushWave <= lastRushWave; rushWave++)
                totalCount += GetWaveEnemyCount(rushWave);

            enemiesLeftInWave = totalCount;
            enemiesPendingSpawn = 0;
            for (int rushWave = firstRushWave; rushWave <= lastRushWave; rushWave++)
            {
                int count = GetWaveEnemyCount(rushWave);
                for (int i = 0; i < count; i++)
                    SpawnEnemy(rushWave);
            }

            wave = nextBossWave;
            playerTookDamageThisWave = false;
            DisplayTopScreenText("MILKSHAKE RUSH!");
            return true;
        }

        private void SpawnMilkshake(Vector3 position, float health)
        {
            if (activeMilkshake)
                Destroy(activeMilkshake.gameObject);

            GameObject milkshakeObject = InstantiateOrCreate(milkshakePrefab, "Milkshake Lure");
            milkshakeObject.transform.position = position;

            var renderer = EnsureComponent<SpriteRenderer>(milkshakeObject);
            renderer.sprite = milkshakeSprite ? milkshakeSprite : GetRuntimeSolidSprite();
            renderer.color = milkshakeSprite ? Color.white : new Color(1f, 0.84f, 0.98f);
            renderer.sortingOrder = 4;
            milkshakeObject.transform.localScale = Vector3.one * 3.6f;

            var collider = EnsureComponent<CircleCollider2D>(milkshakeObject);
            collider.isTrigger = true;
            collider.radius = 0.44f;

            var body = EnsureComponent<Rigidbody2D>(milkshakeObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;

            activeMilkshake = EnsureComponent<UdderMilkshake>(milkshakeObject);
            activeMilkshake.Init(this, health);
            ShowCowText(position, "MILKSHAKE!");
        }

        public void ClearMilkshake(UdderMilkshake milkshake)
        {
            if (activeMilkshake == milkshake)
                activeMilkshake = null;
        }

        public Transform GetActiveMilkshakeTarget()
        {
            return activeMilkshake && activeMilkshake.IsAlive ? activeMilkshake.transform : null;
        }

        public void RecordPlayerTookDamageThisWave()
        {
            playerTookDamageThisWave = true;
        }

        private static int GetNextBossWave(int waveNumber)
        {
            return Mathf.CeilToInt((waveNumber + 1) / 10f) * 10;
        }

        private int GetWaveEnemyCount(int waveNumber)
        {
            return Mathf.CeilToInt((4 + waveNumber * 2) * GetWaveEnemyCountMultiplier());
        }

        private float GetPigWaveRatio()
        {
            return GetPigWaveRatio(wave);
        }

        private static float GetPigWaveRatio(int waveNumber)
        {
            if (waveNumber < 5)
                return 0f;
            if (waveNumber <= 10)
                return 0.1f;

            int decadeSteps = Mathf.Clamp((waveNumber - 1) / 10, 1, 4);
            return 0.1f + decadeSteps * 0.1f;
        }

        private float GetWaveSpawnInterval()
        {
            return Mathf.Max(0.32f, 1.65f - (wave - 1) * 0.055f);
        }

        private float GetEnemySpeedScale()
        {
            return GetEnemySpeedScale(wave);
        }

        private static float GetEnemySpeedScale(int waveNumber)
        {
            return Mathf.Lerp(0.78f, 1.28f, Mathf.Clamp01((waveNumber - 1) / 24f));
        }

        private float GetWaveEnemyCountMultiplier()
        {
            return player ? 1f + player.GetPowerLevel(UdderPower.MoreCowbell) * 0.1f : 1f;
        }

        private float GetDropChanceMultiplier()
        {
            return player ? 1f + player.GetPowerLevel(UdderPower.MoreCowbell) * 0.1f : 1f;
        }

        private static float GetButterWorldSize(int powerLevel)
        {
            int evenUpgrades = Mathf.Clamp(Mathf.Max(1, powerLevel) / 2, 0, 5);
            return 1f + evenUpgrades * 0.2f;
        }

        private static float GetButterLife(int powerLevel)
        {
            int oddUpgrades = Mathf.Max(1, (powerLevel + 1) / 2);
            return 8f + (oddUpgrades - 1) * 2f;
        }

        private static float GetDairyAirCloudLife(int powerLevel, float interval)
        {
            float maxLife = Mathf.Max(7f, interval * 2f);
            float levelProgress = Mathf.Clamp01((powerLevel - 1) / 9f);
            return Mathf.Lerp(7f, maxLife, levelProgress);
        }

        private bool HasLivingAutoAimTarget()
        {
            CleanupEnemyList();
            CleanupSeaUrchins();
            return enemies.Count > 0 || seaUrchins.Count > 0;
        }

        private static bool IsBossWave(int waveNumber)
        {
            return waveNumber >= 10 && waveNumber % 10 == 0;
        }

        private static UdderBossType ChooseBossForWave(int waveNumber)
        {
            if (waveNumber == 10)
                return UdderBossType.MiyamotoMoosashi;

            UdderBossType[] pool =
            {
                UdderBossType.MiyamotoMoosashi,
                UdderBossType.Lidia,
                UdderBossType.BobMoorley,
                UdderBossType.HughHoofner,
                UdderBossType.HolyCow,
                UdderBossType.Ruminator,
                UdderBossType.Beeatrice,
            };
            return pool[Random.Range(0, pool.Length)];
        }

        private static string GetBossName(UdderBossType type)
        {
            return type switch
            {
                UdderBossType.MiyamotoMoosashi => "Miyamoto Moosashi",
                UdderBossType.Lidia => "Lidia",
                UdderBossType.BobMoorley => "Bob Moorley",
                UdderBossType.HughHoofner => "Hugh Hoofner",
                UdderBossType.HolyCow => "The Holy Cow",
                UdderBossType.Ruminator => "The Ruminator",
                UdderBossType.Beeatrice => "BEEatrice",
                _ => "Mystery Boss",
            };
        }

        private void SpawnBoss(UdderBossType bossType)
        {
            bossPending = false;
            bossActive = true;
            bool repeatEncounter = !encounteredBosses.Add(bossType);
            int inherentExtras = bossType switch
            {
                UdderBossType.BobMoorley => 3,
                UdderBossType.HughHoofner => 5,
                UdderBossType.Beeatrice => 12,
                _ => 0,
            };
            int normalReinforcements = repeatEncounter ? wave : 0;
            enemiesPendingSpawn = normalReinforcements;
            waveSpawnTimer = GetWaveSpawnInterval();
            enemiesLeftInWave = (bossType == UdderBossType.HolyCow ? 0 : 1) + inherentExtras + normalReinforcements;
            dolphinTimer = 5f;
            DestroyActiveDolphins();

            if (bossType == UdderBossType.HolyCow)
            {
                SpawnHolyCow();
                return;
            }

            Vector2 center = player.transform.position;
            Vector2 offset = Random.insideUnitCircle.normalized * 8f;
            GameObject selectedBossPrefab = bossType == UdderBossType.Beeatrice && beeatriceBossPrefab ? beeatriceBossPrefab : bossEnemyPrefab;
            bool fromPrefab = selectedBossPrefab;
            string bossName = GetBossName(bossType);
            GameObject bossObject = InstantiateOrCreate(selectedBossPrefab, bossName);
            bossObject.transform.position = center + offset;

            var renderer = EnsureComponent<SpriteRenderer>(bossObject);
            if (!fromPrefab || bossType != UdderBossType.Beeatrice)
            {
                renderer.sprite = bossType == UdderBossType.Beeatrice && beeatriceSprite
                    ? beeatriceSprite
                    : bossCowSprite ? bossCowSprite : cowSprite;
                renderer.sortingOrder = 5;
                renderer.color = new Color(1f, 0.93f, 0.93f);
                ScaleSpriteToHeight(bossObject.transform, renderer.sprite, 1.17f);
            }

            var body = EnsureComponent<Rigidbody2D>(bossObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = EnsureComponent<CircleCollider2D>(bossObject);
            if (!fromPrefab)
                collider.radius = 0.166f;

            var nameObject = new GameObject("Boss Name");
            nameObject.transform.SetParent(bossObject.transform, false);
            Vector3 bossScale = bossObject.transform.localScale;
            nameObject.transform.localPosition = new Vector3(0f, bossScale.y != 0f ? 1.62f / Mathf.Abs(bossScale.y) : 1.62f, 0f);
            nameObject.transform.localScale = new Vector3(
                bossScale.x != 0f ? 1f / Mathf.Abs(bossScale.x) : 1f,
                bossScale.y != 0f ? 1f / Mathf.Abs(bossScale.y) : 1f,
                1f);
            var label = nameObject.AddComponent<TextMeshPro>();
            ApplyPixelFont(label);
            label.text = bossName;
            label.color = Color.black;
            label.fontSize = 3.375f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 12;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.outlineWidth = 0.08f;
            label.outlineColor = new Color(1f, 1f, 1f, 0.85f);

            if (bossType != UdderBossType.Beeatrice)
                ShowEnemyCowSpawnTaunt(bossObject.transform, 0.98f);

            var enemy = EnsureComponent<UdderEnemy>(bossObject);
            enemy.enemyKind = bossType == UdderBossType.Beeatrice ? UdderEnemyKind.Bee : UdderEnemyKind.Cow;
            enemy.downSprite = bossType == UdderBossType.Beeatrice ? beeatriceSprite : bossCowDownSprite ? bossCowDownSprite : cowDownSprite;
            enemy.sideSprite = bossType == UdderBossType.Beeatrice ? beeatriceSprite : bossCowSideSprite ? bossCowSideSprite : cowSideSprite;
            enemy.upSprite = bossType == UdderBossType.Beeatrice ? beeatriceSprite : bossCowUpSprite ? bossCowUpSprite : cowUpSprite;
            enemy.rawMilkFlySprite = rawMilkFlySprite;
            enemy.cottonDeathSprite = cottonDeathSprite;
            enemy.skullDeathSprite = skullDeathSprite;
            enemy.prionAngrySprite = prionAngrySprite;
            enemy.prionIndicatorPrefab = prionIndicatorPrefab;
            enemy.avoidsWater = true;
            enemy.isFlying = bossType == UdderBossType.Beeatrice;
            enemy.IsBoss = true;
            enemy.bossType = bossType;
            if (!fromPrefab || bossType != UdderBossType.Beeatrice)
            {
                enemy.maxHealth = bossType switch
                {
                    UdderBossType.Lidia => 440f,
                    UdderBossType.BobMoorley => 500f,
                    UdderBossType.HughHoofner => 540f,
                    UdderBossType.Ruminator => 430f,
                    UdderBossType.Beeatrice => 480f,
                    _ => 520f,
                };
                enemy.speed = bossType == UdderBossType.Ruminator ? 1.35f : 1.55f;
                enemy.contactDamage = bossType == UdderBossType.Lidia ? 20f : 16f;
                enemy.creamValue = 30;
            }
            enemy.Init(this, player, 1f + wave * 0.18f, 1f);
            enemies.Add(enemy);

            if (bossType == UdderBossType.BobMoorley)
                SpawnBossChickenEscort(bossObject.transform.position, 3);
            else if (bossType == UdderBossType.HughHoofner)
                SpawnBossCowEscort(bossObject.transform.position, 5);
            else if (bossType == UdderBossType.Beeatrice)
                SpawnBeeatriceSwarm(enemy, bossObject.transform.position, 12);

            DisplayText("BOSS: " + bossName.ToUpperInvariant(), levelText, player.transform.position + Vector3.up * 1.2f);
        }

        private void SpawnHolyCow()
        {
            bossActive = false;
            bossWaveCompleted = wave;

            Vector2 center = player.transform.position;
            GameObject holyCow = InstantiateOrCreate(bossEnemyPrefab, "The Holy Cow");
            holyCow.transform.position = center + Vector2.up * 3.2f;

            var renderer = EnsureComponent<SpriteRenderer>(holyCow);
            renderer.sprite = bossCowSprite ? bossCowSprite : cowSprite;
            renderer.sortingOrder = 5;
            renderer.color = new Color(1f, 0.96f, 0.55f);
            ScaleSpriteToHeight(holyCow.transform, renderer.sprite, 1.17f);

            foreach (Collider2D collider in holyCow.GetComponents<Collider2D>())
                collider.enabled = false;

            player.RestoreHealthToFull();
            AddBovinity(Mathf.Max(0f, player.maxBovinity - player.Bovinity));
            DisplayText("THE HOLY COW BLESSES YOU", levelText, player.transform.position + Vector3.up * 1.2f, true);
            StartCoroutine(HolyCowLeaves(holyCow));
            TryQueueNextWave();
        }

        private IEnumerator HolyCowLeaves(GameObject holyCow)
        {
            float timer = 1.6f;
            while (timer > 0f && holyCow)
            {
                timer -= Time.unscaledDeltaTime;
                holyCow.transform.position += Vector3.up * (1.4f * Time.unscaledDeltaTime);
                yield return null;
            }

            if (holyCow)
                Destroy(holyCow);
        }

        private void SpawnBossChickenEscort(Vector3 bossPosition, int count)
        {
            for (int i = 0; i < count; i++)
                SpawnBossEscort(bossPosition, true, i, count);
        }

        private void SpawnBossCowEscort(Vector3 bossPosition, int count)
        {
            for (int i = 0; i < count; i++)
                SpawnBossEscort(bossPosition, false, i, count);
        }

        private void SpawnBeeatriceSwarm(UdderEnemy queen, Vector3 queenPosition, int count)
        {
            var drones = new List<UdderBeeDrone>(count);
            for (int i = 0; i < count; i++)
            {
                float angle = (i / Mathf.Max(1f, count)) * Mathf.PI * 2f;
                Vector2 offset = new(Mathf.Cos(angle), Mathf.Sin(angle));
                bool fromPrefab = beeDronePrefab;
                GameObject droneObject = InstantiateOrCreate(beeDronePrefab, "BEEatrice Drone");
                droneObject.transform.position = queenPosition + (Vector3)(offset * 1.45f);

                var renderer = EnsureComponent<SpriteRenderer>(droneObject);
                if (!fromPrefab)
                {
                    renderer.sprite = beeDroneSprite ? beeDroneSprite : beeatriceSprite;
                    renderer.color = Color.white;
                    renderer.sortingOrder = 5;
                    ScaleSpriteToHeight(droneObject.transform, renderer.sprite, 0.39f);
                }

                var body = EnsureComponent<Rigidbody2D>(droneObject);
                body.gravityScale = 0f;
                body.bodyType = RigidbodyType2D.Kinematic;
                body.freezeRotation = true;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                var collider = EnsureComponent<CircleCollider2D>(droneObject);
                if (!fromPrefab)
                {
                    collider.radius = 0.12f;
                    collider.offset = Vector2.zero;
                }

                var enemy = EnsureComponent<UdderEnemy>(droneObject);
                enemy.enemyKind = UdderEnemyKind.Bee;
                enemy.downSprite = beeDroneSprite;
                enemy.sideSprite = beeDroneSprite;
                enemy.upSprite = beeDroneSprite;
                enemy.rawMilkFlySprite = rawMilkFlySprite;
                enemy.cottonDeathSprite = cottonDeathSprite;
                enemy.skullDeathSprite = skullDeathSprite;
                enemy.prionAngrySprite = prionAngrySprite;
                enemy.prionIndicatorPrefab = prionIndicatorPrefab;
                enemy.avoidsWater = true;
                enemy.isFlying = true;
                enemy.IsBoss = false;
                if (!fromPrefab)
                {
                    enemy.maxHealth = 12f;
                    enemy.speed = 1.55f;
                    enemy.contactDamage = 7f;
                    enemy.creamValue = 1;
                }
                enemy.Init(this, player, 1f + wave * 0.11f, 1f);
                enemies.Add(enemy);

                var drone = EnsureComponent<UdderBeeDrone>(droneObject);
                drone.Init(enemy, player, queen.transform, i, count);
                drones.Add(drone);
            }

            var swarm = EnsureComponent<UdderBeeSwarmController>(queen.gameObject);
            swarm.Init(queen, drones);
        }

        private void SpawnBossEscort(Vector3 bossPosition, bool chicken, int index, int count)
        {
            float angle = (index / Mathf.Max(1f, count)) * Mathf.PI * 2f;
            Vector2 offset = new(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject prefab = chicken ? chickenEnemyPrefab : bossEnemyPrefab;
            GameObject escortObject = InstantiateOrCreate(prefab, chicken ? "Bob Moorley Chicken" : "Hugh Hoofner Cow");
            escortObject.transform.position = bossPosition + (Vector3)(offset * 1.45f);

            var renderer = EnsureComponent<SpriteRenderer>(escortObject);
            renderer.sprite = chicken ? chickenSprite : bossCowSprite ? bossCowSprite : cowSprite;
            renderer.color = chicken ? Color.white : new Color(1f, 0.9f, 0.9f);
            renderer.sortingOrder = 4;
            ScaleSpriteToHeight(escortObject.transform, renderer.sprite, chicken ? 0.39f : 0.95f);

            var body = EnsureComponent<Rigidbody2D>(escortObject);
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = EnsureComponent<CircleCollider2D>(escortObject);
            collider.radius = chicken ? 0.12f : 0.16f;
            collider.offset = Vector2.zero;

            var enemy = EnsureComponent<UdderEnemy>(escortObject);
            enemy.enemyKind = chicken ? UdderEnemyKind.DebtChicken : UdderEnemyKind.Cow;
            enemy.downSprite = chicken ? chickenDownSprite : bossCowDownSprite ? bossCowDownSprite : cowDownSprite;
            enemy.sideSprite = chicken ? chickenSideSprite : bossCowSideSprite ? bossCowSideSprite : cowSideSprite;
            enemy.upSprite = chicken ? chickenUpSprite : bossCowUpSprite ? bossCowUpSprite : cowUpSprite;
            enemy.rawMilkFlySprite = rawMilkFlySprite;
            enemy.cottonDeathSprite = cottonDeathSprite;
            enemy.skullDeathSprite = skullDeathSprite;
            enemy.prionAngrySprite = prionAngrySprite;
            enemy.prionIndicatorPrefab = prionIndicatorPrefab;
            enemy.avoidsWater = true;
            enemy.IsBoss = false;
            enemy.maxHealth = chicken ? 16f : 42f;
            enemy.speed = chicken ? 1.3f : 1.12f;
            enemy.contactDamage = chicken ? 8f : 11f;
            enemy.creamValue = chicken ? 2 : 5;
            enemy.Init(this, player, 1f + wave * 0.12f, 1f);
            enemies.Add(enemy);
        }

        private static void ShowEnemyCowSpawnTaunt(Transform cow, float heightOffset)
        {
            var tauntObject = new GameObject("Enemy Cow Taunt");
            tauntObject.transform.SetParent(cow, false);
            tauntObject.transform.localPosition = new Vector3(0f, heightOffset, 0f);

            var label = tauntObject.AddComponent<TextMeshPro>();
            ApplyPixelFont(label);
            label.text = "I have beef with you!";
            label.color = new Color(1f, 0.17f, 0.08f);
            label.fontSize = 0.36f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 9;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            var timedText = tauntObject.AddComponent<UdderTimedWorldText>();
            timedText.label = label;
            timedText.lingerTime = 3f;
        }

        private static void DestroyActiveDolphins()
        {
            UdderDolphinSurface[] dolphins = Object.FindObjectsByType<UdderDolphinSurface>(FindObjectsSortMode.None);
            foreach (UdderDolphinSurface dolphin in dolphins)
                Object.Destroy(dolphin.gameObject);
        }

        private void CleanupEnemyList()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i] || !enemies[i].IsAlive)
                {
                    enemies.RemoveAt(i);
                    enemiesLeftInWave = Mathf.Max(0, enemiesLeftInWave - 1);
                }
            }

            if (!bossActive && enemiesPendingSpawn <= 0 && enemies.Count == 0)
                enemiesLeftInWave = 0;
        }

        private void CleanupAlliedCows()
        {
            for (int i = alliedCows.Count - 1; i >= 0; i--)
            {
                if (!alliedCows[i] || !alliedCows[i].IsAlive)
                    alliedCows.RemoveAt(i);
            }
        }

        public UdderEnemy FindNearestEnemy(Vector3 position, float range)
        {
            CleanupEnemyList();
            UdderEnemy nearest = null;
            float nearestDistance = range * range;
            for (int i = 0; i < enemies.Count; i++)
            {
                UdderEnemy enemy = enemies[i];
                if (!enemy || !enemy.IsAlive || enemy.IsInvulnerable)
                    continue;

                float distance = (enemy.transform.position - position).sqrMagnitude;
                if (distance <= nearestDistance)
                {
                    nearest = enemy;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        private void RepositionStrandedEnemies()
        {
            if (!player)
                return;

            Vector3 playerPosition = player.transform.position;
            float maxDistance = GetVisibleEnemyLeashDistance();
            float maxDistanceSqr = maxDistance * maxDistance;

            foreach (UdderEnemy enemy in enemies)
            {
                if (!enemy || !enemy.IsAlive)
                    continue;

                Vector3 delta = enemy.transform.position - playerPosition;
                if (delta.sqrMagnitude <= maxDistanceSqr && !IsInWater(enemy.transform.position, 0.45f))
                    continue;

                enemy.transform.position = GetEnemySpawnPositionNearPlayer();
                if (enemy.TryGetComponent(out Rigidbody2D body))
                    body.linearVelocity = Vector2.zero;
            }
        }

        private float GetVisibleEnemyLeashDistance()
        {
            if (!worldCamera)
                return 18f;

            float halfHeight = worldCamera.orthographicSize;
            float halfWidth = halfHeight * worldCamera.aspect;
            return Mathf.Max(halfWidth, halfHeight) + 4f;
        }

        private void CleanupSeaUrchins()
        {
            groundedSeaUrchins = 0;
            for (int i = seaUrchins.Count - 1; i >= 0; i--)
            {
                if (!seaUrchins[i] || !seaUrchins[i].IsAlive)
                {
                    seaUrchins.RemoveAt(i);
                    continue;
                }

                if (seaUrchins[i].IsGrounded)
                    groundedSeaUrchins++;
            }
        }

        private static void ScaleSpriteToHeight(Transform target, Sprite sprite, float worldHeight)
        {
            if (!sprite || sprite.bounds.size.y <= 0f)
            {
                target.localScale = Vector3.one * 5f;
                return;
            }

            float scale = worldHeight / sprite.bounds.size.y;
            target.localScale = Vector3.one * scale;
        }

        private static void ScaleSpriteToWorldSize(Transform target, Sprite sprite, Vector2 worldSize)
        {
            if (!sprite || sprite.bounds.size.x <= 0f || sprite.bounds.size.y <= 0f)
            {
                target.localScale = Vector3.one;
                return;
            }

            target.localScale = new Vector3(worldSize.x / sprite.bounds.size.x, worldSize.y / sprite.bounds.size.y, 1f);
        }

        private void SpawnPickup(Vector3 position, PickupType type)
        {
            bool fromPrefab = pickupPrefab;
            GameObject pickup = InstantiateOrCreate(pickupPrefab, type.ToString());
            pickup.transform.position = position + (Vector3)Random.insideUnitCircle * 0.45f;
            pickup.transform.localScale = Vector3.one * 1.8f;
            var renderer = EnsureComponent<SpriteRenderer>(pickup);
            renderer.sprite = type switch
            {
                PickupType.Cranberry => cranberrySprite ? cranberrySprite : creamSprite,
                PickupType.Strawberry => strawberrySprite ? strawberrySprite : creamSprite,
                PickupType.Raspberry => raspberrySprite ? raspberrySprite : creamSprite,
                PickupType.Blackberries => blackberriesSprite ? blackberriesSprite : creamSprite,
                PickupType.DairyDouble => cheeseSprite ? cheeseSprite : bottleSprite,
                PickupType.MinorMoona => minorMoonaSprite ? minorMoonaSprite : creamSprite,
                PickupType.NormalMoona => normalMoonaSprite ? normalMoonaSprite : creamSprite,
                PickupType.RemarkableMoona => remarkableMoonaSprite ? remarkableMoonaSprite : creamSprite,
                PickupType.ElysianMoona => elysianMoonaSprite ? elysianMoonaSprite : creamSprite,
                PickupType.Honeycomb => honeycombSprite ? honeycombSprite : elysianMoonaSprite,
                _ => creamSprite ? creamSprite : cheeseSprite,
            };
            renderer.color = type switch
            {
                PickupType.DairyDouble => new Color(1f, 0.92f, 0.25f),
                _ => Color.white,
            };
            renderer.sortingOrder = 6;

            var collider = EnsureComponent<CircleCollider2D>(pickup);
            collider.isTrigger = true;
            if (!fromPrefab)
                collider.radius = 0.22f;

            var pickupComponent = EnsureComponent<UdderPickup>(pickup);
            pickupComponent.type = type;
            pickupComponent.amount = type switch
            {
                PickupType.MinorMoona => 1,
                PickupType.NormalMoona => 2,
                PickupType.RemarkableMoona => 5,
                PickupType.ElysianMoona => 10,
                PickupType.Honeycomb => 10,
                _ => 1,
            };
        }

        private void DisplayText(string text, TextAnimation animation, Vector3 worldPosition, bool nomadic = false)
        {
            if (!animation || !worldCamera || PixelBattleTextController.singleton == null)
                return;

            Vector3 viewport = worldCamera.WorldToViewportPoint(worldPosition);
            // Anti-overlap placement is temporarily disabled while damage numbers are hidden.
            // Vector2 position = nomadic
            //     ? FindOpenTextViewportPosition(viewport, text)
            //     : new Vector2(viewport.x, viewport.y);
            // ReserveTextViewportPosition(position, text);
            // viewport.x = position.x;
            // viewport.y = position.y;
            PixelBattleTextController.DisplayText(text, animation, viewport);
        }

        private void DisplayTopScreenText(string text)
        {
            DisplayViewportText(text, levelText, new Vector3(0.5f, 0.88f, 1f));
        }

        private void DisplayBottomScreenText(string text)
        {
            DisplayViewportText(text, levelText, new Vector3(0.5f, 0.12f, 1f));
        }

        private static void DisplayViewportText(string text, TextAnimation animation, Vector3 viewport)
        {
            if (!animation || PixelBattleTextController.singleton == null)
                return;

            PixelBattleTextController.DisplayText(text, animation, viewport);
        }

        private Vector2 FindOpenTextViewportPosition(Vector3 preferredViewport, string text)
        {
            CleanupTextReservations();

            Vector2 preferred = ClampTextViewportPosition(new Vector2(preferredViewport.x, preferredViewport.y), text);
            if (IsTextViewportPositionOpen(preferred, text))
                return preferred;

            float[] radii = { 0.07f, 0.12f, 0.18f, 0.25f, 0.33f };
            for (int radiusIndex = 0; radiusIndex < radii.Length; radiusIndex++)
            {
                float radius = radii[radiusIndex];
                int samples = 8 + radiusIndex * 4;
                for (int i = 0; i < samples; i++)
                {
                    float angle = (i / (float)samples) * Mathf.PI * 2f;
                    Vector2 offset = new(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * 0.62f);
                    Vector2 candidate = ClampTextViewportPosition(preferred + offset, text);
                    if (IsTextViewportPositionOpen(candidate, text))
                        return candidate;
                }
            }

            return preferred;
        }

        private bool IsTextViewportPositionOpen(Vector2 position, string text)
        {
            Rect candidate = GetTextViewportRect(position, text);
            for (int i = 0; i < textReservations.Count; i++)
            {
                if (candidate.Overlaps(textReservations[i].rect))
                    return false;
            }

            return true;
        }

        private Vector2 ClampTextViewportPosition(Vector2 position, string text)
        {
            Vector2 size = EstimateTextViewportSize(text);
            return new Vector2(
                Mathf.Clamp(position.x, size.x * 0.5f, 1f - size.x * 0.5f),
                Mathf.Clamp(position.y, size.y * 0.5f, 1f - size.y * 0.5f));
        }

        private void ReserveTextViewportPosition(Vector2 position, string text)
        {
            CleanupTextReservations();
            textReservations.Add(new TextReservation(GetTextViewportRect(position, text), Time.time + 1.65f));
        }

        private void CleanupTextReservations()
        {
            float now = Time.time;
            for (int i = textReservations.Count - 1; i >= 0; i--)
            {
                if (textReservations[i].expiresAt <= now)
                    textReservations.RemoveAt(i);
            }
        }

        private static Rect GetTextViewportRect(Vector2 position, string text)
        {
            Vector2 size = EstimateTextViewportSize(text);
            return new Rect(position - size * 0.5f, size);
        }

        private static Vector2 EstimateTextViewportSize(string text)
        {
            float width = Mathf.Clamp(0.065f + text.Length * 0.013f, 0.13f, 0.38f);
            return new Vector2(width, 0.11f);
        }

        private readonly struct TextReservation
        {
            public readonly Rect rect;
            public readonly float expiresAt;

            public TextReservation(Rect rect, float expiresAt)
            {
                this.rect = rect;
                this.expiresAt = expiresAt;
            }
        }

        private Vector2 GetNearestWaterCenter(Vector2 position)
        {
            if (waterBodyCenters.Count == 0)
                return waterCenter;

            Vector2 nearest = waterBodyCenters[0];
            float nearestDistance = (nearest - position).sqrMagnitude;
            for (int i = 1; i < waterBodyCenters.Count; i++)
            {
                float distance = (waterBodyCenters[i] - position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = waterBodyCenters[i];
                }
            }

            return nearest;
        }

        private bool TryGetDolphinWaterCenter(Vector2 position, out Vector2 center)
        {
            center = waterCenter;
            if (waterBodyCenters.Count == 0)
                return false;

            List<Vector2> candidates = new();
            for (int i = 0; i < waterBodyCenters.Count; i++)
            {
                Vector2 candidate = waterBodyCenters[i];
                if (IsWaterContaminated(candidate))
                    continue;

                if ((candidate - position).sqrMagnitude > 5f * 5f)
                    continue;

                if (!IsVisibleWorldPoint(candidate))
                    continue;

                candidates.Add(candidate);
            }

            if (candidates.Count == 0)
                return false;

            center = candidates[Random.Range(0, candidates.Count)];
            return true;
        }

        private bool IsVisibleWorldPoint(Vector2 point)
        {
            if (!worldCamera)
                return true;

            Vector3 viewport = worldCamera.WorldToViewportPoint(point);
            return viewport.z > 0f && viewport.x >= 0.04f && viewport.x <= 0.96f && viewport.y >= 0.04f && viewport.y <= 0.96f;
        }

        private Vector2 GetWaterRadii(Vector2 center)
        {
            for (int i = 0; i < waterBodyCenters.Count; i++)
            {
                if ((waterBodyCenters[i] - center).sqrMagnitude < 0.01f)
                    return waterBodyRadii[i];
            }

            return waterRadii;
        }

        private Sprite GetRuntimeSolidSprite()
        {
            if (runtimeSolidSprite)
                return runtimeSolidSprite;

            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            runtimeSolidSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            runtimeSolidSprite.name = "Runtime Solid Butter Tile";
            return runtimeSolidSprite;
        }

        private static GameObject InstantiateOrCreate(GameObject prefab, string fallbackName)
        {
            GameObject instance = prefab ? Instantiate(prefab) : new GameObject(fallbackName);
            instance.name = fallbackName;
            instance.SetActive(true);
            return instance;
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target.TryGetComponent(out T component))
                return component;

            return target.AddComponent<T>();
        }
    }
}
