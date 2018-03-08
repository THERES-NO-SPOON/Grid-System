using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(VRTK_InteractableObject))]
public class Snappable : MonoBehaviour {

	public Material SnapPlaceholderMaterial = null;


	private GridSystem grid = null;
	private GameObject clone = null;
	private VRTK_InteractableObject interactableObject;


	private Vector3 snapPosition;
	private Vector3 snapRotation;


	private void Start() {
		//init VRTK events
		interactableObject = GetComponent<VRTK_InteractableObject>();
		interactableObject.InteractableObjectUngrabbed += OnUngrabbed;
	}


	private void Update() {
		if(clone != null) SnapCloneToSnapPoint();
	
	}


	private void OnTriggerStay(Collider other) {
		if (grid == null) {
			//get the interacting grid system
			GridSystem someGrid = other.GetComponent<GridSystem>();
			if (someGrid != null) {
				grid = someGrid;
				InitClone();
			}
		}
	}


	private void OnTriggerExit(Collider other) {
		CleanUp();
	}


	private void OnUngrabbed(object sender, InteractableObjectEventArgs e) {
		if(grid != null) SnapMeToSnapPoint();
	}


	private void InitClone() {
		clone = Instantiate(gameObject);
		Destroy(clone.GetComponent<Snappable>());
		Destroy(clone.GetComponent<Rigidbody>());
		Destroy(clone.GetComponent<Collider>());

		if (SnapPlaceholderMaterial != null) {
			foreach (Renderer renderer in clone.GetComponentsInChildren<Renderer>()) {
				renderer.material = SnapPlaceholderMaterial;
			}
		}
	}


	private void SnapCloneToSnapPoint() {
		grid.SnapToGrid(transform, out snapPosition, out snapRotation);
		clone.transform.position = snapPosition;
		clone.transform.eulerAngles = snapRotation;
	}


	private void SnapMeToSnapPoint() {
		transform.position = snapPosition;
		transform.eulerAngles = snapRotation;
		CleanUp();
	}


	private void CleanUp() {
		Destroy(clone);
		clone = null;
		grid = null;
	}

}
