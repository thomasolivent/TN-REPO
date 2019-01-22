using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropPlacement : MonoBehaviour {

    [SerializeField]
    public List<GameObject> propList = new List<GameObject>();
    List<GameObject> loadPropButtons = new List<GameObject>();

    [SerializeField]
    public Camera dungeonCam;

    [SerializeField]
    GameObject propButtonPrefab;
    [SerializeField]
    GameObject propViewList;
    [HideInInspector]
    public GameObject selectedProp;

    public void PropsListSwitch()
    {
        if (propViewList.activeSelf)
        {
            propViewList.SetActive(false);
        }else if (!propViewList.activeSelf)
        {
            propViewList.SetActive(true);
            PopulatePropUIList();
        }
    }

    public void PopulatePropUIList()
    {
        foreach(GameObject go in loadPropButtons)
        {
            Destroy(go.gameObject);
        }

        loadPropButtons.Clear();

        foreach(GameObject prop in propList)
        {
            GameObject newButton = Instantiate(propButtonPrefab);
            newButton.transform.SetParent(propButtonPrefab.transform.parent, false);
            newButton.name = prop.name + " Prop";

            newButton.GetComponent<PropLoadButton>().SetupButton(prop.name, prop);
            loadPropButtons.Add(newButton);
            newButton.SetActive(true);
        }
    }

    public void SetPropPosition()
    {
        if (selectedProp != null)
        {
            selectedProp.GetComponent<Prop>().placed = true;
            selectedProp.GetComponent<Prop>().selected = false;
            selectedProp = null;
        }
    }

    public void DeleteSelectedProp()
    {
        if (selectedProp.GetComponent<Prop>().placed)
        {
            Destroy(selectedProp);
            selectedProp = null;
        }
    }
}