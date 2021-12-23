using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.VisualScripting;

using UnityEngine;

using static Randomization;

public class Manager : MonoBehaviour {
	// Prefabs.
	public Vertex VertexPrefab;
	public Edge EdgePrefab;
	public Transform Container;

	// Variables that are only evaluated at start.
	[Range(1f, 1000f)]
	public int VerticesToSpawn;
	[Range(1f, 50f)]
	[Tooltip("Not used when the graph is acyclic. Because the graph is always connected.")]
	public float AverageConnectedness;
	public bool Acyclic;
	public float EdgeMaxLength = 10f;
	public float EdgeStiffness = 10f;

	// Variables that may change at runtime.
	public float GraphCeiling = 10f;
	public float GraphFloor = -10f;
	public float RepulsionForceScale = 1f;
	public float PullForceScale = 1f;
	public float MaxForceMagnitude = 10f;

	// Internal variables.
	private List<Edge> edges = new List<Edge>();
	private List<Vertex> vertices = new List<Vertex>();

	// Start is called before the first frame update
	public void Start() {
		// Spawn all vertices.
		for(int i = 0; i < this.VerticesToSpawn; i++) {
			Vertex vertex = Instantiate(this.VertexPrefab, this.Container);
			vertex.Lightness = Random.value;
			this.vertices.Add(vertex);
		}

		// Spawn all edges.
			var unconnectedVertices = new RandomList<Vertex>(this.vertices);
			var connectedVertices = new RandomList<Vertex>();
			connectedVertices.Add(unconnectedVertices.RemoveOne());

			while(unconnectedVertices.Any()) {
				Vertex parent = connectedVertices.GetOne();
				Vertex child = unconnectedVertices.RemoveOne();
				this.SpawnEdge(parent, child);
				connectedVertices.Add(child);
			}

			if (!Acyclic) {
			int maxEdges = this.vertices.Count*(this.vertices.Count-1)/2;
			int edgesToSpawn = (int) Mathf.Min(maxEdges, this.vertices.Count * AverageConnectedness);
			float actualAverageConnectedness = (float)edgesToSpawn/(float)this.vertices.Count;
			float currentAverageConnectedness = (float)this.edges.Count / (float)this.vertices.Count;
			while(currentAverageConnectedness < actualAverageConnectedness) {
				var notFullyConnectedVertices = new RandomList<Vertex>(this.vertices.Where(v=>v.Connectedness < this.vertices.Count - 1));
				Vertex v1 = notFullyConnectedVertices.RemoveOne();

				notFullyConnectedVertices = new RandomList<Vertex>(notFullyConnectedVertices
					.Where(v =>
					!v.ConnectedEdges.Select(e => e.VertexA).Contains(v1)
					|| !v.ConnectedEdges.Select(e => e.VertexB).Contains(v1)));
				Vertex v2 = notFullyConnectedVertices.RemoveOne();

				this.SpawnEdge(v1 , v2);
				unconnectedVertices.Remove(v2);

				currentAverageConnectedness = (float) this.edges.Count/(float) this.vertices.Count;
			}
		}
	}

	// Update is called once per frame
	public void Update() {
		float deltaTime = Time.deltaTime;
		foreach(var vertex in this.vertices) {
			vertex.Move(this.vertices.Where(v => v!=vertex), deltaTime, this.MaxForceMagnitude * deltaTime, this.GraphFloor, this.GraphCeiling);
		}
		foreach(var edge in this.edges) {
			edge.transform.position = edge.VertexA.transform.position;
			edge.transform.LookAt(edge.VertexB.transform);
			edge.transform.localScale = new Vector3(1, 1, Vector3.Distance(edge.VertexA.transform.position, edge.VertexB.transform.position));
		}
	}

	private Edge SpawnEdge(Vertex v1, Vertex v2) {
		Edge edge = Instantiate(this.EdgePrefab, this.Container);
		edge.VertexA = v1;
		edge.VertexB = v2;
		edge.MaxLength = this.EdgeMaxLength;
		edge.Stiffness = this.EdgeStiffness;

		v1.ConnectedEdges.Add(edge);
		v2.ConnectedEdges.Add(edge);

		this.edges.Add(edge);
		return edge;
	}
}
