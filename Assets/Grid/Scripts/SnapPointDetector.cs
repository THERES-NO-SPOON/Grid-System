using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPointDetector : MonoBehaviour {


	public float Radius = 0.5f;
	public GameObject GridPrefab = null;

	
	public Vector3 SnapTargetPosition { protected set; get; }
	public Quaternion SnapTargetRotation { protected set; get; }


	private GridSystem grid;
	private SphereCollider detector;
	private List<Snappable> snappablesInRange;


	private void Start() {
		detector = GetComponent<SphereCollider>();
		detector.radius = Radius;

		snappablesInRange = new List<Snappable>();

		grid = Instantiate(GridPrefab).GetComponent<GridSystem>();
		grid.enabled = false;
	}


	private void Update() {
		//find the snap point that is closest to me
		if (snappablesInRange != null && snappablesInRange.Count > 0) {
			float minDistance = Mathf.Infinity;
			foreach (Snappable snappable in snappablesInRange) {
				for (int i = 0; i < snappable.SnapPoints.Length; i++) {
					Vector3 pos = snappable.GetSnapPoint(i);
					float distance = (pos - transform.position).magnitude;
					if (distance < minDistance) {
						minDistance = distance;
						SnapTargetPosition = pos;
						SnapTargetRotation = snappable.transform.rotation;
					}
				}
			}

			grid.enabled = true;
			grid.transform.position = SnapTargetPosition;
			grid.transform.rotation = SnapTargetRotation;
		}
		else {
			grid.enabled = false;
		}
	}


	private void OnTriggerStay(Collider other) {
		Snappable snappable = other.GetComponent<Snappable>();
		if(snappable != null && !snappablesInRange.Contains(snappable)) {
			snappablesInRange.Add(snappable);
		}
	}


	private void OnTriggerExit(Collider other) {
		Snappable snappable = other.GetComponent<Snappable>();
		if (snappable != null) snappablesInRange.Remove(snappable);
	}


	private void OnDestroy() {
		Destroy(grid.gameObject);
	}

}
