using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UMA.CharacterSystem;

public class NetworkedCharacter : Photon.PunBehaviour {

	private Transform imageTarget;

	void Awake ()
	{
		if (PhotonNetwork.connected)
		{
			imageTarget = GameObject.FindWithTag("ImageTarget").transform;
			transform.parent = imageTarget;
			
			DynamicCharacterAvatar loadedAvatar = GetComponent<DynamicCharacterAvatar>();
			string myRecipe = (string)photonView.instantiationData[0];
			loadedAvatar.LoadFromRecipeString(myRecipe);
			loadedAvatar.BuildCharacter();
		}
	}

}
