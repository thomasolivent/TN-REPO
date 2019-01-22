using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dice : MonoBehaviour {

    public TextMesh text;   //For Text Above Dice
    public SideResults selectedResult;
    public int selectedVector;
    // Note: the size of the vectorValues and vectorPoints should be the same.
    public SideResults[] vectorValues;
    public Vector3[] vectorPoints; // These vectors should be normalized. Might be worth adding a task to Start to ensure they are normalized.

    Camera diceCam;

    void Start()
    {
        diceCam = FindObjectOfType<DiceRoller>().diceCam;
    }

    // Update is called once per frame
    void Update()
    {
        float bestDot = -1;
        for (int i = 0; i < vectorPoints.Length; ++i)
        {
            var valueVector = vectorPoints[i];
            // Each side vector is in local object space. We need them in world space for our calculation.
            var worldSpaceValueVector = this.transform.localToWorldMatrix.MultiplyVector(valueVector);
            // Mathf.Arccos of the dot product can be used to get the angle of difference. You can use this to check for a tilt (perhaps requiring a reroll)
            float dot = Vector3.Dot(worldSpaceValueVector, Vector3.up);
            if (dot > bestDot)
            {
                // The vector with the greatest dot product is the vector in the most "up" direction. This is the current face selected.
                bestDot = dot;
                selectedVector = i;
            }
        }

        selectedResult = vectorValues[selectedVector];

        //For Text Above Dice
        text.text = selectedResult.ToString();
        text.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + 3f);
        text.transform.LookAt(text.transform.position + diceCam.transform.rotation * Vector3.forward, diceCam.transform.rotation * Vector3.up);


        //For dice despawn
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit hit;
            Ray ray = diceCam.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 1000f) && hit.collider.gameObject.tag == "Dice")
            {
                Destroy(hit.collider.gameObject);
            }
        }
    }

    //For Setting Vector Points
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var valueVector in vectorPoints)
        {
            var worldSpaceValueVector = this.transform.localToWorldMatrix.MultiplyVector(valueVector);
            Gizmos.DrawLine(this.transform.position, this.transform.position + worldSpaceValueVector);
        }
    }
}

// Enum for storing the potential results.
public enum SideResults
{
    One,
    Two,
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Eleven,
    Twelve,
    Thirteen,
    Fourteen,
    Fifteen,
    Sixteen,
    Seventeen,
    Eighteen,
    Nineteen,
    Twenty
}
