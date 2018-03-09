using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Connector : MonoBehaviour {

	public enum ConnectorType {
		Stud,
		Tube,
		Pin,
		Axle,
		Hole,
		AxleHole,
	}


	public ConnectorType Type;
	public Vector3 ValidDirection;


	public List<ConnectorType> AcceptableTypes { protected set; get; }


	private void Awake() {
		AcceptableTypes = GetAcceptableTypes();

		if(ValidDirection == Vector3.zero) {
			ValidDirection = transform.up;
		}
	}


	private List<ConnectorType> GetAcceptableTypes() {
		switch(Type) {
			case ConnectorType.Stud:		return new List<ConnectorType>() { ConnectorType.Tube, ConnectorType.Hole };
			case ConnectorType.Tube:		return new List<ConnectorType>() { ConnectorType.Stud };
			case ConnectorType.Pin:			return new List<ConnectorType>() { ConnectorType.Hole };
			case ConnectorType.Axle:		return new List<ConnectorType>() { ConnectorType.Hole, ConnectorType.AxleHole };
			case ConnectorType.Hole:		return new List<ConnectorType>() { ConnectorType.Stud, ConnectorType.Pin, ConnectorType.Axle };
			case ConnectorType.AxleHole:	return new List<ConnectorType>() { ConnectorType.Axle };
			default: return null;
		}
	}


	public bool CanAccept(Connector other) {
		return AcceptableTypes.Contains(other.Type) && Vector3.Angle(ValidDirection, other.ValidDirection) < 20;
	}




}
