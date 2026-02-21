using System;
using System.Collections.Generic;
using VikingJamGame.Models.GameEvents;

namespace VikingJamGame.Models.Items;

public class Inventory
{
    public const int MAX_SLOTS = 3;

    public event Action? InventoryChanged;

    public List<Item?> Slots { get; } = [null, null, null];

    public bool IsFull => Slots.TrueForAll(slot => slot is not null);

    public bool AddItem(Item item, GameEventContext context)
    {
        int freeSlot = Slots.IndexOf(null);
        if (freeSlot < 0)
        {
            return false;
        }

        Slots[freeSlot] = item;

        foreach (var effect in item.EffectsOnEquip)
        {
            effect.Apply(context);
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public void UseItem(int slot, GameEventContext context)
    {
        ValidateSlot(slot);
        var item = Slots[slot];
        if (item is null)
        {
            return;
        }

        foreach (var effect in item.EffectsOnUse)
        {
            effect.Apply(context);
        }

        if (item.IsConsumable)
        {
            item.RemainingCharges--;
            if (item.RemainingCharges <= 0)
            {
                Slots[slot] = null;
                InventoryChanged?.Invoke();
            }
        }

    }

    public bool RemoveItem(int slot, GameEventContext context)
    {
        ValidateSlot(slot);
        var item = Slots[slot];
        if (item is null)
        {
            return false;
        }

        if (item.IsCursed)
        {
            return false;
        }

        foreach (var effect in item.EffectsOnUnequip)
        {
            effect.Apply(context);
        }

        Slots[slot] = null;
        InventoryChanged?.Invoke();
        return true;
    }

    public bool HasItem(string itemId) =>
        Slots.Exists(item => item is not null && item.Id == itemId);

    private static void ValidateSlot(int slot)
    {
        if (slot < 0 || slot >= MAX_SLOTS)
        {
            throw new ArgumentOutOfRangeException(nameof(slot),
                $"Slot must be between 0 and {MAX_SLOTS - 1}.");
        }
    }
}