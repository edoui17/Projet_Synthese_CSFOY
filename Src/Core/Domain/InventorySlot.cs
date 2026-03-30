namespace Core.Domain;

public class InventorySlot
{
    public ResourceItem Item { get; set; }
    public int Quantity { get; set; }

    public InventorySlot(ResourceItem p_item, int p_quantity)
    {
        Item = p_item;
        Quantity = p_quantity;
    }
}
