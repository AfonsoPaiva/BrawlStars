using UnityEngine;
using TMPro;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Models;
using System;

namespace Assets.Scripts.Presenters
{
    public class HUDPresenter : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] slotTexts;
        private IHUD[] _models = new IHUD[3];

        // per-slot cached event handlers (initialized in Awake)
        private EventHandler[] OnHealthChanged;
        private EventHandler[] OnPAChanged;

        private void Awake()
        {
            // create per-slot delegates so unsubscribe works reliably
            OnHealthChanged = new EventHandler[3];
            OnPAChanged = new EventHandler[3];
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                OnHealthChanged[i] = (s, e) => UpdateSlot(idx);
                OnPAChanged[i] = (s, e) => UpdateSlot(idx);
            }

            // Subscribe to HUDModel changes
            var hud = HUDModel.Instance;
            if (hud != null)
            {
                hud.SlotChanged += OnHUDSlotChanged;

                // Sync current state in case slots were assigned earlier
                SetSlot(1, hud.Slot1);
                SetSlot(2, hud.Slot2);
                SetSlot(3, hud.Slot3);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from HUDModel
            var hud = HUDModel.Instance;
            if (hud != null)
            {
                hud.SlotChanged -= OnHUDSlotChanged;
            }

            // Unsubscribe any model events
            for (int i = 0; i < _models.Length; i++)
            {
                if (_models[i] != null && OnHealthChanged != null)
                {
                    _models[i].HealthChanged -= OnHealthChanged[i];
                }
                if (_models[i] != null && OnPAChanged != null)
                {
                    _models[i].PAProgressChanged -= OnPAChanged[i];
                }
            }
        }

        // Called by HUDModel when a slot changes
        private void OnHUDSlotChanged(object sender, HUDSlotChangedEventArgs e)
        {
            SetSlot(e.SlotIndex, e.AssignedThing);
        }

        public void SetSlot(int slot, IHUD model)
        {
            int idx = slot - 1;
            if (idx < 0 || idx >= slotTexts.Length) return;

            // Unsubscribe previous model safely
            if (_models[idx] != null && OnHealthChanged != null)
            {
                _models[idx].HealthChanged -= OnHealthChanged[idx];
                _models[idx].PAProgressChanged -= OnPAChanged[idx];
            }

            _models[idx] = model;

            if (model != null)
            {
                // Subscribe the cached delegates
                if (OnHealthChanged != null)
                {
                    model.HealthChanged += OnHealthChanged[idx];
                    model.PAProgressChanged += OnPAChanged[idx];
                }

                // Immediately update UI for this slot
                UpdateSlot(idx);
            }
            else
            {
                // Clear UI when slot is empty
                slotTexts[idx].text = "Empty";
            }
        }

        private void UpdateSlot(int idx)
        {
            var model = _models[idx];
            if (model != null && slotTexts != null && idx >= 0 && idx < slotTexts.Length)
            {
                slotTexts[idx].text = $"HP: {model.Health:0} \n PA: {model.PAProgress:P0}";
            }
            else if (slotTexts != null && idx >= 0 && idx < slotTexts.Length)
            {
                slotTexts[idx].text = "Empty";
            }
        }
    }
}