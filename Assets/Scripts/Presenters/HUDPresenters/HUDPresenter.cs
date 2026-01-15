using UnityEngine;
using TMPro;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Presenters
{
    public class HUDPresenter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private GameObject slotPrefab;

        // Dynamic list of slot data
        private readonly List<SlotData> _slotDataList = new List<SlotData>();

        private class SlotData
        {
            public IHUD Model;
            public TMP_Text Text;
            public GameObject GameObject;
            public EventHandler OnHealthChanged;
            public EventHandler OnPAChanged;
        }

        private void Awake()
        {
            var hud = HUDModel.Instance;
            if (hud != null)
            {
                hud.SlotAdded += OnSlotAdded;
                hud.SlotRemoved += OnSlotRemoved;

                // Sync any existing slots
                for (int i = 0; i < hud.SlotCount; i++)
                {
                    CreateSlotUI(i + 1, hud.Slots[i]);
                }
            }
        }

        private void OnDestroy()
        {
            var hud = HUDModel.Instance;
            if (hud != null)
            {
                hud.SlotAdded -= OnSlotAdded;
                hud.SlotRemoved -= OnSlotRemoved;
            }

            // Cleanup all slots
            foreach (var slotData in _slotDataList)
            {
                UnsubscribeFromModel(slotData);
                if (slotData.GameObject != null)
                {
                    Destroy(slotData.GameObject);
                }
            }
            _slotDataList.Clear();
        }

        private void OnSlotAdded(object sender, HUDSlotAddedEventArgs e)
        {
            CreateSlotUI(e.SlotIndex, e.AssignedThing);
        }

        private void OnSlotRemoved(object sender, HUDSlotRemovedEventArgs e)
        {
            RemoveSlotUI(e.RemovedThing);
        }

        private void CreateSlotUI(int slotIndex, IHUD model)
        {
            if (slotPrefab == null)
            {
                Debug.LogError("HUDPresenter: slotPrefab is not assigned!");
                return;
            }
            if (slotContainer == null)
            {
                Debug.LogError("HUDPresenter: slotContainer is not assigned!");
                return;
            }
            if (model == null)
            {
                Debug.LogError("HUDPresenter: model is null!");
                return;
            }

            // Instantiate UI element
            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            TMP_Text text = slotObj.GetComponentInChildren<TMP_Text>();

            // Create slot data with cached event handlers
            var slotData = new SlotData
            {
                Model = model,
                Text = text,
                GameObject = slotObj
            };

            // Create and cache event handlers for this specific slot
            slotData.OnHealthChanged = (s, e) => UpdateSlotDisplay(slotData);
            slotData.OnPAChanged = (s, e) => UpdateSlotDisplay(slotData);

            // Subscribe to model events
            model.HealthChanged += slotData.OnHealthChanged;
            model.PAProgressChanged += slotData.OnPAChanged;

            _slotDataList.Add(slotData);

            // Initial display update
            UpdateSlotDisplay(slotData);

            Debug.Log($"HUDPresenter: Created slot {slotIndex} for {model.GetType().Name}");
        }

        private void RemoveSlotUI(IHUD model)
        {
            var slotData = _slotDataList.Find(s => s.Model == model);
            if (slotData != null)
            {
                UnsubscribeFromModel(slotData);

                if (slotData.GameObject != null)
                {
                    Destroy(slotData.GameObject);
                }

                _slotDataList.Remove(slotData);
                Debug.Log($"HUDPresenter: Removed slot for {model.GetType().Name}");
            }
        }

        private void UnsubscribeFromModel(SlotData slotData)
        {
            if (slotData.Model != null)
            {
                if (slotData.OnHealthChanged != null)
                    slotData.Model.HealthChanged -= slotData.OnHealthChanged;
                if (slotData.OnPAChanged != null)
                    slotData.Model.PAProgressChanged -= slotData.OnPAChanged;
            }
        }

        private void UpdateSlotDisplay(SlotData slotData)
        {
            if (slotData.Text == null || slotData.Model == null) return;

            slotData.Text.text = $"HP: {slotData.Model.Health:F0} | PA: {slotData.Model.PAProgress * 100:F0}%";
        }
    }
}