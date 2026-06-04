using System.Collections.Generic;
using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderWorldStreamer : MonoBehaviour
    {
        public Transform target;
        public UdderGameController game;
        public Sprite grassSprite;
        public Sprite waterSprite;
        public Sprite barnSprite;
        public GameObject grassTilePrefab;
        public GameObject pondTilePrefab;
        public GameObject barnPrefab;
        public float tileScale = 3.2f;
        public int tileRadiusX = 18;
        public int tileRadiusY = 12;
        public int chunkRadius = 2;
        public int arenaSize = 100;
        public int pondBorderThickness = 24;

        private readonly Dictionary<Vector2Int, GameObject> grassTiles = new();
        private readonly Dictionary<Vector2Int, GameObject> pondTiles = new();
        private readonly List<Vector2> waterCenters = new();
        private readonly List<Vector2> waterRadii = new();
        private bool arenaBuilt;
        private float refreshTimer;

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer > 0f)
                return;

            refreshTimer = 0.35f;
            Refresh();
        }

        private void Refresh()
        {
            EnsureArena();
            RefreshWaterRegistrationAndColors();
        }

        private void EnsureArena()
        {
            if (arenaBuilt)
                return;

            int half = arenaSize / 2;
            int border = Mathf.Max(1, pondBorderThickness);
            for (int x = -half; x < half; x++)
            {
                for (int y = -half; y < half; y++)
                    CreateGrassTile(new Vector2Int(x, y));
            }

            for (int x = -half - border; x < half + border; x++)
            {
                for (int y = -half - border; y < -half; y++)
                    CreatePondTile(new Vector2Int(x, y));
                for (int y = half; y < half + border; y++)
                    CreatePondTile(new Vector2Int(x, y));
            }

            for (int y = -half; y < half; y++)
            {
                for (int x = -half - border; x < -half; x++)
                    CreatePondTile(new Vector2Int(x, y));
                for (int x = half; x < half + border; x++)
                    CreatePondTile(new Vector2Int(x, y));
            }

            arenaBuilt = true;
        }

        private void CreateGrassTile(Vector2Int key)
        {
            if (grassTiles.ContainsKey(key))
                return;

            GameObject tile = grassTilePrefab ? Instantiate(grassTilePrefab) : new GameObject($"Grass {key.x},{key.y}");
            tile.name = $"Grass {key.x},{key.y}";
            tile.transform.position = new Vector3(key.x, key.y, 0.5f);
            if (!grassTilePrefab)
                tile.transform.localScale = Vector3.one * tileScale;
            var renderer = EnsureComponent<SpriteRenderer>(tile);
            renderer.sprite = grassSprite;
            renderer.color = new Color(0.5f, 0.76f, 0.32f);
            renderer.sortingOrder = -5;
            grassTiles.Add(key, tile);
        }

        private void CreatePondTile(Vector2Int key)
        {
            if (pondTiles.ContainsKey(key) || !waterSprite)
                return;

            GameObject tile = pondTilePrefab ? Instantiate(pondTilePrefab) : new GameObject($"Pond {key.x},{key.y}");
            tile.name = $"Pond {key.x},{key.y}";
            tile.transform.position = new Vector3(key.x, key.y, 0f);
            if (!pondTilePrefab)
                tile.transform.localScale = Vector3.one * 4f;
            var renderer = EnsureComponent<SpriteRenderer>(tile);
            renderer.sprite = waterSprite;
            renderer.color = new Color(0.7f, 0.95f, 1f);
            renderer.sortingOrder = -2;
            AddWaterCollider(tile, waterSprite);
            pondTiles.Add(key, tile);
        }

        private void RefreshWaterRegistrationAndColors()
        {
            waterCenters.Clear();
            waterRadii.Clear();

            foreach (GameObject tile in pondTiles.Values)
            {
                if (!tile)
                    continue;

                if (tile.TryGetComponent(out BoxCollider2D waterCollider))
                {
                    waterCenters.Add(waterCollider.bounds.center);
                    waterRadii.Add(waterCollider.bounds.extents);
                }
                else
                {
                    waterCenters.Add(tile.transform.position);
                    waterRadii.Add(Vector2.one * 0.5f);
                }

                if (tile.TryGetComponent(out SpriteRenderer renderer))
                {
                    renderer.color = game && game.IsWaterContaminated(tile.transform.position)
                        ? new Color(0.25f, 0.95f, 0.25f)
                        : new Color(0.7f, 0.95f, 1f);
                }
            }

            game?.SetWaterBodies(waterCenters, waterRadii);
        }

        private static void AddWaterCollider(GameObject tile, Sprite sprite)
        {
            if (!tile || !sprite)
                return;

            if (tile.TryGetComponent(out BoxCollider2D existingCollider))
            {
                existingCollider.isTrigger = true;
                return;
            }

            var collider = tile.AddComponent<BoxCollider2D>();
            collider.size = sprite.bounds.size;
            collider.offset = sprite.bounds.center;
            collider.isTrigger = true;
        }

        private static T EnsureComponent<T>(GameObject target) where T : Component
        {
            if (target.TryGetComponent(out T component))
                return component;

            return target.AddComponent<T>();
        }
    }
}
