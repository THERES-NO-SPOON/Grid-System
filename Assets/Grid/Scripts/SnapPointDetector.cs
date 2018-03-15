using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapPointDetector : MonoBehaviour {


	public float Radius = 0.5f;
	public GridSystem GridPrefab = null;
	public GridSystem Grid { protected set; get; }
	

	[HideInInspector]
	public Snappable IgnoreSnappable = null;

	
	public Vector3 SnapTargetPosition { protected set; get; }
	public Quaternion SnapTargetRotation { protected set; get; }

	
	private SphereCollider detector;
	private List<Snappable> snappablesInRange;


	private void Start() {
		detector = GetComponent<SphereCollider>();
		detector.radius = Radius;

		snappablesInRange = new List<Snappable>();

		Grid = Instantiate(GridPrefab);
		Grid.enabled = false;
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

			Grid.enabled = true;
			Grid.transform.position = SnapTargetPosition;
			Grid.transform.rotation = SnapTargetRotation;
		}
		else {
			Grid.enabled = false;
		}
	}


	private void OnTriggerEnter(Collider other) {
		Snappable snappable = other.GetComponent<Snappable>();
		if(snappable != null && snappable != IgnoreSnappable && !snappable.PickedUp && !snappablesInRange.Contains(snappable)) {
			snappablesInRange.Add(snappable);
		}
	}


	private void OnTriggerExit(Collider other) {
		Snappable snappable = other.GetComponent<Snappable>();
		if (snappable != null) snappablesInRange.Remove(snappable);
	}


	private void OnDestroy() {
		Destroy(Grid.gameObject);
	}

}
