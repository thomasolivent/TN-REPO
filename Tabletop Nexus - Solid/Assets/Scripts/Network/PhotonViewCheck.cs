using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonViewCheck : Photon.MonoBehaviour
{

	// Use this for initialization
	void Start () {
        PhotonView pv = GetComponentInParent<PhotonView>();

        if (pv.isMine)
        {
            this.gameObject.SetActive(true);
        }else
        {
            this.gameObject.SetActive(false);
        }
	}
	
}
