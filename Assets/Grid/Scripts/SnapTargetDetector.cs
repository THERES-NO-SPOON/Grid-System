using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapTargetDetector : MonoBehaviour {

	public delegate void SnapTargetDetectorEvent(SnapTargetDetector detector, Snappable target, int targetPoint, int snapPoint);
	public event SnapTargetDetectorEvent TargetChanged;


	public float Radius = 0.5f;
	

	[HideInInspector]
	public Snappable GrabbedSnappable = null;

	
	private SphereCollider detector;
	private List<Snappable> snappablesInRange;
	private Snappable prevSnapTarget = null;
	private int prevTargetPoint = -1;
	private int prevSnapPoint = -1;


	private void Start() {
		detector = GetComponent<SphereCollider>();
		detector.radius = Radius;

		snappablesInRange = new List<Snappable>();
	}


	private void Update() {
		Snappable target = null;
		int targetPoint = -1;
		int snapPoint = -1;
		
		if (snappablesInRange != null && snappablesInRange.Count > 0) {
			float minDistance = Mathf.Infinity;

			//for all the snap points on the grabbed snappable ...
			for (int i = 0; i < GrabbedSnappable.SnapPoints.Length; i++) {
				Vector3 p0 = GrabbedSnappable.GetSnapPointPosition(i);

				//for all the target points on detected other snappables ...
				foreach (Snappable snappable in snappablesInRange) {
					for (int j = 0; j < snappable.SnapPoints.Length; j++) {

						//if snap point and target point match type ...
						if (GrabbedSnappable.SnapPoints[i].Match(snappable.SnapPoints[j])) {
							Vector3 p1 = snappable.GetSnapPointPosition(j);

							//find the pair that are closest
							float distance = (p1 - p0).magnitude;
							if (distance < minDistance) {
								minDistance = distance;
								target = snappable;
								targetPoint = j;
								snapPoint = i;
							}
						}
					}
				}
			}
		}

		//if target or target point or snap point is changed, fire event
		if (prevSnapTarget != target && prevTargetPoint != targetPoint && prevSnapPoint != snapPoint) { 
			prevSnapTarget = target;
			prevTargetPoint = targetPoint;
			prevSnapPoint = snapPoint;
			TargetChanged?.Invoke(this, target, targetPoint, snapPoint);
		}
	}


	private void OnTriggerEnter(Collider other) {
		//only detect other ungrab snappable
		Snappable snappable = other.GetComponent<Snappable>();
		if(snappable != null && snappable != GrabbedSnappable && !snappable.PickedUp && !snappablesInRange.Contains(snappable)) {
			snappablesInRange.Add(snappable);
		}
	}


	private void OnTriggerExit(Collider other) {
		Snappable snappable = other.GetComponent<Snappable>();
		if (snappable != null) snappablesInRange.Remove(snappable);
	}

}
