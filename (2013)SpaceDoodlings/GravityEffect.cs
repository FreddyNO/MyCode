using UnityEngine;
using System.Collections;

public class GravityEffect : MonoBehaviour {
	
	public float gravity = 10.0f;
	public bool freeRotation;
	public bool is3D;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		applyGravity();
	}
	
	void applyGravity(){
		//Controls gravity and rotation for objects
		if(Planet.planet != null){
			Vector3 direction = Planet.gravityLocation.position - transform.position;
			direction = direction.normalized;
			rigidbody.AddForce(new Vector3(direction.x,direction.y,0) * gravity * CameraControls.time);
			
			if(!freeRotation){
				float angle = Mathf.Atan2(direction.y, direction.x)* Mathf.Rad2Deg;
				
				transform.eulerAngles = new Vector3(0, 0, angle);
				if(is3D)
					transform.Rotate(new Vector3(90, -90, 0));
				if(!is3D)
					transform.Rotate(new Vector3(0, 90, -90));
			}
		}
	}
}
