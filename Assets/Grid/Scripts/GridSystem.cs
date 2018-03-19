using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class GridSystem : MonoBehaviour {

	public float GridSize = 1;
	public GameObject GridPointPrefab;

	//=0: no restriction
	//>0: rotation must be N times this value
	//<0: can only rotate as this grid does
	public Vector3 RotationRestriction = Vector3.one * 90;


	private int size = 6;
	private BoxCollider trigger;
	private GameObject container;


	private void Start() {
		//expand trigger to include all points
		trigger = GetComponent<BoxCollider>();
		trigger.size = Vector3.one * size * GridSize;
		VisualizeGrid();
	}


	private void OnEnable() {
		SetEnabled(true);
	}


	private void OnDisable() {
		SetEnabled(false);
	}


	private void SetEnabled(bool enabled) {
		if (trigger) trigger.enabled = enabled;
		container?.SetActive(enabled);
	}


	private void VisualizeGrid() {
		int max = size/2,
			min = -max + 1;

		container = new GameObject();
		container.transform.parent = transform;

		//TODO update grid when grid size is changed
		for (int u = min; u < max; u++) {
			for (int v = min; v < max; v++) {
				for (int w = min; w < max; w++) {
					Vector3 point = new Vector3(u, v, w);
					Vector3 position = GridCoordinateToWorld(point);
					float scale = Mathf.Max(0, max - point.magnitude) / max;

					GameObject p = Instantiate(GridPointPrefab, position, transform.rotation, container.transform);
					p.transform.localScale *= scale;
				}
			}
		}
	}


	//move the origin and rotate the coordinate system
	public void TransformOrigin(Vector3 position, Quaternion rotation) {
		transform.position = position;
		transform.rotation = rotation;
	}
	public void TransformOrigin(Vector3 position, Vector3 rotation) {
		TransformOrigin(position, Quaternion.Euler(rotation));
	}
	public void TransformOrigin(Transform otherTransform) {
		TransformOrigin(otherTransform.position, otherTransform.rotation);
	}


	public void SnapOriginTo(Snappable snappable=null, int snapPoint=-1) {
		if (snappable != null) {
			transform.parent = snappable.transform;
			transform.localPosition = snappable.SnapPoints[snapPoint].LocalPosition;
			transform.localRotation = Quaternion.identity;
		}
		else {
			transform.parent = null;
		}
	}


	//world position => local position
	public Vector3 WorldToLocal(Vector3 worldPosition) {
		return transform.InverseTransformPoint(worldPosition);
	}


	public Quaternion WorldToLocal(Quaternion worldRotation) {
		return Quaternion.Inverse(transform.rotation) * worldRotation;
	}


	//local position => world position
	public Vector3 LocalToWorld(Vector3 localPosition) {
		return transform.TransformPoint(localPosition);
	}


	public Quaternion LocalToWorld(Quaternion localRotation) {
		return transform.rotation * localRotation;
	}


	//grid coordinate [u,v,w] => world position [x,y,z]
	public Vector3 GridCoordinateToWorld(Vector3 coordinate) {
		return LocalToWorld(coordinate * GridSize);
	}


	//world position [x,y,z] => grid coordinate [u,v,w]
	public Vector3 WorldToGridCoordinate(Vector3 worldPosition) {
		Vector3 tmp = WorldToLocal(worldPosition) / GridSize;
		return new Vector3(Mathf.RoundToInt(tmp.x), Mathf.RoundToInt(tmp.y), Mathf.RoundToInt(tmp.z));
	}


	//get the world transform of the closest grip point
	public void SnapToGrid(Transform otherTransform, out Vector3 snapTo, out Quaternion restrictedRotation) {
		SnapToGrid(otherTransform.position, otherTransform.rotation, out snapTo, out restrictedRotation);
	}
	public void SnapToGrid(Vector3 position, Quaternion rotation, out Vector3 snapTo, out Quaternion restrictedRotation) {
		snapTo = GridCoordinateToWorld(WorldToGridCoordinate(position));
		restrictedRotation = GetRestrictedRotation(rotation);
	}


	private float GetRestrictedAngle(float angle, float restriction) {
		if (restriction > 0)		return Mathf.RoundToInt(angle / restriction) * restriction;
		else if (restriction < 0)	return 0;
		else						return angle;
	}


	private Quaternion GetRestrictedRotation(Quaternion worldRotation) {
		Vector3 angles = WorldToLocal(worldRotation).eulerAngles;

		Vector3 restricted = new Vector3(
			GetRestrictedAngle(angles.x, RotationRestriction.x),
			GetRestrictedAngle(angles.y, RotationRestriction.y),
			GetRestrictedAngle(angles.z, RotationRestriction.z)
		);

		return LocalToWorld(Quaternion.Euler(restricted));
	}

}
