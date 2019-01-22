using System.Collections;
using System.Collections.Generic;
using UMA.CharacterSystem;
using UnityEngine;

public class CharacterMovement : Photon.MonoBehaviour {

    float speed = 3f;
    float damp = 1f;

    Transform ImageTarget = null;

     Vector3 newMovePos;
     Vector3 adjMovePos;

    void Start()
    {
        newMovePos = transform.position;
        ImageTarget = FindObjectOfType<ImageTargetScaler>().gameObject.transform;
        //float scaleNum = (transform.localScale.x * ImageTarget.localScale.x) / 10;
        //transform.localScale = new Vector3(scaleNum, scaleNum, scaleNum);
    }

    void Update()
    {
        adjMovePos = newMovePos * (ImageTarget.localScale.x / 5);

        if (photonView.isMine || PhotonNetwork.connected == false)
        {
            if (transform.position != adjMovePos)
            {
                transform.position = Vector3.MoveTowards(transform.position, adjMovePos, (Time.deltaTime * damp) * speed);
            }
        }
    }

    public void Move(string dir)
    {
        if (photonView.isMine || PhotonNetwork.connected == false)
        {            
            switch (dir)
            {
                case "Left":
                    newMovePos += Vector3.left;
                    break;
                case "Right":
                    newMovePos += Vector3.right;
                    break;
                case "Forward":
                    newMovePos += Vector3.forward;
                    break;
                case "Back":
                    newMovePos += Vector3.back;
                    break;
                default:
                    break;
            }
        }
    }
}