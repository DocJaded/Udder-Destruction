using System.Collections.Generic;
using UnityEngine;

namespace UdderDestruction
{
    public sealed class UdderBeeSwarmController : MonoBehaviour
    {
        private readonly List<UdderBeeDrone> drones = new();
        private UdderEnemy queen;
        private float attackTimer;
        private int nextAttacker;

        public void Init(UdderEnemy queenEnemy, IReadOnlyList<UdderBeeDrone> swarm)
        {
            queen = queenEnemy;
            drones.Clear();
            for (int i = 0; i < swarm.Count; i++)
            {
                if (swarm[i])
                    drones.Add(swarm[i]);
            }

            queen.IsInvulnerable = drones.Count > 0;
            AssignAttackers();
        }

        private void Update()
        {
            if (!queen || !queen.IsAlive)
                return;

            for (int i = drones.Count - 1; i >= 0; i--)
            {
                if (!drones[i] || !drones[i].IsAlive)
                    drones.RemoveAt(i);
            }

            queen.IsInvulnerable = drones.Count > 0;
            if (drones.Count == 0)
                return;

            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                AssignAttackers();
        }

        private void AssignAttackers()
        {
            attackTimer = 1f;
            for (int i = 0; i < drones.Count; i++)
                drones[i].SetAttacking(false);

            int attackerCount = Mathf.Min(3, drones.Count);
            for (int i = 0; i < attackerCount; i++)
            {
                int index = (nextAttacker + i) % drones.Count;
                drones[index].SetAttacking(true);
            }

            nextAttacker = (nextAttacker + attackerCount) % drones.Count;
        }
    }
}
