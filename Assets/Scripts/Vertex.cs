using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Vertex : MonoBehaviour {
	[Range(0f, 1f)]
	public float Lightness = 0f;
	public List<Edge> ConnectedEdges = new List<Edge>();
	public int Connectedness => this.ConnectedEdges.Count;

	private Rigidbody _rigidbody;

	public void Start() {
		this.GetComponentInChildren<MeshRenderer>().material.color = Color.Lerp(Color.black, Color.white, Lightness);
		this._rigidbody = this.GetComponent<Rigidbody>();
	}

	public Vector3 GetRepulsionForce(Vertex otherVertex) {
		Vector3 difference = otherVertex.transform.position - this.transform.position;
		float distanceSquared = difference.sqrMagnitude;
		if(distanceSquared < 0.0001f) { return Vector3.zero; }
		return (Connectedness / distanceSquared) * difference.normalized;
	}

	public void Move(IEnumerable<Vertex> otherVertices, float deltaTime, float maximumForceMagnitude, float floor, float ceiling) {
		// Calculate each force that acts on this vertex.
		// Start with the repulsion forces from the other vertices.
		IEnumerable<Vector3> repulsionForces = otherVertices.Select(v=>v.GetRepulsionForce(this));

		// At the same time, the edges will be tugging on this vertex if the maximum length is exceeded.
		IEnumerable<Vector3> pullForces = this.ConnectedEdges.Select(edge=>edge.GetPullStrength(this));

		// Then the force for the vertical position.
		// We need to convert the lightness to a value between floor and ceiling.
		float idealFloatHeight = (ceiling - floor) * this.Lightness + floor;
		Vector3 floatForce = Vector3.up * (idealFloatHeight - this.transform.position.y);

		// And we need a bit of wiggle to keep from following too strict a pattern.
		Vector3 wiggleForce = (Vector3.forward * Random.value + Vector3.right * Random.value + Vector3.up * Random.value) * 0.001f;

		// Put all the forces together.
		Vector3 compositeForce = Vector3.zero;
		foreach(Vector3 repulsionForce in repulsionForces) {
			compositeForce += repulsionForce;
		}

		foreach(var pullForce in pullForces) {
			compositeForce += pullForce;
		}

		compositeForce += floatForce;
		compositeForce += wiggleForce;

		// And scale them to the elapsed time and maximum move speed.
		// this.transform.Translate(Vector3.ClampMagnitude(compositeForce * deltaTime, maximumMoveDelta));
		this._rigidbody.AddForce(Vector3.ClampMagnitude(compositeForce, maximumForceMagnitude));
	}
}
