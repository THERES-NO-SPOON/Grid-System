using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(VRTK_InteractableObject))]
public class Snappable : MonoBehaviour {

	public Material SnapPlaceholderMaterial = null;
	public Vector3[] SnapPoints;


	private GridSystem grid = null;
	private GameObject clone = null;
	private VRTK_InteractableObject interactableObject;


	private Vector3 snapPosition;
	private Quaternion snapRotation;


	private void Start() {
		//init VRTK events
		interactableObject = GetComponent<VRTK_InteractableObject>();
		interactableObject.InteractableObjectGrabbed += OnGrabbed;
		interactableObject.InteractableObjectUngrabbed += OnUngrabbed;
	}


	private void Update() {
		if(clone != null) SnapCloneToSnapPoint();

	}


	private void OnDrawGizmos() {
		if (SnapPoints != null) {
			Gizmos.color = Color.yellow;
			foreach (Vector3 snapPoint in SnapPoints) {
				Gizmos.DrawSphere(transform.TransformPoint(snapPoint), 0.005f);
			}
		}
	}


	private void OnTriggerStay(Collider other) {
		if (grid == null) {
			//get the interacting grid system
			GridSystem someGrid = other.GetComponent<GridSystem>();
			if (someGrid != null) {
				grid = someGrid;
				ShowPlaceholder();
			}
		}
	}


	private void OnTriggerExit(Collider other) {
		CleanUp();
	}


	private void OnGrabbed(object sender, InteractableObjectEventArgs e) {
		
	}


	private void OnUngrabbed(object sender, InteractableObjectEventArgs e) {
		if(grid != null) SnapMeToSnapPoint();
	}


	private void ShowPlaceholder() {
		clone = Instantiate(gameObject);
		Destroy(clone.GetComponent<Snappable>());
		Destroy(clone.GetComponent<Rigidbody>());
		Destroy(clone.GetComponent<Collider>());

		if (SnapPlaceholderMaterial != null) {
			foreach (Renderer renderer in clone.GetComponentsInChildren<Renderer>()) {
				renderer.material = SnapPlaceholderMaterial;
			}
		}

		HideOriginal();
	}


	private void CleanUp() {
		Destroy(clone);
		clone = null;
		grid = null;
		HideOriginal(false);
	}


	private void HideOriginal(bool hide=true) {
		foreach(Renderer renderer in GetComponentsInChildren<Renderer>()) {
			renderer.enabled = !hide;
		}
	}


	private void SnapCloneToSnapPoint() {
		grid.SnapToGrid(transform, out snapPosition, out snapRotation);
		clone.transform.position = snapPosition;
		clone.transform.rotation = snapRotation;
	}


	private void SnapMeToSnapPoint() {
		transform.position = snapPosition;
		transform.rotation = snapRotation;
		CleanUp();
	}

}
