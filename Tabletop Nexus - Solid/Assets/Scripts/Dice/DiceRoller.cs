using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceRoller : MonoBehaviour {

    [SerializeField]
    List<GameObject> diceGO;

    [SerializeField]
    GameObject diceButtonsPanel;

    [SerializeField]
    public Camera diceCam;

    [SerializeField]
    GameObject spawn;

    bool showHidePanel = false;

    GameObject cd;
    Rigidbody drb;

    public void ShowHideDiceButtons()
    {
        if (showHidePanel)
        {
            diceButtonsPanel.SetActive(false);
            showHidePanel = false;
        }else if(!showHidePanel)
        {
            diceButtonsPanel.SetActive(true);
            showHidePanel = true;
        }
    }

    public void RollDice(int dSize)
    {
        if (dSize != 10)
        {
            //Debug.Log(dSize);
            cd = Instantiate(diceGO[dSize], spawn.transform.position, Quaternion.identity);
            drb = cd.GetComponent<Rigidbody>();
            drb.AddTorque(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100));
            drb.AddForce(new Vector3(Random.Range(400, 800) * -1, 0, 0));
        }
    }
}