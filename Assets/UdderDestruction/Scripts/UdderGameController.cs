using System.Collections.Generic;
using PixelBattleText;
using TMPro;
using UnityEngine;

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

        private readonly List<UdderEnemy> enemies = new();
        private readonly List<UdderSeaUrchin> seaUrchins = new();
        private readonly List<Vector2> waterBodyCenters = new();
        private readonly List<Vector2> waterBodyRadii = new();
        private float runTimer;
        private float nextWaveTimer = 1.2f;
        private float dolphinTimer = 5f;
        private int groundedSeaUrchins;
        private int enemiesLeftInWave;
        private int wave = 1;
        private int cream;
        private bool finished;
        private bool bossPending;
        private bool bossActive;
        private bool firstBossDefeated;
        private Sprite runtimeSolidSprite;

        public float CritChance => 0.11f + wave * 0.005f;
        public int Wave => wave;
        public int Cream => cream;
        public int GroundedSeaUrchins => groundedSeaUrchins;
        public string WaveStatusText => enemiesLeftInWave > 0 ? $"ENEMIES {enemiesLeftInWave}" : $"NEXT {Mathf.CeilToInt(nextWaveTimer)}S";

        private void Start()
        {
            if (!worldCamera)
                worldCamera = Camera.main;

            if (player)
                player.Init(this);
        }

        private void Update()
        {
            if (finished || !player || !player.IsAlive)
                return;

            runTimer += Time.deltaTime;

            CleanupEnemyList();
            CleanupSeaUrchins();
            UpdateDolphin();

            if (bossPending)
            {
                nextWaveTimer -= Time.deltaTime;
                if (nextWaveTimer <= 0f)
                    SpawnMiyamotoMoosashi();
                return;
            }

            if (!bossActive && enemiesLeftInWave <= 0 && enemies.Count == 0)
            {
                nextWaveTimer -= Time.deltaTime;
                if (nextWaveTimer <= 0f)
                    StartWave();
            }
        }

        public void FireMilk(Vector3 origin, Vector2 direction, MilkMode mode, float damage)
        {
            GameObject shot = new GameObject(mode + " Shot");
            shot.transform.position = origin + (Vector3)(direction.normalized * 0.9f);
            shot.transform.localScale = Vector3.one * 1.8f;
            var renderer = shot.AddComponent<SpriteRenderer>();
            renderer.sprite = bottleSprite;
            renderer.color = mode switch
            {
                MilkMode.Buttermilk => new Color(0.9f, 1f, 0.35f),
                MilkMode.SpoiledMilk => new Color(0.55f, 1f, 0.45f),
                MilkMode.RawMilk => new Color(0.95f, 0.96f, 0.82f),
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
                MilkMode.SpoiledMilk => 0.34f,
                MilkMode.RawMilk => 0.5f,
                _ => 0.42f,
            };
            projectile.Fire(direction);
        }

        public void DeployButter(Vector3 origin)
        {
            Vector3 position = new(Mathf.Round(origin.x), Mathf.Round(origin.y), 0f);
            GameObject slick = new GameObject("Weaponized Butter Slick");
            slick.transform.position = position;
            slick.transform.localScale = Vector3.one;

            var renderer = slick.AddComponent<SpriteRenderer>();
            renderer.sprite = butterSprite ? butterSprite : GetRuntimeSolidSprite();
            renderer.color = new Color(1f, 0.84f, 0.12f, 0.72f);
            renderer.sortingOrder = -1;

            var collider = slick.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one;

            slick.AddComponent<UdderButterSlick>();
            ShowCowText(origin, "BUTTER!");
        }

        public void DeployDairyAir(Vector3 origin)
        {
            GameObject cloud = new GameObject("Dairy Air Cloud");
            cloud.transform.position = origin;

            var renderer = cloud.AddComponent<SpriteRenderer>();
            renderer.sprite = GetRuntimeSolidSprite();
            renderer.color = new Color(0.92f, 0.96f, 1f, 0.42f);
            renderer.sortingOrder = 3;

            var collider = cloud.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.55f;

            cloud.AddComponent<UdderDairyAirCloud>();
            ShowCowText(origin, "DAIRY AIR!");
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

        public Vector2 GetWaterBlockedMove(Vector2 position, Vector2 desiredMove)
        {
            if (desiredMove.sqrMagnitude <= 0.01f)
                return desiredMove;

            Vector2 next = position + desiredMove.normalized * 0.42f;
            if (!IsInWater(next, 0.15f))
                return desiredMove;

            Vector2 water = GetNearestWaterCenter(position);
            Vector2 away = position - water;
            if (away.sqrMagnitude < 0.01f)
                away = Vector2.up;

            Vector2 tangent = new(-away.y, away.x);
            if (Vector2.Dot(tangent, desiredMove) < 0f)
                tangent = -tangent;

            return tangent.normalized * Mathf.Clamp01(Vector2.Dot(desiredMove.normalized, tangent.normalized));
        }

        public bool TryGetWaterAvoidance(Vector2 position, Vector2 desiredDirection, out Vector2 adjustedDirection)
        {
            Vector2 lookAhead = position + desiredDirection.normalized * 0.75f;
            if (!IsInsideWater(lookAhead, 0.35f) && !IsInsideWater(position, 0f))
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

            adjustedDirection = (tangent.normalized * 0.85f + away.normalized * 0.55f).normalized;
            return true;
        }

        public bool IsInWater(Vector2 position, float padding)
        {
            return IsInsideWater(position, padding);
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

        public void SpawnSpoiledPool(Vector3 position)
        {
            GameObject pool = new GameObject("Spoiled Milk Puddle");
            pool.transform.position = position;
            pool.transform.localScale = Vector3.one * 1.7f;
            var renderer = pool.AddComponent<SpriteRenderer>();
            renderer.sprite = skullBottleSprite ? skullBottleSprite : bottleSprite;
            renderer.color = new Color(0.35f, 1f, 0.35f, 0.7f);
            renderer.sortingOrder = 2;
            var collider = pool.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.75f;
            pool.AddComponent<UdderHazardPool>();
        }

        public void RegisterEnemyDefeated(UdderEnemy enemy)
        {
            bool defeatedBoss = enemy.IsBoss;
            enemies.Remove(enemy);
            enemiesLeftInWave = Mathf.Max(0, enemiesLeftInWave - 1);
            cream += enemy.creamValue;
            DisplayText(defeatedBoss ? "BOSS DEFEATED!" : Random.value < 0.12f ? "MOO-TALITY!" : "KO", koText, enemy.transform.position + Vector3.up * 0.6f);

            if (defeatedBoss)
            {
                bossActive = false;
                firstBossDefeated = true;
            }
            else
            {
                float roll = Random.value;
                if (roll < 0.2f)
                    SpawnPickup(enemy.transform.position, PickupType.Cream);
                else if (roll < 0.25f)
                    SpawnPickup(enemy.transform.position, PickupType.Heal);
                else if (roll < 0.3f)
                    SpawnPickup(enemy.transform.position, PickupType.Cheese);
                else if (roll < 0.34f)
                    SpawnPickup(enemy.transform.position, PickupType.Buttermilk);
                else if (roll < 0.38f)
                    SpawnPickup(enemy.transform.position, PickupType.SpoiledMilk);
                else if (roll < 0.42f)
                    SpawnPickup(enemy.transform.position, PickupType.RawMilk);
            }

            if (enemiesLeftInWave == 0 && enemies.Count == 0)
            {
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
                player.ApplyUpgrade(Random.Range(0, 3));
                DisplayText("WAVE CLEARED!", levelText, player.transform.position + Vector3.up * 1.1f);
            }
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
                _ => crit ? critText : damageText,
            };
            DisplayText(label, animation, worldPosition + Vector3.up * 0.45f);
        }

        public void ShowCowText(Vector3 worldPosition, string text)
        {
            DisplayText(text, healText, worldPosition + Vector3.up * 0.8f);
        }

        public void GameOver()
        {
            finished = true;
            DisplayText("PASTURE PRIME", koText, player.transform.position + Vector3.up);
        }

        private void SpawnEnemy()
        {
            Vector2 center = player.transform.position;
            Vector2 offset = Random.insideUnitCircle.normalized * Random.Range(7f, 10f);
            bool isChicken = Random.value < 0.45f;
            GameObject enemyObject = new GameObject(isChicken ? "Debt Chicken" : "Hostile Ham");
            enemyObject.transform.position = center + offset;
            var renderer = enemyObject.AddComponent<SpriteRenderer>();
            renderer.sprite = isChicken ? chickenSprite : pigSprite;
            renderer.sortingOrder = 4;
            renderer.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.55f), wave * 0.04f);
            ScaleSpriteToHeight(enemyObject.transform, renderer.sprite, isChicken ? 0.39f : 0.78f);

            var body = enemyObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = enemyObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.28f;

            var enemy = enemyObject.AddComponent<UdderEnemy>();
            enemy.downSprite = isChicken ? chickenDownSprite : pigDownSprite;
            enemy.sideSprite = isChicken ? chickenSideSprite : pigSideSprite;
            enemy.upSprite = isChicken ? chickenUpSprite : pigUpSprite;
            enemy.avoidsWater = true;
            enemy.maxHealth = Random.value < 0.25f ? 18f : 11f;
            enemy.speed = Random.value < 0.35f ? 2.5f : 1.75f;
            enemy.creamValue = Random.Range(1, 4);
            enemy.Init(this, player, 1f + wave * 0.13f, 1f + wave * 0.025f);
            enemies.Add(enemy);
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
            Vector2 center = player ? GetNearestWaterCenter(player.transform.position) : waterCenter;
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
            dolphin.AddComponent<UdderDolphinSurface>();
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
            int count = 4 + wave * 2;
            enemiesLeftInWave = count;
            nextWaveTimer = 0f;
            DisplayText("WAVE " + wave, levelText, player.transform.position + Vector3.up * 1.2f);

            for (int i = 0; i < count; i++)
                SpawnEnemy();
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
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var collider = bossObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.34f;

            var nameObject = new GameObject("Boss Name");
            nameObject.transform.SetParent(bossObject.transform, false);
            nameObject.transform.localPosition = new Vector3(0f, 0.58f, 0f);
            var label = nameObject.AddComponent<TextMeshPro>();
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
                    enemies.RemoveAt(i);
            }
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
                PickupType.Cheese => cheeseSprite ? cheeseSprite : bottleSprite,
                PickupType.Buttermilk => bottleSprite,
                PickupType.SpoiledMilk => skullBottleSprite ? skullBottleSprite : bottleSprite,
                PickupType.RawMilk => creamSprite ? creamSprite : bottleSprite,
                PickupType.Heal => creamSprite ? creamSprite : cheeseSprite,
                _ => creamSprite ? creamSprite : cheeseSprite,
            };
            renderer.color = type switch
            {
                PickupType.Buttermilk => new Color(0.95f, 1f, 0.35f),
                PickupType.SpoiledMilk => new Color(0.45f, 1f, 0.45f),
                PickupType.RawMilk => new Color(0.95f, 0.96f, 0.82f),
                PickupType.Cheese => new Color(1f, 0.86f, 0.25f),
                PickupType.Heal => new Color(1f, 0.75f, 0.85f),
                _ => Color.white,
            };
            renderer.sortingOrder = 6;

            var collider = pickup.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;

            var pickupComponent = pickup.AddComponent<UdderPickup>();
            pickupComponent.type = type;
        }

        private void DisplayText(string text, TextAnimation animation, Vector3 worldPosition)
        {
            if (!animation || !worldCamera || PixelBattleTextController.singleton == null)
                return;

            Vector3 viewport = worldCamera.WorldToViewportPoint(worldPosition);
            PixelBattleTextController.DisplayText(text, animation, viewport);
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
