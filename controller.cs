using UnityEngine;
using System.Collections;

// [RequireComponent (typeof (Controller2D))]
public class controller : MonoBehaviour {

	public float moveSpeed = 3f;

	Vector3 velocity;

	void Start () {
		//controller = GetComponent<Controller2D>();
	}
	
	void Update () {
		/*if(transform.position.x < -10 || transform.position.x > 10)
			velocity.x *= -1; 
		if(transform.position.y < -10 || transform.position.y > 10)
			velocity.y *= -1; */

		Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical"));

		velocity.x = input.x * moveSpeed; 
		velocity.y = input.y * moveSpeed;

//		Mathf.Clamp(velocity.x, 0f,10f);
//		Mathf.Clamp(velocity.y, 0f,10f);

		if(input == Vector2.zero)
			velocity = Vector3.zero;

		transform.Translate(velocity * Time.deltaTime);
	}
}
