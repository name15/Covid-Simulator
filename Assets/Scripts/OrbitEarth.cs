using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitEarth : MonoBehaviour
{
	public Transform target;
	public Transform directionalLight;
	
	public Vector2 lightOffset = new Vector2(-0.5f, 0.1f);	// Anchor the directional light to the camera with a given offset
	
	public float rotationSpeed = 5f;
	public float scaleSpeed = 10;

	[Range(0, 1)]
	public float movementSmoothness = 0.75f;
	[Range(0, 1)]
	public float secondaryMovementSmoothness = 0.9f; // TODO: use parabolic interpolation


	// Limit camera movement
	[Range(0.001f, Mathf.PI / 2)]
	public float poleLimit = 0.1f;	// Keep margin from the poles (in radians)
	public float scaleMin = 5.5f;	// Minimal distance to target position
	public float scaleMax = 20;     // Maximal distance to target position


	private Vector2 mouseDownCoords;
	
	private Vector3 initCameraPos;		// in spherical coords
	private Vector3 updateCameraPos;	// in spherical coords
	private Vector3 cameraPos;          // in spherical coords

	private bool useSecondaryMovementSmoothness = false;

	// Converts cartesian to spherical coordinates
	Vector3 Cartesian2Spherical(Vector3 c) {
		float r = c.magnitude;
		return new Vector3(
			Mathf.Atan2(c.z, c.x),      // phi
			Mathf.Asin(c.y / r),        // thetha
			r                           // radius
		);
	}

	// Converts spherical to cartesian coordinates
	Vector3 Spherical2Cartesian(Vector3 s) {
		return new Vector3(
			s.z * Mathf.Cos(s.y) * Mathf.Cos(s.x),
			s.z * Mathf.Sin(s.y),	
			s.z * Mathf.Cos(s.y) * Mathf.Sin(s.x)
		);
	}

	private void Start() {
		initCameraPos = Cartesian2Spherical(transform.position);
		cameraPos = initCameraPos;
	}

	private void OnValidate() {
		// Handle errors with scaleMin & scaleMax
		if (scaleMin < 0) {
			Debug.LogError("Scale Min must be positive.");
			scaleMin = Mathf.Abs(scaleMin);
		}
		if (scaleMax <= scaleMin) {
			Debug.LogError("Scale Max must be greater than Scale Max.");
			scaleMax = scaleMin + 1;
		}
	}

	// Limit vertical rotation & scale
	private void LimitMovement() {
		// Limit scale (using scaleMin & scaleMax)
		float scale = initCameraPos.z + updateCameraPos.z;
		if (scale < scaleMin) updateCameraPos.z = scaleMin - initCameraPos.z;
		if (scale > scaleMax) updateCameraPos.z = scaleMax - initCameraPos.z;

		// Limit vertical rotation (using poleLimit)
		float rotate = initCameraPos.y + updateCameraPos.y;
		if (Mathf.Abs(rotate) > Mathf.PI / 2 - poleLimit) updateCameraPos.y = (Mathf.PI / 2 - poleLimit) * Mathf.Sign(rotate) - initCameraPos.y;
	}

	// Set camera view from other script
	public void SetView(Vector3 sphericalCoords) {
		updateCameraPos = sphericalCoords - initCameraPos;
		useSecondaryMovementSmoothness = true;

		// Limit movement
		LimitMovement();
	}

	private bool dragEnabled = false;
	void Update() {
		bool overUI = EventSystem.current.IsPointerOverGameObject();
		
		float scale = initCameraPos.z + updateCameraPos.z;

		// Slowdown effect (applied to drag and scroll variables)
		float slowdown = 1 - 2 * Mathf.Asin(scaleMin / scale) / Mathf.PI;  // Tangent angle from camera to sphere
		slowdown = Mathf.Max(slowdown, 0.05f); // Min value (0.05 = bounce little when scale --> minScale)

		// Mouse press 
		if (Input.GetMouseButtonDown(0)) {
			if (!overUI) { // Should work only if mouse isn't over UI
				mouseDownCoords = Input.mousePosition;
				initCameraPos = cameraPos;
				updateCameraPos.z = 0;

				dragEnabled = true;
			} else {
				dragEnabled = false;
			}
		}

		// ScrollWheel
		if (!overUI) { // Should work only if mouse isn't over UI
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			scroll *= scaleSpeed * slowdown;

			updateCameraPos.z -= scroll;
		}

		// Mouse drag
		if (Input.GetMouseButton(0) && dragEnabled) { // Should work only if mouse has been clicked outside the UI
			Vector2 mouseDragCoords = Input.mousePosition;
			Vector2 drag = -(mouseDragCoords - mouseDownCoords) / new Vector2(Screen.width, Screen.height);
			drag *= rotationSpeed * slowdown;

			updateCameraPos.x = drag.x;
			updateCameraPos.y = drag.y;
		}

		// Limit movement
		LimitMovement();
	}

	private void FixedUpdate() {
		// Move with smoothening effect (using movementSmoothness / secondaryMovementSmoothness)
		float smooth = useSecondaryMovementSmoothness ? secondaryMovementSmoothness : movementSmoothness;
		cameraPos = Vector3.Lerp(cameraPos, initCameraPos + updateCameraPos, 1 - smooth);
		
		Vector3 pos = cameraPos;

		// Move Camera
		transform.position = target.position + Spherical2Cartesian(pos);
		transform.LookAt(target);

		// Move Directional Light
		pos.x += lightOffset.x; pos.y += lightOffset.y;
		directionalLight.position = target.position + Spherical2Cartesian(pos);
		directionalLight.LookAt(target);
	}
	
	//DEBUG
	private void OnGUI() {
		GUIStyle style = new GUIStyle();
		style.fontSize = 20;
		style.normal.textColor = Color.white;
		GUI.Label(new Rect(10, 10, 300, 10), cameraPos.ToString(), style);
	}
}
