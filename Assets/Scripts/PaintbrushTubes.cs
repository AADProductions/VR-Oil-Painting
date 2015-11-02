using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PaintbrushTubes : MonoBehaviour
{
	public LayerMask DrawMask;
	public Camera MainCamera;
	public Vector3 PositionOffset;
	public Vector3 RotationOffset;
	public MeshRenderer PaintbrushTip;
	public Transform BrushTip;
	public Transform CheckTip;
	public float MaxPointDistance = 0.002f;
	public float MaxDrawDistance = 0.01f;
	public float MaxPickDistance = 0.05f;
	public Material PaintMaterial;
	public SteamVR_TrackedObject Controller;
	public Color PaintColor = Color.white;
	public Color LightColor = Color.white;
	public Texture2D PaletteTexture;
	bool inContact;
	bool pickingColor;
	TubeRenderer currentMark;
	Vector3 lastPos;
	List <Vector3> currentPositions = new List<Vector3> ();
	Collider lastColliderEntered;
	RaycastHit hit;

	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag ("Canvas")) {
			currentMark = new GameObject ("BrushStroke").AddComponent <TubeRenderer> ();
			currentMark.gameObject.layer = gameObject.layer;
			currentMark.transform.parent = other.transform;
			currentMark.MainCamera = MainCamera;
			currentMark.material = new Material (PaintMaterial);
			currentMark.material.color = PaintColor * LightColor;
			currentPositions.Clear ();
			var device = SteamVR_Controller.Input ((int)Controller.index);
			device.TriggerHapticPulse (500, Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
			inContact = true;
		} else if (other.CompareTag ("Palette")) {
			pickingColor = true;
		}
	}

	void OnTriggerExit (Collider other)
	{
		if (other.CompareTag ("Canvas")) {
			currentMark = null;
			inContact = false;
		} else if (other.CompareTag ("Palette")) {
			pickingColor = false;
		}
	}
			
	void FixedUpdate ()
	{	
		if (inContact) {
			if (Physics.Raycast (BrushTip.position, BrushTip.forward, out hit, MaxDrawDistance, DrawMask, QueryTriggerInteraction.Ignore)) {
				if (currentPositions.Count == 0 || Vector3.Distance (lastPos, hit.point) > MaxPointDistance) {
					lastPos = hit.point;
					currentPositions.Add (lastPos);
					if (currentPositions.Count > 1) {
						currentMark.vertices = new TubeRenderer.TubeVertex [currentPositions.Count];
						for (int i = 0; i < currentPositions.Count; i++) {
							currentMark.vertices [i] = new TubeRenderer.TubeVertex (currentPositions [i], 0.006f, PaintColor * LightColor);
						}
					}
					var device = SteamVR_Controller.Input ((int)Controller.index);
					device.TriggerHapticPulse (250, Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
				}
			} else {
				currentMark = null;
				inContact = false;
			}
		} else if (pickingColor) {
			if (Physics.Raycast (BrushTip.position, BrushTip.forward, out hit, MaxPickDistance, DrawMask, QueryTriggerInteraction.Ignore)) {
				var device = SteamVR_Controller.Input ((int)Controller.index);
				device.TriggerHapticPulse (250, Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);
				int width = Mathf.FloorToInt (hit.textureCoord.x * PaletteTexture.width);
				int height = Mathf.FloorToInt (hit.textureCoord.y * PaletteTexture.height);
				PaintColor = PaletteTexture.GetPixel (width, height);
				PaintbrushTip.material.color = PaintColor;
			}
		}
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
			Application.Quit ();
		}
	}
}
