using UnityEngine;
using System.Collections;

// [RequireComponent (typeof (Controller2D))]
public class controller : MonoBehaviour {

	public float moveSpeed = 6f;
	Vector2 velocity;

	void Start () {
		//controller = GetComponent<Controller2D>();
	}
	
	void Update () {
		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));
		Vector2 direction = input.normalized;
		velocity = direction*moveSpeed;
	}

	void FixedUpdate(){
		transform.Translate(velocity * Time.fixedDeltaTime);
	}

	void OnTriggerEnter2D(Collider2D col){
		if(col.gameObject.tag == "pickup"){
			Debug.Log("picked up");
			Destroy(col.gameObject);
		}
		
	}
}