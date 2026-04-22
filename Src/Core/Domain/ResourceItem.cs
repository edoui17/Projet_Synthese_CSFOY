namespace Core.Domain;

public class ResourceItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? IconPath { get; set; }

    public virtual ICollection<InventoryEntry> InventoryEntries { get; set; } = new List<InventoryEntry>();

    public ResourceItem() { }

    public ResourceItem(string p_id, string p_name, string p_type, string? p_iconPath)
    {
        Id = p_id;
        Name = p_name;
        Type = p_type;
        IconPath = p_iconPath;
    }
}
