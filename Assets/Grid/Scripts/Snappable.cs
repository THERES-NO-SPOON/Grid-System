using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Snappable : MonoBehaviour {

	public Material SnapPlaceholderMaterial = null;

	private GridSystem grid = null;
	private GameObject clone = null;


	private void Update() {
		if(clone != null) {
			Vector3 snapPoint = grid.SnapPosition(transform.position);
			clone.transform.position = snapPoint;
			clone.transform.rotation = grid.transform.rotation;
		}
	}


	private void OnTriggerEnter(Collider other) {
		GridSystem someGrid = other.GetComponent<GridSystem>();
		if (someGrid != null) {
			grid = someGrid;

			clone = Instantiate(gameObject);
			Destroy(clone.GetComponent<Snappable>());
			Destroy(clone.GetComponent<Rigidbody>());
			Destroy(clone.GetComponent<Collider>());
			if(SnapPlaceholderMaterial != null) {
				clone.GetComponent<Renderer>().material = SnapPlaceholderMaterial;
			}
		}
	}


	private void OnTriggerExit(Collider other) {
		grid = null;

		Destroy(clone);
		clone = null;
	}

}
