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
        public float tileScale = 3.2f;
        public int tileRadiusX = 18;
        public int tileRadiusY = 12;
        public int chunkRadius = 2;

        private readonly Dictionary<Vector2Int, GameObject> grassTiles = new();
        private readonly Dictionary<Vector2Int, List<GameObject>> chunkProps = new();
        private readonly List<Vector2> waterCenters = new();
        private readonly List<Vector2> waterRadii = new();
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
            if (!target)
                return;

            Vector2Int center = new(Mathf.RoundToInt(target.position.x), Mathf.RoundToInt(target.position.y));
            EnsureGrass(center);
            EnsureProps(center);
            game?.SetWaterBodies(waterCenters, waterRadii);
        }

        private void EnsureGrass(Vector2Int center)
        {
            HashSet<Vector2Int> wanted = new();
            for (int x = center.x - tileRadiusX; x <= center.x + tileRadiusX; x++)
            {
                for (int y = center.y - tileRadiusY; y <= center.y + tileRadiusY; y++)
                {
                    Vector2Int key = new(x, y);
                    wanted.Add(key);
                    if (grassTiles.ContainsKey(key))
                        continue;

                    GameObject tile = new($"Grass {x},{y}");
                    tile.transform.position = new Vector3(x, y, 0.5f);
                    tile.transform.localScale = Vector3.one * tileScale;
                    var renderer = tile.AddComponent<SpriteRenderer>();
                    renderer.sprite = grassSprite;
                    renderer.color = new Color(0.5f, 0.76f, 0.32f);
                    renderer.sortingOrder = -5;
                    grassTiles.Add(key, tile);
                }
            }

            RemoveOutside(grassTiles, wanted);
        }

        private void EnsureProps(Vector2Int center)
        {
            waterCenters.Clear();
            waterRadii.Clear();

            Vector2Int chunkCenter = WorldToChunk(center);
            HashSet<Vector2Int> wantedChunks = new();
            for (int x = chunkCenter.x - chunkRadius; x <= chunkCenter.x + chunkRadius; x++)
            {
                for (int y = chunkCenter.y - chunkRadius; y <= chunkCenter.y + chunkRadius; y++)
                {
                    Vector2Int chunk = new(x, y);
                    wantedChunks.Add(chunk);
                    if (!chunkProps.ContainsKey(chunk))
                        chunkProps.Add(chunk, BuildChunk(chunk));

                    foreach (GameObject prop in chunkProps[chunk])
                    {
                        if (!prop)
                            continue;

                        if (prop.name.Contains("Pond"))
                        {
                            waterCenters.Add(prop.transform.position);
                            waterRadii.Add(Vector2.one * 1.35f);
                        }
                    }
                }
            }

            List<Vector2Int> stale = new();
            foreach (Vector2Int key in chunkProps.Keys)
            {
                if (!wantedChunks.Contains(key))
                    stale.Add(key);
            }

            foreach (Vector2Int key in stale)
            {
                foreach (GameObject prop in chunkProps[key])
                {
                    if (prop)
                        Destroy(prop);
                }
                chunkProps.Remove(key);
            }
        }

        private List<GameObject> BuildChunk(Vector2Int chunk)
        {
            List<GameObject> props = new();
            int hash = Hash(chunk.x, chunk.y);
            Vector2 chunkOrigin = new(chunk.x * 10f, chunk.y * 10f);

            if ((hash % 100) < 24 || chunk == new Vector2Int(-1, 0))
                props.AddRange(CreateWaterPatch(chunkOrigin + new Vector2(2f, -2f)));

            if (((hash / 100) % 100) < 14)
                props.Add(CreateProp("Procedural Barn", barnSprite, chunkOrigin + new Vector2(-2.5f, 2.5f), Color.white, 1.8f, -2));

            return props;
        }

        private List<GameObject> CreateWaterPatch(Vector2 center)
        {
            List<GameObject> tiles = new();
            if (!waterSprite)
                return tiles;

            Vector2 spacing = waterSprite.bounds.size * 4f;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    Vector2 pos = center + new Vector2(x * spacing.x * 0.5f, y * spacing.y * 0.5f);
                    tiles.Add(CreateProp("Procedural Pond Tile", waterSprite, pos, new Color(0.7f, 0.95f, 1f), 4f, -2));
                }
            }
            return tiles;
        }

        private GameObject CreateProp(string name, Sprite sprite, Vector2 position, Color color, float scale, int sortingOrder)
        {
            if (!sprite)
                return null;

            GameObject prop = new(name);
            prop.transform.position = new Vector3(position.x, position.y, 0f);
            prop.transform.localScale = Vector3.one * scale;
            var renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return prop;
        }

        private static Vector2Int WorldToChunk(Vector2Int position)
        {
            return new Vector2Int(Mathf.FloorToInt(position.x / 10f), Mathf.FloorToInt(position.y / 10f));
        }

        private static int Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 73856093 ^ y * 19349663;
                h ^= h >> 13;
                h *= 83492791;
                return Mathf.Abs(h);
            }
        }

        private static void RemoveOutside(Dictionary<Vector2Int, GameObject> objects, HashSet<Vector2Int> wanted)
        {
            List<Vector2Int> stale = new();
            foreach (Vector2Int key in objects.Keys)
            {
                if (!wanted.Contains(key))
                    stale.Add(key);
            }

            foreach (Vector2Int key in stale)
            {
                if (objects[key])
                    Destroy(objects[key]);
                objects.Remove(key);
            }
        }
    }
}
