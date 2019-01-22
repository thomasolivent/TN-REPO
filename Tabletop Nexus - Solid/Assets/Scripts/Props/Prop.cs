using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour {

    public int identifier;
    public bool placed;
    public bool selected;
    PropPlacement pPlace;

    private void Start()
    {
        pPlace = FindObjectOfType<PropPlacement>();
    }

    // Update is called once per frame
    void Update () {
        if (!placed && selected)
        {
            GetComponent<Collider>().enabled = false;
            Ray ray = pPlace.dungeonCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                transform.position = hit.point;
            }
        }else
        {
            GetComponent<Collider>().enabled = true;
        }
    }
}