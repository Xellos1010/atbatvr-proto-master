using UnityEngine;
using System.Collections;

public class BillboardPlane : MonoBehaviour
{

    public float minRotation;
    public float maxRotation;

    private float minRotationCamera;
    private float maxRotationCamera;

    Camera _camera;

	// Use this for initialization
	void Awake()
    {
        minRotationCamera = 330;
        maxRotationCamera = 358;
        _camera = Camera.main;
        Debug.Log(transform.rotation);
        Debug.Log(transform.eulerAngles);
        Debug.Log(transform.localEulerAngles);

    }
	
	// Update is called once per frame
	void Update ()
    {
        Debug.Log(_camera.transform.rotation);
        Debug.Log(_camera.transform.eulerAngles);
        Debug.Log(_camera.transform.localEulerAngles);
        //if (_camera.transform.rotation.y > minRotation && _camera.transform.rotation.y < maxRotation)
        if(_camera.transform.eulerAngles.y > minRotationCamera && _camera.transform.eulerAngles.y < maxRotationCamera)
        {
            transform.rotation = new Quaternion(transform.rotation.x, -_camera.transform.rotation.y, transform.rotation.z, transform.rotation.w);
        }
    }
}
