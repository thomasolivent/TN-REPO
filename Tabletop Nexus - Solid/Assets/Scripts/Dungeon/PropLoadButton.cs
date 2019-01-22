using UnityEngine;
using UnityEngine.UI;

public class PropLoadButton : MonoBehaviour
{

    string myString;
    GameObject prop;
    GameObject newProp;

    [SerializeField]
    Text buttonText;
    
    [SerializeField]
    PropPlacement pPlace;

    public void SetupButton(string str, GameObject go)
    {
        myString = str;
        prop = go;
        buttonText.text = myString;
    }

    public void OnClick()
    {
        if (pPlace.selectedProp == null || pPlace.selectedProp.GetComponent<Prop>().placed)
        {
            newProp = Instantiate(prop);
            pPlace.selectedProp = newProp;
            newProp.GetComponent<Prop>().placed = false;
            newProp.GetComponent<Prop>().selected = true;
        }else if (pPlace.selectedProp == newProp)
        {
            Destroy(newProp);
            pPlace.selectedProp = null;
        }

        //if(pPlace.selectedProp != null && pPlace.selectedProp.GetComponent<Prop>().placed == false && pPlace.selectedProp != prop)
        //{
        //    Destroy()
        //}
        //if a prop is selected but not placed and not the same prop swap that prop for this one
    }
}

//to do: sync props?