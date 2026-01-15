using System;
using System.Collections.Generic;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
    public class HUDModel
    {
        private static readonly Lazy<HUDModel> _instance = new Lazy<HUDModel>(() => new HUDModel());
        public static HUDModel Instance => _instance.Value;

        private HUDModel() { }

        // Dynamic list of IHUD elements
        private readonly List<IHUD> _slots = new List<IHUD>();

        public IReadOnlyList<IHUD> Slots => _slots;
        public int SlotCount => _slots.Count;

        public event EventHandler<HUDSlotChangedEventArgs> SlotChanged;
        public event EventHandler<HUDSlotAddedEventArgs> SlotAdded;
        public event EventHandler<HUDSlotRemovedEventArgs> SlotRemoved;

        // Legacy properties for backward compatibility
        public IHUD Slot1 => _slots.Count > 0 ? _slots[0] : null;
        public IHUD Slot2 => _slots.Count > 1 ? _slots[1] : null;
        public IHUD Slot3 => _slots.Count > 2 ? _slots[2] : null;

        /// <summary>
        /// Adds a new IHUD element and returns its slot index (1-based).
        /// </summary>
        public int AssignNext(IHUD thing)
        {
            if (thing == null || _slots.Contains(thing)) return 0;

            _slots.Add(thing);
            int slotIndex = _slots.Count; // 1-based index

            SlotAdded?.Invoke(this, new HUDSlotAddedEventArgs(slotIndex, thing));
            SlotChanged?.Invoke(this, new HUDSlotChangedEventArgs(slotIndex, thing));

            return slotIndex;
        }

        /// <summary>
        /// Removes an IHUD element by reference.
        /// </summary>
        public void Remove(IHUD thing)
        {
            int idx = _slots.IndexOf(thing);
            if (idx >= 0)
            {
                _slots.RemoveAt(idx);
                SlotRemoved?.Invoke(this, new HUDSlotRemovedEventArgs(idx + 1, thing));
            }
        }

        /// <summary>
        /// Clears a slot by 1-based index.
        /// </summary>
        public void ClearSlot(int slotIndex)
        {
            int idx = slotIndex - 1;
            if (idx >= 0 && idx < _slots.Count)
            {
                var thing = _slots[idx];
                _slots.RemoveAt(idx);
                SlotRemoved?.Invoke(this, new HUDSlotRemovedEventArgs(slotIndex, thing));
            }
        }

        /// <summary>
        /// Gets IHUD at 1-based slot index.
        /// </summary>
        public IHUD GetSlot(int slotIndex)
        {
            int idx = slotIndex - 1;
            return (idx >= 0 && idx < _slots.Count) ? _slots[idx] : null;
        }

        public void ClearAll()
        {
            while (_slots.Count > 0)
            {
                ClearSlot(_slots.Count);
            }
        }
    }

    #region Event Args
    public class HUDSlotChangedEventArgs : EventArgs
    {
        public int SlotIndex { get; }
        public IHUD AssignedThing { get; }

        public HUDSlotChangedEventArgs(int slotIndex, IHUD assignedThing)
        {
            SlotIndex = slotIndex;
            AssignedThing = assignedThing;
        }
    }

    public class HUDSlotAddedEventArgs : EventArgs
    {
        public int SlotIndex { get; }
        public IHUD AssignedThing { get; }

        public HUDSlotAddedEventArgs(int slotIndex, IHUD assignedThing)
        {
            SlotIndex = slotIndex;
            AssignedThing = assignedThing;
        }
    }

    public class HUDSlotRemovedEventArgs : EventArgs
    {
        public int SlotIndex { get; }
        public IHUD RemovedThing { get; }

        public HUDSlotRemovedEventArgs(int slotIndex, IHUD removedThing)
        {
            SlotIndex = slotIndex;
            RemovedThing = removedThing;
        }
    }
    #endregion
}
