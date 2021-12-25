using System.Collections.Generic;
using UnityEngine;


public class BezierCurve : MonoBehaviour
{ 
	Mesh mesh;
	public GameObject _object;
	List<GameObject> points;
	public ComputeShader shader;

	[Range(10, 256)]
	[SerializeField] int steps = 80;
	int old_steps = 0;

	[Range(0.01f, 0.1f)]
	[SerializeField] float thickness = 0.05f;
	float old_thick = 0;
	Vector3[] bezier_points;

	int[] bicoefs;


	int fact(int n)
	{
		int res = 1;
		for(int i = 0; i < n; i++)res *= n - i;
		return res;
	}

	void CalculateBicoefs()
	{
		bicoefs = new int[points.Count];
		int n = bicoefs.Length - 1;
		for (int i = 0; i < bicoefs.Length; i++)
		{
			bicoefs[i] = (fact(n)) / ((fact(i)) * (fact(n-i)));
		}
	}

	void CalculateBezier()
	{
		float[] us = new float[steps];
		bezier_points = new Vector3[steps];

		List<Vector3> points_pos = points.ConvertAll<Vector3>(
			(GameObject _object) => _object.transform.position
			);

		ComputeBuffer _points = new ComputeBuffer(points.Count, sizeof(float) * 3);
		_points.SetData(points_pos);

		ComputeBuffer _bezier_points = new ComputeBuffer(steps, sizeof(float) * 3);
		ComputeBuffer _us = new ComputeBuffer(steps, sizeof(float));

		ComputeBuffer _bicoefs = new ComputeBuffer(bicoefs.Length, sizeof(int));
		_bicoefs.SetData(bicoefs);

		shader.SetBuffer(0, "points", _points);
		shader.SetBuffer(0, "bicoefs", _bicoefs);
		shader.SetBuffer(0, "bezier_points", _bezier_points);
		shader.SetInt("steps", steps);
		shader.SetBuffer(0, "us", _us);
		shader.Dispatch(0, steps, 1, 1);

		_bezier_points.GetData(bezier_points);
		_us.GetData(us);

		_points.Dispose();
		_us.Dispose();
		_bezier_points.Dispose();
		_bicoefs.Dispose();

		List<Vector3> verts = new List<Vector3>();
		List<int> triangles = new List<int>();
		List<Vector2> uv = new List<Vector2>();
		mesh.Clear();

		// constructing verts
		for (int i = 0; i < bezier_points.Length - 1; i++)
		{
			Vector3 forward =(bezier_points[i + 1] - bezier_points[i]).normalized;
			Quaternion rot = Quaternion.LookRotation(forward, Vector3.forward * -1);

			Vector3 p0 = bezier_points[i] + rot * (Vector3.right * thickness);
			Vector3 p1 = bezier_points[i] - rot * (Vector3.right * thickness);

			verts.Add(p0);
			uv.Add(new Vector2(us[i], 0));

			verts.Add(p1);
			uv.Add(new Vector2(us[i], 1));
		}
		
		Vector3 _forward =(bezier_points[bezier_points.Length - 1] - bezier_points[bezier_points.Length - 2]).normalized;
		Quaternion _rot = Quaternion.LookRotation(_forward, Vector3.forward * -1);

		Vector3 _p0 = bezier_points[bezier_points.Length - 1] + _rot * (Vector3.right * thickness);
		Vector3 _p1 = bezier_points[bezier_points.Length - 1] - _rot * (Vector3.right * thickness);

		verts.Add(_p0);
		uv.Add(new Vector2(us[bezier_points.Length - 1], 0));
		verts.Add(_p1);
		uv.Add(new Vector2(us[bezier_points.Length - 1], 1));

		//constructing triangles
		for (int i = 0; i < verts.Count - 2; i+=2)
		{
			triangles.Add(i);
			triangles.Add(i + 1);
			triangles.Add(i + 3);

			triangles.Add(i);
			triangles.Add(i + 3);
			triangles.Add(i + 2);
		}

		mesh.SetVertices(verts);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uv);
	}

	void AddPoint(Vector3 pos)
	{
		GameObject obj = Instantiate(_object, Vector3.zero, Quaternion.identity);
		obj.transform.parent = transform;
		obj.transform.localPosition = pos;
		points.Add(obj);
	}


	void Awake()
	{
		mesh = new Mesh();
		mesh.name = "BezierCurve";

		GetComponent<MeshFilter>().sharedMesh = mesh;

		points = new List<GameObject>();
		
		AddPoint(new Vector3(0, 0, 0));
		AddPoint(new Vector3(1, 0.5f, 0));
		AddPoint(new Vector3(2, -0.5f, 0));
		AddPoint(new Vector3(3f, 0.0f, 0));

		CalculateBicoefs();
		CalculateBezier();
	}

	void Update()
	{
		if(steps != old_steps || thickness != old_thick)
		{
			CalculateBezier();
			old_steps = steps;
			old_thick = thickness;
			return;
		}

		if(Input.GetMouseButtonDown(1))
		{
			Vector3 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			AddPoint(new Vector3(mouse_pos.x, mouse_pos.y, 0));
			//This is wrong // have to write a seperate function to calculate index in list
			points.Reverse(points.Count - 2, 2);

			CalculateBicoefs();
			CalculateBezier();
		}

		for (int i = 0; i < points.Count; i++)
		{
			if(points[i].transform.hasChanged)
			{
				points[i].transform.hasChanged = false;
				CalculateBezier();
			}
		}
	}

}
