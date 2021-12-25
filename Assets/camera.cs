using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camera : MonoBehaviour
{
	// Start is called before the first frame update

	public GameObject objectToPoint;

	void Start()
	{
		Bounds targetBounds = objectToPoint.GetComponent<Renderer>().bounds;

		float screenRatio = (float)Screen.width / (float)Screen.height;
		float targetRatio = targetBounds.size.x / targetBounds.size.y;

		if (screenRatio >= targetRatio)
		{
			Camera.main.orthographicSize = targetBounds.size.y / 2;
		}
		else
		{
			float differenceInSize = targetRatio / screenRatio;
			Camera.main.orthographicSize = targetBounds.size.y / 2 * differenceInSize;
		}

		transform.position = new Vector3(targetBounds.center.x, targetBounds.center.y, -1f);
	}
}
