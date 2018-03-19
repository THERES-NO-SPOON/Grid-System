using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;


[Serializable]
public class SnapPoint {

	public enum SnapPointType {
		Stud, Tube
	}

	public Vector3 LocalPosition;
	public SnapPointType Type;


	public bool Match(SnapPoint other) {
		return Match(other.Type);
	}


	public bool Match(SnapPointType otherType) {
		return (Type == SnapPointType.Stud && otherType == SnapPointType.Tube) ||
			   (Type == SnapPointType.Tube && otherType == SnapPointType.Stud);
	}

}


[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Snappable : MonoBehaviour {

	[Header("Resources")]
	public SnapTargetDetector SnapTargetDetectorPrefab;
	public GridSystem GridSystemPrefab;
	public Material SnapPlaceholderMaterial = null;

	[Header("Snap Points")]
	public SnapPoint[] SnapPoints;


	public bool PickedUp { protected set; get; } = false;


	//the grid that I'm currently interacting with
	private GridSystem interactingGrid = null;
	private SnapTargetDetector snapTargetDetector = null;
	//when grabbed by the controller, the snap point which is closest to the controller
	private int selectedSnapPoint = -1;
	//a placeholder clone representing my transform after snap to the grid
	private GameObject placeholder = null;
	private Vector3 snapTo;
	private Quaternion restrictedRotation;


	private void Start() {
		Interactable interactable = GetComponent<Interactable>();
		interactable.onAttachedToHand += OnPickedUp;
		interactable.onDetachedFromHand += OnReleased;
	}


	private void Update() {
		if(placeholder != null) SnapPlaceholderToGrid();
	}


	private void OnDrawGizmos() {
		//visualize snap points
		if (SnapPoints != null) {
			for (int i = 0; i < SnapPoints.Length; i++) {
				bool isSelected = i == selectedSnapPoint;
				Gizmos.color = isSelected ? Color.red : Color.yellow;
				Gizmos.DrawSphere(GetSnapPointPosition(i), 0.005f);
			}
		}
	}


	private void OnTriggerEnter(Collider other) {
		if (PickedUp && interactingGrid == null) {
			//get the currently interacting grid system
			GridSystem grid = other.GetComponent<GridSystem>();
			if (grid != null) {
				interactingGrid = grid;
				ShowPlaceholder();
			}
		}
	}


	private void OnTriggerExit(Collider other) {
		GridSystem grid = other.GetComponent<GridSystem>();
		if(grid == interactingGrid) HidePlaceholder();
	}


	public void OnPickedUp(Hand hand) {
		PickedUp = true;
		FindSnapPointClosestToHolderHand(hand);

		snapTargetDetector = Instantiate(SnapTargetDetectorPrefab, transform.position, Quaternion.identity, transform);
		snapTargetDetector.GrabbedSnappable = this;
	}


	public void OnReleased(Hand hand) {
		PickedUp = false;
		selectedSnapPoint = -1;

		if (interactingGrid != null) SnapMeToGrid();

		Destroy(snapTargetDetector.gameObject);
		snapTargetDetector = null;
	}


	//get world position of specific snap point
	public Vector3 GetSnapPointPosition(int i) {
		return transform.TransformPoint(SnapPoints[i].LocalPosition);
	}


	//get world position of currently selected snap point
	public Vector3 GetSelectedSnapPointPosition() {
		if (selectedSnapPoint == -1) return transform.position;
		else return GetSnapPointPosition(selectedSnapPoint);
	}


	//find the snap point that is closest to the attaching controller
	//and mark it as the "selected snap point"
	public void FindSnapPointClosestToHolderHand(Hand hand) {
		selectedSnapPoint = -1;

		if (SnapPoints != null && SnapPoints.Length > 0) {
			Vector3 attachPoint = transform.InverseTransformPoint(hand.transform.position);
			float minDistance = Mathf.Infinity;
			for (int i = 0; i < SnapPoints.Length; i++) {
				float distance = (attachPoint - SnapPoints[i].LocalPosition).magnitude;
				if (distance < minDistance) {
					minDistance = distance;
					selectedSnapPoint = i;
				}
			}
		}
	}


	//show the placeholder (and snap it to the grip on next Update)
	private void ShowPlaceholder() {
		//clone me
		placeholder = Instantiate(gameObject);

		//destroy all components but Transform on the clone, we don't need them here
		//TODO is there a better, more generic way to remove components on a gameobject?
		//	   because there are dependencies amount those components, I can't just get them all and remov them with a for loop
		Destroy(placeholder.GetComponent<Throwable>());
		Destroy(placeholder.GetComponent<VelocityEstimator>());
		Destroy(placeholder.GetComponent<Interactable>());
		Destroy(placeholder.GetComponent<Snappable>());
		Destroy(placeholder.GetComponent<Rigidbody>());
		Destroy(placeholder.GetComponent<Collider>());

		//replace the material
		if (SnapPlaceholderMaterial != null) {
			foreach (Renderer renderer in placeholder.GetComponentsInChildren<Renderer>()) {
				renderer.material = SnapPlaceholderMaterial;
			}
		}

		HideOriginal();
	}


	//hide (destroy) the placeholder
	private void HidePlaceholder() {
		HideOriginal(false);
		Destroy(placeholder);
		placeholder = null;
		interactingGrid = null;
	}


	//show/hide the original gameobject
	private void HideOriginal(bool hide = true) {
		foreach (Renderer renderer in GetComponentsInChildren<Renderer>()) {
			renderer.enabled = !hide;
		}
	}


	private void SnapPlaceholderToGrid() {
		//snap the selected snap point to the grid, calculate the position and rotation for the placeholder
		interactingGrid.SnapToGrid(GetSelectedSnapPointPosition(), transform.rotation, out snapTo, out restrictedRotation);
		snapTo -= interactingGrid.transform.rotation * SnapPoints[selectedSnapPoint].LocalPosition;

		placeholder.transform.position = snapTo;
		placeholder.transform.rotation = restrictedRotation;
	}


	private void SnapMeToGrid() {
		transform.position = snapTo;
		transform.rotation = restrictedRotation;
		HidePlaceholder();
	}

}
