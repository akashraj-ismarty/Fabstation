using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AngleCalculation : MonoBehaviour
{
    public LayerMask modelLayer; // Layer for the 3D model
    public LineRenderer lineRenderer;
    public GameObject arcPrefab; // Prefab for the arc
    public TextMeshPro angleText; // Text to display the angle
    public GameObject pointPrefab; // Prefab for the points
    public Material pointMaterial;
    public Material lineMaterial;
    public Material arcMaterial;

    private List<Vector3> points = new List<Vector3>();
    private List<GameObject> pointObjects = new List<GameObject>();
    private GameObject currentArc;

    private void Start()
    {
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, modelLayer))
            {
                AddPoint(hit.point);
                // Debug.Log("Inside Raycast" + hit.point.ToString());

            }
        }

        if (points.Count == 3)
        {
            CalculateAndDisplayAngle();
        }
    }

    void AddPoint(Vector3 point)
    {
        if (points.Count < 3)
        {
            points.Add(point);
            GameObject pointObj = Instantiate(pointPrefab, point, Quaternion.identity, transform);
            pointObj.GetComponent<Renderer>().material = pointMaterial;
            pointObjects.Add(pointObj);
        }
        else
        {
            // Reset if we already have 3 points
            ResetPoints();
            points.Add(point);
            GameObject pointObj = Instantiate(pointPrefab, point, Quaternion.identity, transform);
            pointObj.GetComponent<Renderer>().material = pointMaterial;
            pointObjects.Add(pointObj);
        }
    }

    void CalculateAndDisplayAngle()
    {
        Vector3 a = points[0];
        Vector3 b = points[1];
        Vector3 c = points[2];

        // Calculate vectors from B to A and B to C
        Vector3 ba = (a - b).normalized;
        Vector3 bc = (c - b).normalized;

        // Calculate the angle between the vectors
        float angle = Vector3.Angle(ba, bc);

        // Display the angle
        angleText.text = angle.ToString("F1") + "°";

        // Draw lines
        lineRenderer.positionCount = 3;
        lineRenderer.SetPositions(new Vector3[] { a, b, c });

        // Draw arc
        DrawArc(b, ba, bc, angle);
    }

    void DrawArc(Vector3 center, Vector3 startDirection, Vector3 endDirection, float angle)
    {
        if (currentArc != null)
        {
            Destroy(currentArc);
        }

        currentArc = Instantiate(arcPrefab, center, Quaternion.identity, transform);

        currentArc.GetComponent<Renderer>().material = arcMaterial;
        LineRenderer arcLineRenderer = currentArc.GetComponent<LineRenderer>();
        arcLineRenderer.material = arcMaterial;
        arcLineRenderer.startWidth = 0.02f;
        arcLineRenderer.endWidth = 0.02f;

        int segments = 50;
        arcLineRenderer.positionCount = segments + 1;
        Vector3[] arcPoints = new Vector3[segments + 1];

        // Calculate the rotation axis
        Vector3 rotationAxis = Vector3.Cross(startDirection, endDirection).normalized;
        if (rotationAxis == Vector3.zero)
        {
            rotationAxis = Vector3.up;
        }

        // Calculate the radius of the arc
        float radius = 0.2f; // Adjust as needed

        // Calculate the arc points
        for (int i = 0; i <= segments; i++)
        {
            float angleStep = angle / segments;
            Quaternion rotation = Quaternion.AngleAxis(angleStep * i, rotationAxis);
            Vector3 point = rotation * startDirection * radius;
            arcPoints[i] = center + point;
        }

        arcLineRenderer.SetPositions(arcPoints);

        // Position the angle text
        // Calculate the midpoint of the arc
        Vector3 midDirection = (startDirection + endDirection).normalized;
        Vector3 textPosition = center + midDirection * 0.4f; // 0.4 is the radius from the intersection point

        // **Uplift the text:**
        Vector3 upliftDirection = Vector3.up; // You can change this to a different direction if needed
        float upliftAmount = 0.1f; // Adjust this value to control how much the text is uplifted
        textPosition += upliftDirection * upliftAmount;
        
        angleText.transform.position = textPosition;

        // Make the text always face the camera, but freeze rotation on Y-axis
        Vector3 lookDirection = Camera.main.transform.position - angleText.transform.position;
        lookDirection.x = 0; // Freeze X-axis rotation
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        angleText.transform.rotation = lookRotation;
        
        // Ensure text is upright
        angleText.transform.rotation *= Quaternion.Euler(0, 180, 0); // Rotate 180 degrees around the Y-axis to make it readable
    
    }

    void ResetPoints()
    {
        points.Clear();
        foreach (GameObject obj in pointObjects)
        {
            Destroy(obj);
        }
        pointObjects.Clear();
        lineRenderer.positionCount = 0;
        if (currentArc != null)
        {
            Destroy(currentArc);
        }
        angleText.text = "";
    }
}
