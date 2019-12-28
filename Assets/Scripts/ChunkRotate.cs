using UnityEngine;
using UnityEngine.EventSystems;

public class ChunkRotate : MonoBehaviour
{
    public float speed = 1f;
    
    public bool autoRotate = true;
    private int autoRotateDir = 1;

    public bool controlRotation = true;
    public float controllRotationSpeed = 2f;
    public float rotationSmooth = 2f;

    private Vector3 lastMousePosition;
    private float lastMx;
    private float targetRotation;
    private float lastTargetRotation;

    private void Start()
    {
        targetRotation = transform.rotation.eulerAngles.y;
    }

    private void Update()
    {
        
        if(controlRotation)
        {
            if(!EventSystem.current.IsPointerOverGameObject() && EventSystem.current.currentSelectedGameObject == null)
            {
                if(Input.GetMouseButtonDown(0))
                {
                    lastTargetRotation = targetRotation;
                    lastMousePosition = Input.mousePosition;
                    lastMx = lastMousePosition.x;
                }

                if(Input.GetMouseButton(0))
                {
                    var mouseDir = (Input.mousePosition - lastMousePosition).normalized;
                    lastMousePosition = Input.mousePosition;
                    targetRotation += mouseDir.x * controllRotationSpeed;
                    // transform.Rotate(Vector3.up * mouseDir.x * speed * Time.deltaTime);
                    autoRotateDir = targetRotation < lastTargetRotation ? -1 : 1;
                }   
            }
        }

        if(autoRotate)
        {
            targetRotation += speed * autoRotateDir * Time.deltaTime;
            // transform.Rotate(Vector3.up * speed * autoRotateDir * Time.deltaTime, Space.World);
        }

        var euleurAngles = transform.rotation.eulerAngles;
        euleurAngles.y = targetRotation;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(euleurAngles), rotationSmooth * Time.deltaTime);
    }
    

    public void SetAutoRotate(bool val)
    {
        this.autoRotate = val;
    }

    public void SetControlRotate(bool val)
    {
        this.controlRotation = val;
    }

    public void SetHeight(float y)
    {
        var pos = transform.position;
        pos.y = y / 2f;
        transform.position = pos;
    }
}