namespace Core.Managers;

using System.Collections.Generic;
using Core.Interfaces;

public class InventoryManager : IInventoryManager
{
    private readonly Dictionary<string, int> m_materials = new Dictionary<string, int>();

    public void AddMaterial(string p_type, int p_amount)
    {
        if (string.IsNullOrEmpty(p_type) || p_amount <= 0)
        {
            return;
        }

        if (m_materials.ContainsKey(p_type))
        {
            m_materials[p_type] += p_amount;
        }
        else
        {
            m_materials[p_type] = p_amount;
        }
    }

    public void RemoveMaterial(string p_type, int p_amount)
    {
        if (string.IsNullOrEmpty(p_type) || p_amount <= 0 || !m_materials.ContainsKey(p_type))
        {
            return;
        }

        m_materials[p_type] -= p_amount;
        if (m_materials[p_type] < 0)
        {
            m_materials[p_type] = 0;
        }
    }

    public int GetMaterialCount(string p_type)
    {
        if (string.IsNullOrEmpty(p_type) || !m_materials.ContainsKey(p_type))
        {
            return 0;
        }

        return m_materials[p_type];
    }
}
