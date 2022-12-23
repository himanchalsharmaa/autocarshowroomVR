using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AroundCameraController : MonoBehaviour
{
    public bool autoCam = false;

    [Header("Target")]
    public Transform RespawnLocator;
    public float YOffset = 0.2f;

    private Transform targetTransform;


    [Header("Camera Properties")]

    public float distance = 5.0f;

    public float cameraSpeed = 2f;

    public float minY = -20f;
    public float maxY = 80f;

    public float minDistance = 0.5f;
    public float maxDistance = 15f;

    float x = 0.0f;
    float y = 0.0f;

    float hitDistance = 1.0f;


    Bounds targetBound;
    Rect curRect;



    Bounds GetWholeBounds(GameObject target)
    {
        Bounds wholeBounds = new Bounds(target.transform.position, Vector3.zero);


        Collider[] colliders = target.GetComponentsInChildren<Collider>();

        if (colliders.Length != 0)
        {
            foreach (Collider col in colliders)
            {
                wholeBounds.Encapsulate(col.bounds);
            }
        }
        else
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>();

            if (renderers.Length != 0)
            {
                wholeBounds.center = renderers[0].bounds.center;


                foreach (Renderer renderer in renderers)
                {
                    wholeBounds.Encapsulate(renderer.bounds);
                }
            }
        }

        return wholeBounds;
    }


    public void SetTurntableAnimation(System.Boolean value)
    {
        autoCam = (bool)value;
    }


    private void OnEnable()
    {
        Start();
    }


    private Transform GetTargetTransform()
    {
        Transform outTransform = null;

        if (RespawnLocator != null)
        {
            outTransform = RespawnLocator;
        }

        return outTransform;
    }


    void Start()
    {
        x = this.transform.eulerAngles.y;
        y = this.transform.eulerAngles.x;

        targetTransform = GetTargetTransform();

        if (targetTransform)
            this.transform.LookAt(targetTransform.position);

        if (targetTransform)
        {
            targetBound = GetWholeBounds(targetTransform.gameObject);

            Vector3 min = GetComponent<Camera>().WorldToScreenPoint(targetBound.min);
            Vector3 max = GetComponent<Camera>().WorldToScreenPoint(targetBound.max);

            int minScreenX = (int)((float)Screen.width * 0.1f);
            int minScreenY = (int)((float)Screen.height * 0.1f);

            curRect = new Rect(minScreenX, minScreenY, Screen.width-minScreenX, Screen.height-minScreenY);

        }

    }


    private void Update()
    {
        if (targetTransform == null)
        {
            targetTransform = GetTargetTransform();
        }

        if (targetTransform != null)
        {
            if (!autoCam)
            {
                if (Input.GetMouseButton(1))
                {
                    x += Input.GetAxis("Mouse X") * cameraSpeed;
                    y -= Input.GetAxis("Mouse Y") * cameraSpeed;
                }

                targetBound = GetWholeBounds(targetTransform.gameObject);

                Vector3 min = GetComponent<Camera>().WorldToScreenPoint(targetBound.min);
                Vector3 max = GetComponent<Camera>().WorldToScreenPoint(targetBound.max);

                float targetWidth = Mathf.Abs(max.x - min.x);
                float targetHeight = Mathf.Abs(max.y - min.y);

                float targetDistance = Vector3.Distance(min, max);

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 50 * (distance / 100), minDistance, maxDistance);
            }
        }
    }



    private void LateUpdate()
    {
        if (targetTransform != null)
        {
            if (autoCam)
            {
                this.transform.RotateAround(targetTransform.position, new Vector3(0f, 1f, 0f), 10 * Time.deltaTime * cameraSpeed);
            }
            else
            {
                y = ClampAngle(y, minY, maxY);

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                RaycastHit hit;

                if (Physics.Linecast(this.transform.position, targetTransform.position, out hit))
                {
                    hitDistance = hit.distance;
                }

                Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
                Vector3 position = rotation * negDistance + (targetTransform.position + new Vector3(0f, YOffset, 0f));

                this.transform.rotation = rotation;
                this.transform.position = position;
            }
        }
        else
        {
            targetTransform = GetTargetTransform();
        }
    }


    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
            angle += 360f;
        if (angle > 360f)
            angle -= 360f;

        return Mathf.Clamp(angle, min, max);
    }


}
