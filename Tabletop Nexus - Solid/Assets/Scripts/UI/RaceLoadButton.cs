using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaceLoadButton : MonoBehaviour {

    string myString;
    [SerializeField]
    Text buttonText;

    [SerializeField]
    CharacterCreator charCreator;

    public void SetupButton(string str)
    {
        myString = str;
        Debug.Log(str);
        buttonText.text = myString;
    }

    public void OnClick()
    {
        charCreator.SwitchRace(myString);
    }
}
