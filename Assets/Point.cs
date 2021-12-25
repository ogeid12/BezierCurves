using UnityEngine;

public class Point : MonoBehaviour
{
	private Vector3 mOffset;
	private float mZCoord;

	void OnMouseDown()

	{
		mZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
		// Store offset = gameobject world pos - mouse world pos
		mOffset = gameObject.transform.position - GetMouseAsWorldPoint();
	}

	private Vector3 GetMouseAsWorldPoint()
	{
		// Pixel coordinates of mouse (x,y)
		Vector3 mousePoint = Input.mousePosition;
		mousePoint.z = mZCoord;
		return Camera.main.ScreenToWorldPoint(mousePoint);

	}

	void OnMouseDrag()
	{
		transform.position = GetMouseAsWorldPoint() + mOffset;
	}
}