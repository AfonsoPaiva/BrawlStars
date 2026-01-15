using System;
using Assets.Scripts.Interfaces;

namespace Assets.Scripts.Models
{
    public class HUDModel
    {
        private static readonly Lazy<HUDModel> _instance = new Lazy<HUDModel>(() => new HUDModel());
        public static HUDModel Instance => _instance.Value;

        private HUDModel() { }

        public event EventHandler<HUDSlotChangedEventArgs> SlotChanged;

        private IHUD _slot1;
        public IHUD Slot1
        {
            get => _slot1;
            set => SetSlot(ref _slot1, value, 1);
        }

        private IHUD _slot2;
        public IHUD Slot2
        {
            get => _slot2;
            set => SetSlot(ref _slot2, value, 2);
        }

        private IHUD _slot3;
        public IHUD Slot3
        {
            get => _slot3;
            set => SetSlot(ref _slot3, value, 3);
        }

        private void SetSlot(ref IHUD backingField,IHUD value, int slotIndex)
        {
            if (backingField == value) return;
            backingField = value;
            SlotChanged?.Invoke(this, new HUDSlotChangedEventArgs(slotIndex, value));
        }
        public int AssignNext(IHUD thing)
        {
            if (Slot1 == null)
            {
                Slot1 = thing;
                return 1;
            }
            if (Slot2 == null)
            {
                Slot2 = thing;
                return 2;
            }
            if (Slot3 == null)
            {
                Slot3 = thing;
                return 3;
            }
            return 0;
        }

        // Allows explicit clearing
        public void ClearSlot(int slotIndex)
        {
            if (slotIndex == 1) Slot1 = null;
            else if (slotIndex == 2) Slot2 = null;
            else if (slotIndex == 3) Slot3 = null;
        }
    }

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
}
