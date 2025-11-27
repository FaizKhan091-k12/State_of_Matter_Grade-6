using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataRow : MonoBehaviour
{
    public TextMeshProUGUI txtShape;
    public TextMeshProUGUI txtAir;
    public TextMeshProUGUI txtParachute;
    public TextMeshProUGUI txtTime;

    /// <summary>
    /// Fill the UI fields. TimeStr should include the unit or formatted value (e.g. "0.64s")
    /// </summary>
    public void SetRow(string shape, string air, string parachute, string timeStr)
    {
        if (txtShape != null) txtShape.text = shape;
        if (txtAir != null) txtAir.text = air;
        if (txtParachute != null) txtParachute.text = parachute;
        if (txtTime != null) txtTime.text = timeStr;
    }
}
