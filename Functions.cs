using System;
using System.Collections.Generic;
using UnityEngine;

namespace DomaPuckTrajectoryTrace;

public class Functions
{
    public static int segments = 32;
    public static float radius = .5f;
    public static Material material;
    public static GameObject circleObject;


    // Functions Section
    public static List<float> GenerateArithmeticSequence(float time, int linePoints)
    {
        List<float> sequence = new List<float>();
        float currentValue = 0;

        for (int i = 0; i <= linePoints; i++)
        {
            sequence.Add(currentValue);
            currentValue += time / linePoints;
        }

        return sequence;
    }

    // Section 2: Logic for Trajectory Predictions for Shot Logic

    public static Vector3 GetTrajectoryPosition(Vector3 startPosition, Vector3 startVelocity, float drag, float time)
    {
        Vector3 acceleration = Physics.gravity;
        Vector3 position = startPosition;
        float puckdrag = drag;
        float currentTime = time;

        float frameDrag = 1.0f - puckdrag * Time.fixedDeltaTime;
        float frames = time / Time.fixedDeltaTime;

        float frameDragLog = (float)Math.Log(frameDrag);
        float frameDragPower = Mathf.Pow(frameDrag, frames);

        position += startVelocity * ((frameDragPower - 1.0f) / frameDragLog) * Time.fixedDeltaTime;

        float accDragFactor = puckdrag * Time.fixedDeltaTime - 1;
        accDragFactor *= frameDragPower - frames * frameDragLog - 1.0f;
        accDragFactor /= puckdrag * frameDragLog;

        position += acceleration * accDragFactor * Time.fixedDeltaTime;

        return position;
    }

    public static Tuple<float, Vector3> GetTimeForReachingYOnTheWayDown(Vector3 startPosition, Vector3 startVelocity, float drag, float targetY,
        float startTime = 0.1f, int iterations = 32, float epsilon = 0.01f)
    {
        float time = startTime;
        Vector3 tangent = GetTangent(startVelocity, drag, time);

        for (int i = 0; i < iterations; i++)
        {
            //On the way up
            if (tangent.y >= 0.0f)
            {
                time *= 2.0f;
                tangent = GetTangent(startVelocity, drag, time);
            }
            else
            {
                //Using Newton's method
                var position = GetTrajectoryPosition(startPosition, startVelocity, drag, time);
                tangent = GetTangent(startVelocity, drag, time);

                if (Mathf.Abs(position.y - targetY) < epsilon)
                {
                    var results = new Tuple<float, Vector3>(time, position);
                    return results;
                }

                time = time - ((position.y - targetY) / tangent.y);
            }
        }

        var positiontrue = GetTrajectoryPosition(startPosition, startVelocity, drag, time);
        var resultsdown = new Tuple<float, Vector3>(time, positiontrue);
        return resultsdown;
    }
    public static Vector3 GetTangent(Vector3 startVelocity, float drag, float time)
    {
        float frameDrag = 1.0f - drag * Time.fixedDeltaTime;
        float frames = time / Time.fixedDeltaTime;

        float frameDragPower = Mathf.Pow(frameDrag, frames);

        Vector3 tangent = startVelocity * frameDragPower;

        Vector3 accTangent = (Physics.gravity / drag) * (drag * Time.fixedDeltaTime * frameDragPower - frameDragPower + 1.0f);
        accTangent -= Physics.gravity * Time.fixedDeltaTime;

        return (tangent + accTangent);
    }

    internal static void UpdateLineRenderer(List<Vector3> predictedPuckPosList)
    {
        //Set the position count
        PuckTraceTrajectory.lineRenderer.positionCount = predictedPuckPosList.Count;

        //Convert the list to an array and update the line renderer
        PuckTraceTrajectory.lineRenderer.SetPositions(predictedPuckPosList.ToArray());
    }
    // Section 2: Logic for Trajectory Predictions for Shot Logic --- END

    // Section 3: Visualise the Trajectory Predictions Location

    public static GameObject CreateCircle()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();

        // Add center vertex
        vertices.Add(Vector3.zero);
        uv.Add(new Vector2(0.5f, 0.5f));

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * 2 * Mathf.PI;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            vertices.Add(new Vector3(x, 0.01f, z));
            uv.Add(new Vector2((x / radius + 1) * 0.5f, (z / radius + 1) * 0.5f));
        }

        for (int i = 1; i <= segments; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i % segments + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        circleObject = new GameObject("CircularPlane");
        MeshFilter meshFilter = circleObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = circleObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter.mesh = mesh;
        meshRenderer.material = new Material(Shader.Find("Sprites/Default")); // Or another appropriate shader
        meshRenderer.material.SetColor("_Color", new Color(1.0f, 0.647f, 0.0f, 0.5f));

        return circleObject;
    }

    // Section 3: Visualise the Trajectory Predictions Location --- END
}