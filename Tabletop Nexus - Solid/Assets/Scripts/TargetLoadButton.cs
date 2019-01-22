using UnityEngine;
using UnityEngine.UI;

public class TargetLoadButton : MonoBehaviour {

    string myString;
    [SerializeField]
    Text buttonText;

    [SerializeField]
    UDTEventHandler UDTEH;

    public void SetupButton(string str)
    {
        myString = str;
        //Debug.Log(str);
        buttonText.text = myString;
    }

    public void OnClick()
    {
        UDTEH.SelectTarget(myString);
    }
}
