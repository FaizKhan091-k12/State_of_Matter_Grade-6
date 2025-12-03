using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataRow : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI txtShape;
    public TextMeshProUGUI txtAir;
    public TextMeshProUGUI txtParachute;
    public TextMeshProUGUI txtTime;

    [Header("Colored Icon")]
    public Image proceduralImage;

    /// <summary>
    /// Assigns texts + icon color.
    /// </summary>
    public void SetRow(string shape, string air, string parachute, string timeStr, Color iconColor)
    {
        if (txtShape != null) txtShape.text = shape;
        if (txtAir != null) txtAir.text = air;
        if (txtParachute != null) txtParachute.text = parachute;
        if (txtTime != null) txtTime.text = timeStr;

        if (proceduralImage != null)
        {
            proceduralImage.color = iconColor;   // ONLY IMAGE COLOR
        }
    }
}
