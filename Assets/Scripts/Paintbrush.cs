using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Paintbrush : MonoBehaviour
{
	public LayerMask DrawMask;
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
	LineRenderer currentMark;
	Vector3 lastPos;
	List <Vector3> currentPositions = new List<Vector3> ();
	Collider lastColliderEntered;
	RaycastHit hit;

	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag ("Canvas")) {
			currentMark = new GameObject ("BrushStroke").AddComponent <LineRenderer> ();
			currentMark.gameObject.layer = gameObject.layer;
			currentMark.transform.parent = other.transform;
			currentMark.SetWidth (0.006f, 0.006f);
			currentMark.material = PaintMaterial;
			currentMark.material.SetColor ("_Color", PaintColor * LightColor);
			currentMark.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
			currentMark.receiveShadows = true;
			currentMark.useLightProbes = false;
			currentMark.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
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
					currentMark.SetColors (PaintColor * LightColor, PaintColor * LightColor);
					lastPos = hit.point;
					currentPositions.Add (lastPos);
					currentMark.material.SetColor ("_Color", PaintColor * LightColor);
					if (currentPositions.Count > 1) {
						currentMark.SetVertexCount (currentPositions.Count);
						for (int i = 0; i < currentPositions.Count; i++) {
							currentMark.SetPosition (i, currentPositions [i]);
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
