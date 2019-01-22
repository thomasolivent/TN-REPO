using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterLoadButton : MonoBehaviour
{

    string myString;
    [SerializeField]
    Text buttonText;

    [SerializeField]
    CharacterCreator cSave;

    public void SetupButton(string str)
    {
        myString = str;
        Debug.Log(str);
        buttonText.text = myString;
    }

    public void OnClick()
    {
        cSave.SelectCharacter(myString);
    }
}