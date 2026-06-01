using System.Collections;
using System.Collections.Generic;
using PixelBattleText;
using TMPro;
using UnityEngine;
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
        public Sprite rawMilkSprite;
        public Sprite rawMilkFlySprite;
        public Sprite cottonDeathSprite;
        public Sprite skullDeathSprite;
        public Sprite dolphinSprite;
        public Sprite seaUrchinSprite;

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

        private readonly List<UdderEnemy> enemies = new();
        private readonly List<UdderSeaUrchin> seaUrchins = new();
        private readonly List<Vector2> waterBodyCenters = new();
        private readonly List<Vector2> waterBodyRadii = new();
        private readonly List<Vector2> contaminatedWaterCenters = new();
        private readonly List<TextReservation> textReservations = new();
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
        private bool firstBossDefeated;
        private bool choosingPower;
        private int bankedDairyDoubles;
        private Sprite runtimeSolidSprite;
        private UdderHud hud;
        private GameObject gameOverOverlay;
        private GameObject pauseOverlay;
        private GameObject mainMenuOverlay;
        private GameObject auraFarmingObject;
        private Sprite auraCircleSprite;
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
                _ => 7.4f,
            };
            float life = mode switch
            {
                MilkMode.SpoiledMilk => 0.425f,
                MilkMode.RawMilk => 0.6f,
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

            ShowMainMenu();
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
            CleanupSeaUrchins();
            RepositionStrandedEnemies();
            UpdateDolphin();
            UpdateWaveSpawning();

            if (bossPending)
            {
                nextWaveTimer -= Time.deltaTime;
                if (nextWaveTimer <= 0f)
                    SpawnMiyamotoMoosashi();
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
            GameObject shot = new GameObject(mode + " Shot");
            shot.transform.position = origin + (Vector3)(direction.normalized * 0.9f);
            shot.transform.localScale = Vector3.one * 1.8f;
            var renderer = shot.AddComponent<SpriteRenderer>();
            renderer.sprite = GetProjectileSprite(mode);
            renderer.color = mode switch
            {
                MilkMode.SpoiledMilk => new Color(0.55f, 1f, 0.45f),
                _ => Color.white,
            };
            renderer.sortingOrder = 5;

            var collider = shot.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            var body = shot.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;

            var projectile = shot.AddComponent<UdderProjectile>();
            projectile.game = this;
            projectile.mode = mode;
            projectile.powerLevel = Mathf.Max(1, powerLevel);
            projectile.condensedMilkLevel = condensedMilkLevel;
            projectile.damage = mode switch
            {
                MilkMode.Buttermilk => damage * 1.15f,
                MilkMode.RawMilk => damage * 0.45f,
                _ => damage,
            };
            projectile.speed = mode switch
            {
                MilkMode.SpoiledMilk => 6.2f,
                MilkMode.RawMilk => 5.8f,
                _ => 7.4f,
            };
            projectile.life = mode switch
            {
                MilkMode.SpoiledMilk => 0.425f,
                MilkMode.RawMilk => 0.6f,
                _ => 0.55f,
            };
            projectile.Fire(direction);
        }

        private Sprite GetProjectileSprite(MilkMode mode)
        {
            return mode switch
            {
                MilkMode.WholeMilk => wholeMilkSprite ? wholeMilkSprite : bottleSprite,
                MilkMode.Buttermilk => buttermilkSprite ? buttermilkSprite : bottleSprite,
                MilkMode.RawMilk => rawMilkSprite ? rawMilkSprite : bottleSprite,
                _ => bottleSprite,
            };
        }

        public void DeployButter(Vector3 origin, int powerLevel)
        {
            int size = GetButterGridSize(powerLevel);
            float life = GetButterLife(powerLevel);
            Vector3 position = new(Mathf.Round(origin.x), Mathf.Round(origin.y), 0f);
            GameObject slick = new GameObject("Weaponized Butter Slick");
            slick.transform.position = position;
            slick.transform.localScale = new Vector3(size, size, 1f);

            var renderer = slick.AddComponent<SpriteRenderer>();
            renderer.sprite = butterSprite ? butterSprite : GetRuntimeSolidSprite();
            renderer.color = new Color(1f, 0.84f, 0.12f, 0.78f);
            renderer.sortingOrder = -1;

            var collider = slick.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            var butter = slick.AddComponent<UdderButterSlick>();
            butter.life = life;
        }

        public void DeployDairyAir(Vector3 origin, int powerLevel, float interval)
        {
            GameObject cloud = new GameObject("Dairy Air Cloud");
            cloud.transform.position = origin;

            var renderer = cloud.AddComponent<SpriteRenderer>();
            renderer.sprite = GetRuntimeSolidSprite();
            renderer.color = new Color(1f, 1f, 1f, 0f);
            renderer.sortingOrder = 3;

            var collider = cloud.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.55f;

            BuildDairyAirSprites(cloud.transform);
            var dairyAir = cloud.AddComponent<UdderDairyAirCloud>();
            dairyAir.life = GetDairyAirCloudLife(powerLevel, interval);
        }

        private void BuildDairyAirSprites(Transform parent)
        {
            Sprite sprite = dairyAirSprite ? dairyAirSprite : GetRuntimeSolidSprite();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    GameObject tile = new("Dairy Air Puff");
                    tile.transform.SetParent(parent, false);
                    tile.transform.localPosition = new Vector3(x * 0.34f, y * 0.34f, 0f);
                    tile.transform.localScale = Vector3.one * 0.72f;
                    var tileRenderer = tile.AddComponent<SpriteRenderer>();
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

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (!enemies[i] || !enemies[i].IsAlive)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

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
            Vector2 away = dolphinPosition - threatPosition;
            if (away.sqrMagnitude < 0.01f)
                away = dolphinPosition - center;
            if (away.sqrMagnitude < 0.01f)
                away = Vector2.up;

            if (Mathf.Abs(away.x) >= Mathf.Abs(away.y))
            {
                float sign = Mathf.Sign(away.x);
                beachPosition = new Vector2(
                    center.x + sign * (radii.x + 0.95f),
                    Mathf.Clamp(dolphinPosition.y, center.y - radii.y, center.y + radii.y));
            }
            else
            {
                float sign = Mathf.Sign(away.y);
                beachPosition = new Vector2(
                    Mathf.Clamp(dolphinPosition.x, center.x - radii.x, center.x + radii.x),
                    center.y + sign * (radii.y + 0.95f));
            }

            beachPosition = ClampToVisibleField(beachPosition, 0.6f);
            return true;
        }

        public Vector2 GetWaterBlockedMove(Vector2 position, Vector2 desiredMove)
        {
            return desiredMove;
        }

        public bool TryGetWaterAvoidance(Vector2 position, Vector2 desiredDirection, out Vector2 adjustedDirection)
        {
            Vector2 lookAhead = position + desiredDirection.normalized * 0.42f;
            if (!IsInsideWater(lookAhead, 0f) && !IsInsideWater(position, 0f))
            {
                adjustedDirection = desiredDirection;
                return false;
            }

            Vector2 water = GetNearestWaterCenter(position);
            Vector2 away = position - water;
            if (away.sqrMagnitude < 0.01f)
                away = Vector2.up;

            Vector2 tangent = new(-away.y, away.x);
            if (Vector2.Dot(tangent, desiredDirection) < 0f)
                tangent = -tangent;

            adjustedDirection = (tangent.normalized * 0.92f + away.normalized * 0.25f).normalized;
            return true;
        }

        public bool IsInWater(Vector2 position, float padding)
        {
            return IsInsideWater(position, padding);
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
        }

        public void SetAuraFarmingActive(Transform owner, float radius)
        {
            if (!owner || radius <= 0f)
            {
                if (auraFarmingObject)
                    auraFarmingObject.SetActive(false);
                return;
            }

            if (!auraFarmingObject)
                auraFarmingObject = CreateAuraFarmingObject(owner);

            auraFarmingObject.SetActive(true);
            auraFarmingObject.transform.SetParent(owner, false);
            auraFarmingObject.transform.localPosition = new Vector3(0f, -0.18f, 0f);
            auraFarmingObject.transform.localScale = Vector3.one * (radius * 2f);
        }

        public void PulseAuraDamage(Vector3 position, float radius, float damage)
        {
            float radiusSqr = radius * radius;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                UdderEnemy enemy = enemies[i];
                if (!enemy || !enemy.IsAlive)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                if ((enemy.transform.position - position).sqrMagnitude <= radiusSqr)
                    enemy.TakeDamage(damage, MilkMode.Aura, false);
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

            target.ApplyPrionPulse(GetPrionDuration(level), GetPrionSpreadChance(level));
            ShowCowText(target.transform.position, "Prion Pulse!");
            return true;
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

        public void TrySpreadPrionFrom(UdderEnemy source, Vector3 position, float spreadChance)
        {
            if (spreadChance <= 0f || Random.value >= spreadChance)
                return;

            UdderEnemy target = null;
            float nearestDistance = float.PositiveInfinity;
            float rangeSqr = 3.5f * 3.5f;
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
                if (distance <= rangeSqr && distance < nearestDistance)
                {
                    target = enemy;
                    nearestDistance = distance;
                }
            }

            if (target)
                target.ApplyPrionPulse(3f, spreadChance);
        }

        private static float GetPrionDuration(int level)
        {
            return 3f + Mathf.Max(0, level - 1) * 0.5f;
        }

        private static float GetPrionSpreadChance(int level)
        {
            return 0.1f + Mathf.Max(0, level - 1) * 0.01f;
        }

        private GameObject CreateAuraFarmingObject(Transform owner)
        {
            GameObject aura = new("Aura Farming Field");
            aura.transform.SetParent(owner, false);
            var renderer = aura.AddComponent<SpriteRenderer>();
            renderer.sprite = GetAuraCircleSprite();
            renderer.color = new Color(1f, 0.98f, 0.88f, 0.34f);
            renderer.sortingOrder = -1;

            var particles = aura.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.startColor = new Color(1f, 0.97f, 0.78f, 0.72f);
            main.startLifetime = 0.85f;
            main.startSpeed = 0.28f;
            main.startSize = 0.055f;
            main.maxParticles = 40;
            ParticleSystem.EmissionModule emission = particles.emission;
            emission.rateOverTime = 18f;
            ParticleSystem.ShapeModule shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.42f;
            return aura;
        }

        private Sprite GetAuraCircleSprite()
        {
            if (auraCircleSprite)
                return auraCircleSprite;

            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.46f;
            float inner = size * 0.39f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float ring = Mathf.Clamp01((outer - distance) / 2.2f) * Mathf.Clamp01((distance - inner) / 2.2f);
                    float fill = distance < inner ? 0.2f : 0f;
                    texture.SetPixel(x, y, new Color(1f, 0.98f, 0.9f, Mathf.Max(ring, fill)));
                }
            }

            texture.Apply();
            auraCircleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            auraCircleSprite.name = "Runtime Aura Farming Circle";
            return auraCircleSprite;
        }

        private bool IsInsideWater(Vector2 position, float padding)
        {
            if (waterBodyCenters.Count == 0)
                SetWaterBodies(System.Array.Empty<Vector2>(), System.Array.Empty<Vector2>());

            for (int i = 0; i < waterBodyCenters.Count; i++)
            {
                Vector2 delta = position - waterBodyCenters[i];
                Vector2 radii = waterBodyRadii[i];
                if (Mathf.Abs(delta.x) <= radii.x + padding && Mathf.Abs(delta.y) <= radii.y + padding)
                    return true;
            }

            return false;
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
            GameObject pool = new GameObject("Spoiled Milk Puddle");
            pool.transform.position = position;
            pool.transform.localScale = Vector3.one * (1.7f * sizeMultiplier);
            var renderer = pool.AddComponent<SpriteRenderer>();
            renderer.sprite = spoiledPuddleSprite ? spoiledPuddleSprite : wholeMilkSprite ? wholeMilkSprite : creamSprite ? creamSprite : GetRuntimeSolidSprite();
            renderer.color = spoiledPuddleIdleController ? new Color(1f, 0.92f, 0.25f, 0.82f) : new Color(1f, 0.84f, 0.12f, 0.62f);
            renderer.sortingOrder = 1;
            if (spoiledPuddleIdleController)
            {
                var animator = pool.AddComponent<Animator>();
                animator.runtimeAnimatorController = spoiledPuddleIdleController;
            }
            var collider = pool.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.75f;
            var puddle = pool.AddComponent<UdderHazardPool>();
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
            enemiesLeftInWave = Mathf.Max(0, enemiesLeftInWave - 1);
            cream += enemy.creamValue;
            AddBovinity(wave);
            if (defeatedBoss)
                DisplayText("BOSS DEFEATED!", koText, enemy.transform.position + Vector3.up * 0.6f);

            if (defeatedBoss)
            {
                bossActive = false;
                firstBossDefeated = true;
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
            CreateMainMenuButton(panel.transform, "Start Game", new Vector2(0f, 70f), StartGame);
            CreateMainMenuButton(panel.transform, "Sound Settings", new Vector2(0f, 0f), null);
            CreateMainMenuButton(panel.transform, "Info", new Vector2(0f, -70f), null);
            CreateMainMenuButton(panel.transform, "About", new Vector2(0f, -140f), null);
            CreateMainMenuButton(panel.transform, "Exit Game", new Vector2(0f, -210f), ExitGame);
        }

        private void StartGame()
        {
            mainMenuActive = false;
            Time.timeScale = 1f;

            if (mainMenuOverlay)
            {
                Destroy(mainMenuOverlay);
                mainMenuOverlay = null;
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
                if (player.GetPowerLevel(power) < 10)
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

        private void ChoosePower(UdderPower power)
        {
            int level = player.GetPowerLevel(power);
            int gain = 1;

            if (bankedDairyDoubles > 0 && level + 2 <= 10)
            {
                bankedDairyDoubles--;
                gain = 2;
                DisplayText("DAIRY DOUBLE!", levelText, player.transform.position + Vector3.up * 1.3f);
            }

            int roll = Random.Range(1, 101);
            if (roll == 100)
            {
                if (gain == 1 && level + 2 <= 10)
                {
                    gain = 2;
                    DisplayText("DAIRY DOUBLE!", levelText, player.transform.position + Vector3.up * 1.3f);
                }
                else
                {
                    bankedDairyDoubles++;
                    DisplayText("DAIRY DOUBLE BANKED!", levelText, player.transform.position + Vector3.up * 1.3f);
                }
            }

            player.AddPowerLevel(power, gain);
            DisplayText(UdderPlayer.GetPowerLabel(power).ToUpperInvariant() + " +" + gain, levelText, player.transform.position + Vector3.up);
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
            string label = crit ? "CRIT " + Mathf.CeilToInt(amount) : Mathf.CeilToInt(amount).ToString();
            TextAnimation animation = mode switch
            {
                MilkMode.Buttermilk => acidText,
                MilkMode.SpoiledMilk => poisonText,
                MilkMode.RawMilk => poisonText,
                MilkMode.Prion => poisonText,
                MilkMode.Aura => healText,
                _ => crit ? critText : damageText,
            };
            DisplayText(label, animation, worldPosition + Vector3.up * 0.45f);
        }

        public void ShowCowText(Vector3 worldPosition, string text)
        {
            DisplayText(text, healText, worldPosition + Vector3.up * 0.8f, true);
        }

        public void ShowHealText(Vector3 worldPosition, float amount)
        {
            string label = amount > 0f ? "+" + Mathf.CeilToInt(amount).ToString() : "HP Full!";
            DisplayText(label, healText, worldPosition + Vector3.up * 0.8f, true);
        }

        public void ShowCheesedItText(Transform cow)
        {
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

            TMP_Text dontCry = CreateGameOverText(panel.transform, "Don't cry", new Vector2(0f, 86f), 62);
            yield return FadeOutGameOverText(dontCry, 1f);

            CreateGameOverText(panel.transform, "Over", new Vector2(0f, 0f), 96);

            TMP_Text spilledMilk = CreateGameOverText(panel.transform, "Spilled Milk", new Vector2(0f, -92f), 62);
            yield return FadeOutGameOverText(spilledMilk, 1f);

            CreateGameOverText(panel.transform, "Game", new Vector2(0f, 98f), 96);
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
            rect.sizeDelta = new Vector2(960f, 130f);

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
            bool isChicken = Random.value >= GetPigWaveRatio();
            GameObject enemyObject = new GameObject(isChicken ? "Debt Chicken" : "Hostile Ham");
            enemyObject.transform.position = GetEnemySpawnPositionNearPlayer();
            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sprite = isChicken ? chickenSprite : pigSprite;
            renderer.sortingOrder = 4;
            renderer.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.55f), wave * 0.04f);
            ScaleSpriteToHeight(enemyObject.transform, renderer.sprite, isChicken ? 0.39f : 0.78f);

            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = enemyObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.28f;

            var enemy = enemyObject.AddComponent<UdderEnemy>();
            enemy.downSprite = isChicken ? chickenDownSprite : pigDownSprite;
            enemy.sideSprite = isChicken ? chickenSideSprite : pigSideSprite;
            enemy.upSprite = isChicken ? chickenUpSprite : pigUpSprite;
            enemy.rawMilkFlySprite = rawMilkFlySprite;
            enemy.cottonDeathSprite = cottonDeathSprite;
            enemy.skullDeathSprite = skullDeathSprite;
            enemy.avoidsWater = true;
            enemy.maxHealth = isChicken ? 8f : Random.value < 0.25f ? 22f : 14f;
            enemy.speed = isChicken ? Random.Range(1.05f, 1.35f) : Random.Range(1.45f, 1.85f);
            enemy.creamValue = isChicken ? Random.Range(1, 3) : Random.Range(2, 5);
            enemy.Init(this, player, 1f + wave * 0.13f, GetEnemySpeedScale());
            enemies.Add(enemy);
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

            Vector2 radii = GetWaterRadii(center);
            Vector2 local = new(Random.Range(-0.82f, 0.82f), Random.Range(-0.82f, 0.82f));
            Vector3 surfacePosition = new(
                center.x + local.x * radii.x,
                center.y + local.y * radii.y,
                0f);

            SpawnDolphinSurface(surfacePosition);

            float edgeAmount = Mathf.Max(Mathf.Abs(local.x), Mathf.Abs(local.y));
            if (edgeAmount >= 0.68f && player)
                LaunchSeaUrchin(surfacePosition, player.transform.position);
        }

        private void SpawnDolphinSurface(Vector3 position)
        {
            GameObject dolphin = new("Pond Dolphin");
            dolphin.transform.position = position;
            var renderer = dolphin.AddComponent<SpriteRenderer>();
            renderer.sprite = dolphinSprite ? dolphinSprite : GetRuntimeSolidSprite();
            renderer.color = dolphinSprite ? Color.white : new Color(0.4f, 0.85f, 1f);
            renderer.sortingOrder = 4;
            ScaleSpriteToHeight(dolphin.transform, renderer.sprite, 1.2f);
            var collider = dolphin.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.32f;
            var body = dolphin.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            var surface = dolphin.AddComponent<UdderDolphinSurface>();
            surface.game = this;
            surface.cottonDeathSprite = cottonDeathSprite;
            surface.skullDeathSprite = skullDeathSprite;
        }

        private void LaunchSeaUrchin(Vector3 origin, Vector3 target)
        {
            GameObject urchinObject = new("Hostile Sea Urchin");
            urchinObject.transform.position = origin;
            var renderer = urchinObject.AddComponent<SpriteRenderer>();
            renderer.sprite = seaUrchinSprite ? seaUrchinSprite : GetRuntimeSolidSprite();
            renderer.color = seaUrchinSprite ? Color.white : new Color(0.2f, 0.05f, 0.25f);
            renderer.sortingOrder = 6;
            ScaleSpriteToHeight(urchinObject.transform, renderer.sprite, 0.65f);

            var collider = urchinObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.18f;
            var body = urchinObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;

            var seaUrchin = urchinObject.AddComponent<UdderSeaUrchin>();
            seaUrchin.Init(this, target);
            seaUrchins.Add(seaUrchin);
            ShowCowText(origin, "URCHIN!");
        }

        private void StartWave()
        {
            int count = Mathf.CeilToInt((4 + wave * 2) * GetWaveEnemyCountMultiplier());
            enemiesLeftInWave = count;
            enemiesPendingSpawn = count;
            waveSpawnTimer = 0f;
            nextWaveTimer = 0f;
            DisplayText("WAVE " + wave, levelText, player.transform.position + Vector3.up * 1.2f);
        }

        private void UpdateWaveSpawning()
        {
            if (bossPending || bossActive || enemiesPendingSpawn <= 0)
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

            if (!firstBossDefeated && wave >= 10)
            {
                bossPending = true;
                nextWaveTimer = 3f;
                dolphinTimer = 5f;
                DisplayText("MIYAMOTO MOOSASHI APPROACHES!", levelText, player.transform.position + Vector3.up * 1.1f);
                return;
            }

            nextWaveTimer = 5f;
            wave++;
            DisplayText("WAVE CLEARED!", levelText, player.transform.position + Vector3.up * 1.1f);
        }

        private float GetPigWaveRatio()
        {
            if (wave < 5)
                return 0f;
            if (wave <= 10)
                return 0.1f;

            int decadeSteps = Mathf.Clamp((wave - 1) / 10, 1, 4);
            return 0.1f + decadeSteps * 0.1f;
        }

        private float GetWaveSpawnInterval()
        {
            return Mathf.Max(0.32f, 1.65f - (wave - 1) * 0.055f);
        }

        private float GetEnemySpeedScale()
        {
            return Mathf.Lerp(0.78f, 1.28f, Mathf.Clamp01((wave - 1) / 24f));
        }

        private float GetWaveEnemyCountMultiplier()
        {
            return player ? 1f + player.GetPowerLevel(UdderPower.MoreCowbell) * 0.1f : 1f;
        }

        private float GetDropChanceMultiplier()
        {
            return player ? 1f + player.GetPowerLevel(UdderPower.MoreCowbell) * 0.1f : 1f;
        }

        private static int GetButterGridSize(int powerLevel)
        {
            return Mathf.Clamp(1 + powerLevel / 2, 1, 4);
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

        private void SpawnMiyamotoMoosashi()
        {
            bossPending = false;
            bossActive = true;
            enemiesLeftInWave = 1;
            dolphinTimer = 5f;
            DestroyActiveDolphins();

            Vector2 center = player.transform.position;
            Vector2 offset = Random.insideUnitCircle.normalized * 8f;
            GameObject bossObject = new("Miyamoto Moosashi");
            bossObject.transform.position = center + offset;

            var renderer = bossObject.AddComponent<SpriteRenderer>();
            renderer.sprite = bossCowSprite ? bossCowSprite : cowSprite;
            renderer.sortingOrder = 5;
            renderer.color = new Color(1f, 0.93f, 0.93f);
            ScaleSpriteToHeight(bossObject.transform, renderer.sprite, 1.17f);

            var body = bossObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = bossObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.34f;

            var nameObject = new GameObject("Boss Name");
            nameObject.transform.SetParent(bossObject.transform, false);
            nameObject.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            var label = nameObject.AddComponent<TextMeshPro>();
            ApplyPixelFont(label);
            label.text = "Miyamoto Moosashi";
            label.color = new Color(1f, 0.05f, 0.05f);
            label.fontSize = 0.42f;
            label.alignment = TextAlignmentOptions.Center;
            label.sortingOrder = 8;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            ShowEnemyCowSpawnTaunt(bossObject.transform, 0.98f);

            var enemy = bossObject.AddComponent<UdderEnemy>();
            enemy.downSprite = bossCowDownSprite ? bossCowDownSprite : cowDownSprite;
            enemy.sideSprite = bossCowSideSprite ? bossCowSideSprite : cowSideSprite;
            enemy.upSprite = bossCowUpSprite ? bossCowUpSprite : cowUpSprite;
            enemy.rawMilkFlySprite = rawMilkFlySprite;
            enemy.cottonDeathSprite = cottonDeathSprite;
            enemy.skullDeathSprite = skullDeathSprite;
            enemy.avoidsWater = true;
            enemy.IsBoss = true;
            enemy.maxHealth = 520f;
            enemy.speed = 1.55f;
            enemy.contactDamage = 16f;
            enemy.creamValue = 30;
            enemy.Init(this, player, 1f + wave * 0.18f, 1f);
            enemies.Add(enemy);

            DisplayText("BOSS: MIYAMOTO MOOSASHI", levelText, player.transform.position + Vector3.up * 1.2f);
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

        private void SpawnPickup(Vector3 position, PickupType type)
        {
            GameObject pickup = new GameObject(type.ToString());
            pickup.transform.position = position + (Vector3)Random.insideUnitCircle * 0.45f;
            pickup.transform.localScale = Vector3.one * 1.8f;
            var renderer = pickup.AddComponent<SpriteRenderer>();
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
                _ => creamSprite ? creamSprite : cheeseSprite,
            };
            renderer.color = type switch
            {
                PickupType.DairyDouble => new Color(1f, 0.92f, 0.25f),
                _ => Color.white,
            };
            renderer.sortingOrder = 6;

            var collider = pickup.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;

            var pickupComponent = pickup.AddComponent<UdderPickup>();
            pickupComponent.type = type;
            pickupComponent.amount = type switch
            {
                PickupType.MinorMoona => 1,
                PickupType.NormalMoona => 2,
                PickupType.RemarkableMoona => 5,
                PickupType.ElysianMoona => 10,
                _ => 1,
            };
        }

        private void DisplayText(string text, TextAnimation animation, Vector3 worldPosition, bool nomadic = false)
        {
            if (!animation || !worldCamera || PixelBattleTextController.singleton == null)
                return;

            Vector3 viewport = worldCamera.WorldToViewportPoint(worldPosition);
            Vector2 position = nomadic
                ? FindOpenTextViewportPosition(viewport, text)
                : new Vector2(viewport.x, viewport.y);
            ReserveTextViewportPosition(position, text);
            viewport.x = position.x;
            viewport.y = position.y;
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
            textReservations.Add(new TextReservation(GetTextViewportRect(position, text), Time.time + 1.25f));
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
            float width = Mathf.Clamp(0.055f + text.Length * 0.012f, 0.12f, 0.34f);
            return new Vector2(width, 0.085f);
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
                return !IsWaterContaminated(center);

            bool found = false;
            float nearestDistance = float.PositiveInfinity;
            for (int i = 0; i < waterBodyCenters.Count; i++)
            {
                Vector2 candidate = waterBodyCenters[i];
                if (IsWaterContaminated(candidate))
                    continue;

                float distance = (candidate - position).sqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    center = candidate;
                    found = true;
                }
            }

            return found;
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
    }
}
