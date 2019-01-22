using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetBuilderSwitch : MonoBehaviour {

    [SerializeField]
    GameObject targetBuilderPanel;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnEnable()
    {
        targetBuilderPanel.SetActive(true);
    }
}
