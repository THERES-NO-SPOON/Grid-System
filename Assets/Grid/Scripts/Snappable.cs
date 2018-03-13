using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Snappable : MonoBehaviour {

	public Material SnapPlaceholderMaterial = null;
	public Vector3[] SnapPoints;


	//the grid that I'm currently interacting with
	private GridSystem grid = null;
	//a placeholder clone representing my transform after snap to the grid
	private GameObject clone = null;
	//when grabbed by the controller, the snap point which is closest to the controller
	private int selectedSnapPoint = -1;
	//world position of the selected snap point
	private Vector3 selectedSnapPosition {
		get {
			if (selectedSnapPoint == -1) return transform.position;
			else return transform.TransformPoint(SnapPoints[selectedSnapPoint]);
		}
	}
	private Vector3 snapTo;
	private Quaternion restrictedRotation;


	private bool pickedUp = false;


	private void Start() {
		Interactable interactable = GetComponent<Interactable>();
		interactable.onAttachedToHand += OnAttachedToHand;
		interactable.onDetachedFromHand += OnDetachFromHand;
	}


	private void Update() {
		if(clone != null) SnapCloneToGrid();

	}


	private void OnDrawGizmos() {
		//visualize snap points
		if (SnapPoints != null) {
			for (int i = 0; i < SnapPoints.Length; i++) {
				bool isSelected = i == selectedSnapPoint;
				Gizmos.color = isSelected ? Color.red : Color.yellow;
				Gizmos.DrawSphere(transform.TransformPoint(SnapPoints[i]), 0.005f);
			}
		}
	}


	private void OnTriggerStay(Collider other) {
		if (pickedUp && grid == null) {
			//get the currently interacting grid system
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


	public void OnAttachedToHand(Hand hand) {
		pickedUp = true;

		//find the snap point that is closest to the attaching controller
		if (SnapPoints != null && SnapPoints.Length > 0) {
			Vector3 attachPoint = transform.InverseTransformPoint(hand.transform.position);
			float minDistance = Mathf.Infinity;
			for (int i = 0; i < SnapPoints.Length; i++) {
				float distance = (attachPoint - SnapPoints[i]).magnitude;
				if (distance < minDistance) {
					minDistance = distance;
					selectedSnapPoint = i;
				}
			}
		}
	}


	public void OnDetachFromHand(Hand hand) {
		pickedUp = false;
		selectedSnapPoint = -1;

		if (grid != null) SnapMeToGrid();
	}


	private void ShowPlaceholder() {
		clone = Instantiate(gameObject);

		//destroy all components but Transform on the clone, we don't need them here
		Destroy(clone.GetComponent<Throwable>());
		Destroy(clone.GetComponent<VelocityEstimator>());
		Destroy(clone.GetComponent<Interactable>());
		Destroy(clone.GetComponent<Snappable>());
		Destroy(clone.GetComponent<Rigidbody>());
		Destroy(clone.GetComponent<Collider>());

		//replace the material
		if (SnapPlaceholderMaterial != null) {
			foreach (Renderer renderer in clone.GetComponentsInChildren<Renderer>()) {
				renderer.material = SnapPlaceholderMaterial;
			}
		}

		HideOriginal();
	}


	private void HideOriginal(bool hide = true) {
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
			renderer.enabled = !hide;
		}
	}


	private void CleanUp() {
		HideOriginal(false);
		Destroy(clone);
		clone = null;
		grid = null;
	}


	private void SnapCloneToGrid() {
		grid.SnapToGrid(selectedSnapPosition, transform.rotation, out snapTo, out restrictedRotation);
		clone.transform.position = snapTo;
		clone.transform.rotation = restrictedRotation;
	}


	private void SnapMeToGrid() {
		transform.position = snapTo;
		transform.rotation = restrictedRotation;
		CleanUp();
	}

}
