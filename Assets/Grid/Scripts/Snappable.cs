using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Snappable : MonoBehaviour {

	public Material SnapPlaceholderMaterial = null;
	public Vector3[] SnapPoints;


	private GridSystem grid = null;
	private GameObject clone = null;


	private Vector3 snapPosition;
	private Quaternion snapRotation;
	private Rigidbody rb;
	private bool pickedUp = false;


	private void Start() {
		rb = GetComponent<Rigidbody>();
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
		if (pickedUp && grid == null) {
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


	public void OnPickUp() {
		pickedUp = true;
	}


	public void OnDetachFromHand() {
		pickedUp = false;

		if (grid != null) SnapMeToSnapPoint();
	}


	private void ShowPlaceholder() {
		clone = Instantiate(gameObject);
		Destroy(clone.GetComponent<Snappable>());
		Destroy(clone.GetComponent<Throwable>());
		Destroy(clone.GetComponent<Interactable>());
		Destroy(clone.GetComponent<VelocityEstimator>());
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
