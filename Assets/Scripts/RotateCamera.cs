using UnityEngine;

public class RotateCamera : MonoBehaviour
{
	public Transform target;
	public float rotationSpeed = 3.14f;
	public float scaleSpeed = 10;

	[Range(0, 1)]
	public float movementSmoothness = 0.75f;
	[Range(0.001f, 30)]
	public float scaleExponent = 20;

	[Range(0.001f, Mathf.PI / 2)]
	public float poleLimit = 0.1f;
	public float scaleMin = 5.3f;
	public float scaleMax = 30;


	private Vector2 mouseDownCoords;
	
	private Vector3 initCameraPos;		// in spherical coords
	private Vector3 updateCameraPos;	// in spherical coords
	private Vector3 cameraPos;          // in spherical coords

	// Converts cartesian to spherical coordinates
	Vector3 Cartesian2Spherical(Vector3 c) {
		float r = c.magnitude;
		return new Vector3(
			Mathf.Atan2(c.z, c.x),
			Mathf.Asin(c.y / r),
			r
		);
	}

	// Converts spherical to cartesian coordinates
	Vector3 Spherical2Cartesian(Vector3 s) { // the z component is the radius
		return new Vector3(
			s.z * Mathf.Cos(s.y) * Mathf.Cos(s.x),
			s.z * Mathf.Sin(s.y),
			s.z * Mathf.Cos(s.y) * Mathf.Sin(s.x)
		);
	}

	private void Start() {
		updateCameraPos = Cartesian2Spherical(transform.position);
	}

	private void OnValidate() {
		// Handle errors with ScaleMin & ScaleMax
		if (scaleMin < 0) {
			Debug.LogError("Scale Min must be positive.");
			scaleMin = Mathf.Abs(scaleMin);
		}
		if (scaleMax <= scaleMin) {
			Debug.LogError("Scale Max must be greater than Scale Max.");
			scaleMax = scaleMin + 1;
		}
	}

	void Update()
	{
		// Mouse press 
		if (Input.GetMouseButtonDown(0)) {
			mouseDownCoords = Input.mousePosition;
			initCameraPos = cameraPos;
			updateCameraPos.z = 0;
		} 
		// Mouse drag
		if (Input.GetMouseButton(0)) {
			Vector2 mouseDragCoords = Input.mousePosition;
			Vector2 drag = - (mouseDragCoords - mouseDownCoords) / new Vector2(Screen.width, Screen.height);
			drag *= rotationSpeed * cameraPos.z;

			updateCameraPos.x = drag.x;
			updateCameraPos.y = drag.y;

			// Limit vertical rotation (using poleLimit)
			float rotate = initCameraPos.y + updateCameraPos.y;
			if (Mathf.Abs(rotate) > Mathf.PI / 2 - poleLimit) updateCameraPos.y = (Mathf.PI / 2 - poleLimit) * Mathf.Sign(rotate) - initCameraPos.y;
		}

		// ScrollWheel
		float scroll = Input.GetAxis("Mouse ScrollWheel");
		scroll *= scaleSpeed;

		updateCameraPos.z -= scroll;

			// Limit scale (using scaleMin & scaleMax)
		float scale = initCameraPos.z + updateCameraPos.z;
		if (scale < scaleMin) updateCameraPos.z = scaleMin - initCameraPos.z; //TODO
		if (scale > scaleMax) updateCameraPos.z = scaleMax - initCameraPos.z; //TODO
	}

	private void FixedUpdate() {
		// Move with smoothening effect (using movementSmoothness)
		cameraPos = Vector3.Lerp(cameraPos, initCameraPos + updateCameraPos, 1 - movementSmoothness);

		// Slow down scroll when scale is small (using scaleExponent)
		float limit = (scaleMin - 1 / scaleExponent);
		Vector3 cam= cameraPos; 
		cam.z = Mathf.Pow(
			Mathf.Pow(scaleExponent * (scaleMax - limit), 1 / scaleMax),
			cameraPos.z
		) / scaleExponent + limit;

		// Move Camera
		transform.position = target.position + Spherical2Cartesian(cam);
		transform.LookAt(target);
	}
}
