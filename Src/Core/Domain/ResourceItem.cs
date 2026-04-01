namespace Core.Domain;

public class ResourceItem
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string IconPath { get; set; }

    public ResourceItem(string p_id, string p_name, string p_type, string p_iconPath)
    {
        Id = p_id;
        Name = p_name;
        Type = p_type;
        IconPath = p_iconPath;
    }
}
