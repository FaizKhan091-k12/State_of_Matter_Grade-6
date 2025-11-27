using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataTableManager : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform contentParent;      // the ScrollRect content (vertical layout)
    public GameObject dataRowPrefab;         // the prefab with DataRow component
    public int maxRows = 100;                // optional cap, 0 = unlimited

    private List<GameObject> _rows = new List<GameObject>();

    /// <summary>
    /// Add a row to the table. timeSeconds will be formatted automatically to "0.00s"
    /// </summary>
    public void AddRow(string shape, string airResistance, string parachuteArea, float timeSeconds)
    {
        if (dataRowPrefab == null || contentParent == null)
        {
            Debug.LogWarning("[DataTableManager] Prefab or contentParent missing.");
            return;
        }

        // create
        GameObject go = Instantiate(dataRowPrefab, contentParent);
        go.transform.SetAsLastSibling();

        // set values
        DataRow dr = go.GetComponent<DataRow>();
        if (dr != null)
        {
            string timeStr = $"{timeSeconds:F2}s";
            dr.SetRow(shape, airResistance, parachuteArea, timeStr);
        }
        else
        {
            // fallback: try to find text children by name (optional)
            var texts = go.GetComponentsInChildren<Text>();
            if (texts.Length >= 4)
            {
                texts[0].text = shape;
                texts[1].text = airResistance;
                texts[2].text = parachuteArea;
                texts[3].text = $"{timeSeconds:F2}s";
            }
        }

        _rows.Add(go);

        // optional culling: keep table manageable
        if (maxRows > 0 && _rows.Count > maxRows)
        {
            Destroy(_rows[0]);
            _rows.RemoveAt(0);
        }

        Canvas.ForceUpdateCanvases(); // ensure layout updated (useful before scrolling)
        // Optional: scroll to bottom if you have a ScrollRect parent
        var sc = contentParent.GetComponentInParent<ScrollRect>();
        if (sc != null)
        {
            sc.verticalNormalizedPosition = 0f; // bottom
        }
    }

    /// <summary>
    /// Clear all rows
    /// </summary>
    public void Clear()
    {
        foreach (var r in _rows)
            if (r != null) Destroy(r);
        _rows.Clear();
    }
}
