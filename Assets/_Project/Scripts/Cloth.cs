using System.Collections.Generic;
using UnityEngine;

public class Cloth : MonoBehaviour
{
    [Header("Cloth Settings")] public int pointCount = 6; // Number of points in the cloth
    public float maxDistance = 1f; // Maximum distance between points
    public Vector2 gravity = new Vector2(0, -9.8f); // Gravity affecting the points
    public float damping = 0.99f; // Velocity damping to simulate air resistance
    public Vector2 baseOffset = new Vector2(-0.5f, -2.0f); // Default initial offset for new points
    public Vector2 windOffset = Vector2.zero; // Environmental wind effect
    public float pointSize = 0.2f; // Size of the circles at the start
    public float stiffness = 0.5f; // Cloth stiffness (0: loose, 1: rigid)

    private List<Vector2> points = new List<Vector2>(); // Current positions of points
    private List<Vector2> prevPoints = new List<Vector2>(); // Previous positions for velocity calculation

    void Start()
    {
        // Initialize points and velocities
        Vector2 anchorPosition = transform.position;
        points.Add(anchorPosition); // The anchor point (fixed)

        for (int i = 1; i < pointCount; i++)
        {
            Vector2 initialPos = anchorPosition + baseOffset * i;
            points.Add(initialPos); // Initialize positions
            prevPoints.Add(initialPos); // Set initial velocities to 0
        }

        prevPoints.Insert(0, anchorPosition); // The anchor point has no velocity
    }

    void Update()
    {
        ApplyForces(); // Apply gravity and wind
        ConstrainPoints(); // Enforce distance constraints
    }

    void ApplyForces()
    {
        for (int i = 1; i < points.Count; i++) // Skip the anchor point (0)
        {
            // Calculate velocity (current position - previous position)
            Vector2 velocity = (points[i] - prevPoints[i]) * damping;

            // Apply gravity, wind, and other forces
            Vector2 acceleration = gravity + windOffset;

            // Update position using Verlet integration
            prevPoints[i] = points[i]; // Store current position
            points[i] += velocity + acceleration * Time.deltaTime * Time.deltaTime; // Update position
        }
    }

    void ConstrainPoints()
    {
        // Anchor the first point to the object's position
        points[0] = transform.position;

        // Apply constraints between points
        for (int iteration = 0; iteration < 3; iteration++) // Iterating increases stability
        {
            for (int i = 1; i < points.Count; i++)
            {
                Vector2 direction = points[i] - points[i - 1];
                float distance = direction.magnitude;

                if (distance > maxDistance)
                {
                    float difference = distance - maxDistance;
                    direction.Normalize();

                    // Distribute the correction between points based on stiffness
                    Vector2 correction = direction * difference * stiffness;

                    if (i == 1) // If it's the first point after the anchor, only move the current point
                    {
                        points[i] -= correction;
                    }
                    else
                    {
                        points[i] -= correction * 0.5f;
                        points[i - 1] += correction * 0.5f;
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // Visualize the points as circles and lines
        if (points == null || points.Count == 0) return;

        Gizmos.color = Color.white;

        for (int i = 0; i < points.Count; i++)
        {
            float size = pointSize * (1f - (float)i / points.Count); // Reduce size for further points
            Gizmos.DrawSphere(points[i], size);

            if (i > 0)
            {
                Gizmos.DrawLine(points[i - 1], points[i]); // Draw line between points
            }
        }
    }
}