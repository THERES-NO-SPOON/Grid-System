using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class GridSystem : MonoBehaviour {

	public float GridSize = 1;
	public GameObject GridPoint;

	//=0: no restriction
	//>0: rotation must be N times this value
	//<0: can only rotate as this grid does
	public Vector3 RotationRestriction = Vector3.one * 90;


	private int size = 6;
	private BoxCollider trigger;


	private void Start() {
		trigger = GetComponent<BoxCollider>();
		trigger.size = Vector3.one * size * GridSize;

		VisualizeGrid();
	}


	private void VisualizeGrid() {
		int max = size/2,
			min = -max + 1;

		//TODO update grid when grid size is changed
		for (int u = min; u < max; u++) {
			for (int v = min; v < max; v++) {
				for (int w = min; w < max; w++) {
					Vector3 point = new Vector3(u, v, w);
					Vector3 position = GridCoordinateToWorld(point);
					float scale = Mathf.Max(0, max - point.magnitude) / max;

					GameObject p = Instantiate(GridPoint, position, transform.rotation, transform);
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
	public void SnapToGrid(Transform otherTransform, out Vector3 position, out Quaternion rotation) {
		position = GridCoordinateToWorld(WorldToGridCoordinate(otherTransform.position));
		rotation = GetRestrictedRotation(otherTransform.rotation);
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
