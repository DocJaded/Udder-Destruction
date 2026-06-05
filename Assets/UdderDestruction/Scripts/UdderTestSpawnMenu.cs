using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UdderDestruction
{
    public sealed class UdderTestSpawnMenu : MonoBehaviour
    {
        public UdderGameController game;
        public TMP_Dropdown dropdown;
        public Button spawnButton;
        public TMP_Text statusText;

        private readonly List<UdderTestSpawnUnit> units = new();

        private void Start()
        {
            if (!game)
                game = FindFirstObjectByType<UdderGameController>();

            PopulateDropdown();

            if (spawnButton)
            {
                spawnButton.onClick.RemoveListener(SpawnSelected);
                spawnButton.onClick.AddListener(SpawnSelected);
            }

            UpdateStatus("Select a unit, then spawn it near Moolissa.");
        }

        private void PopulateDropdown()
        {
            units.Clear();
            units.AddRange((UdderTestSpawnUnit[])Enum.GetValues(typeof(UdderTestSpawnUnit)));

            if (!dropdown)
                return;

            dropdown.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>(units.Count);
            foreach (UdderTestSpawnUnit unit in units)
                options.Add(new TMP_Dropdown.OptionData(GetLabel(unit)));

            dropdown.AddOptions(options);
            dropdown.value = 0;
            dropdown.RefreshShownValue();
        }

        private void SpawnSelected()
        {
            if (!game || !dropdown || units.Count == 0)
            {
                UpdateStatus("Spawn menu is missing its game reference.");
                return;
            }

            int index = Mathf.Clamp(dropdown.value, 0, units.Count - 1);
            UdderTestSpawnUnit unit = units[index];
            game.SpawnTestUnit(unit);
            UpdateStatus("Spawned " + GetLabel(unit) + ".");
        }

        private void UpdateStatus(string text)
        {
            if (statusText)
                statusText.text = text;
        }

        private static string GetLabel(UdderTestSpawnUnit unit)
        {
            return unit switch
            {
                UdderTestSpawnUnit.DebtChicken => "Enemy: Debt Chicken",
                UdderTestSpawnUnit.HostileHam => "Enemy: Hostile Ham",
                UdderTestSpawnUnit.EnemyCow => "Enemy: Cow",
                UdderTestSpawnUnit.BeeDrone => "Enemy: Bee Drone",
                UdderTestSpawnUnit.PondDolphin => "Enemy: Pond Dolphin",
                UdderTestSpawnUnit.HostileSeaUrchin => "Enemy: Hostile Sea Urchin",
                UdderTestSpawnUnit.MiyamotoMoosashi => "Boss: Miyamoto Moosashi",
                UdderTestSpawnUnit.Lidia => "Boss: Lidia",
                UdderTestSpawnUnit.BobMoorley => "Boss: Bob Moorley",
                UdderTestSpawnUnit.HughHoofner => "Boss: Hugh Hoofner",
                UdderTestSpawnUnit.HolyCow => "Boss: The Holy Cow",
                UdderTestSpawnUnit.Ruminator => "Boss: The Ruminator",
                UdderTestSpawnUnit.Beeatrice => "Boss: BEEatrice",
                _ => unit.ToString(),
            };
        }
    }
}
