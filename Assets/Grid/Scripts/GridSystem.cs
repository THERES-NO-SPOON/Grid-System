using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour {

	public float GridSize = 1;
	public GameObject Target;

	
	private Vector3 _snapPosition = Vector3.zero;


	private void Update() {
		if(Target != null) {
			_snapPosition = SnapPosition(Target.transform.position);
		}
	}


	private void OnDrawGizmos() {
		VisualizeGrid();

		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(_snapPosition, Vector3.one * .2f);
	}


	private void VisualizeGrid(int size = 5) {
		Gizmos.color = Color.yellow;

		int min = -size + 1;
		for (int u = min; u < size; u++) {
			for (int v = min; v < size; v++) {
				for (int w = min; w < size; w++) {
					Vector3 point = GridCoordinateToWorld(new Vector3(u, v, w));
					Gizmos.DrawRay(point, transform.forward * GridSize);
					Gizmos.DrawRay(point, transform.right * GridSize);
					Gizmos.DrawRay(point, transform.up * GridSize);
					Gizmos.DrawSphere(point, 0.1f);
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
	public void TransformOrigin(Transform transform) {
		TransformOrigin(transform.position, transform.rotation);
	}


	//world position => local position
	public Vector3 WorldToLocal(Vector3 worldPosition) {
		return transform.InverseTransformPoint(worldPosition);
	}


	//local position => world position
	public Vector3 LocalToWorld(Vector3 localPosition) {
		return transform.TransformPoint(localPosition);
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


	//get the world position of the closest grip point
	public Vector3 SnapPosition(Vector3 worldPosition) {
		return GridCoordinateToWorld(WorldToGridCoordinate(worldPosition));
	}

}
