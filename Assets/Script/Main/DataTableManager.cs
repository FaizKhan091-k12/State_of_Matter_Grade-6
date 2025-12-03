using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataTableManager : MonoBehaviour
{
    [Header("Table Setup")]
    public RectTransform contentParent;
    public GameObject dataRowPrefab;

    [Header("Row Colors")]
    public Color cubeColor = new Color(0.3f, 0.8f, 0.4f);        // green
    public Color sphereColor = new Color(0.2f, 0.5f, 1f);        // blue
    public Color streamlinedColor = new Color(1f, 0.6f, 0.2f);   // orange

    private List<GameObject> _rows = new List<GameObject>();

    /// <summary>
    /// Adds a new row to the table.
    /// </summary>
    public void AddRow(string shape, string air, string parachute, float timeSeconds)
    {
        if (dataRowPrefab == null || contentParent == null)
        {
            Debug.LogWarning("[DataTableManager] Missing references!");
            return;
        }

        // Instantiate row
        GameObject rowGO = Instantiate(dataRowPrefab, contentParent);
        rowGO.transform.SetAsLastSibling();

        // Choose color based on shape
        Color iconColor = ColorForShape(shape);

        // Fill row
        DataRow dr = rowGO.GetComponent<DataRow>();
        if (dr != null)
        {
            dr.SetRow(
                shape,
                air,
                parachute,
                $"{timeSeconds:F2}s",
                iconColor);
        }

        _rows.Add(rowGO);
    }

    /// <summary>
    /// Returns inspector-assigned color based on shape text.
    /// </summary>
    private Color ColorForShape(string shape)
    {
        switch (shape)
        {
            case "Cube": return cubeColor;
            case "Sphere": return sphereColor;
            case "Streamlined": return streamlinedColor;
        }
        return Color.white;
    }
}
