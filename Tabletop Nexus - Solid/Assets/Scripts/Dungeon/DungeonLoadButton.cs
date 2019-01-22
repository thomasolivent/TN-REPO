using UnityEngine;
using UnityEngine.UI;

public class DungeonLoadButton : MonoBehaviour
{

    string myString;
    [SerializeField]
    Text buttonText;

    [SerializeField]
    DungeonSaveLoad dSave;

    public void SetupButton(string str)
    {
        myString = str;
        //Debug.Log(str);
        buttonText.text = myString;
    }

    public void OnClick()
    {
        dSave.SelectDungeon(myString);
    }
}