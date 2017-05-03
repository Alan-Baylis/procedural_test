using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraMovement : MonoBehaviour {

	GameObject player;

	void Update () {
		if(player == null)
			player = GameObject.FindWithTag("player");

		Vector3 velocity = new Vector3(player.transform.position.x,player.transform.position.y,-10);
		this.transform.Translate(velocity);		
	}
}
