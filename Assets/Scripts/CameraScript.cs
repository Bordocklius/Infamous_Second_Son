using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform _target;
    public Transform _cameraTransform;

    private Vector3 _offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        _offset = _cameraTransform.position - _target.position;
        _cameraTransform.position = _target.position + _offset;
        //_cameraTransform.LookAt(_target.position);
        Debug.Log(_cameraTransform.position);
        //Debug.Log(_target.position);
    }
}
