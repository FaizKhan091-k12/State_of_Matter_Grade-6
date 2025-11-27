using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ToggleController : MonoBehaviour
{
  public bool isThisAirToggle; 
  [SerializeField] Toggle toggle;



  [SerializeField] TextMeshProUGUI toggleText;
  [SerializeField] private Color offColor, onColor;


  private void Start()
  {
    toggle.onValueChanged.AddListener(delegate { SliderController(); });
    SliderController();
  }


  public void SliderController()
  {
    if (isThisAirToggle)
    {
      if (toggle.isOn)
      {
      
        toggle.transform.localScale = new Vector3(1, 1, 1);
        toggle.transform.GetChild(0).GetComponent<Image>().color = onColor;
        toggleText.text = "ON";
      
      }
      else
      {
     
        toggle.transform.localScale = new Vector3(-1, 1, 1);
        toggle.transform.GetChild(0).GetComponent<Image>().color = offColor;
        toggleText.text = "OFF";
    
       

      }
    }
    else
    {
      
    }
    
  }
  
}
