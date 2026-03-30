namespace Core.Interfaces;

public interface IInventoryManager
{
    void AddMaterial(string p_type, int p_amount);
    void RemoveMaterial(string p_type, int p_amount);
    int GetMaterialCount(string p_type);
}
