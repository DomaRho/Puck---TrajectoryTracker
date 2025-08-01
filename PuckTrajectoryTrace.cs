using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace DomaPuckTrajectoryTrace;

public class PuckTraceTrajectory : MonoBehaviour
{
    internal static Color shade = new Color(1.0f, 0.647f, 0.0f, 0.5f);
    internal static float puckdrag = 0.3f;
    internal static List<float> timeSequence;
    internal static Tuple<float, Vector3> endTrajectory;
    internal static Vector3 predictionPuckPos;
    internal static List<Vector3> predictedPuckPosList = new List<Vector3>();
    internal static LineRenderer lineRenderer;
    internal static GameObject trajectoryShadow;


    [HarmonyPatch(typeof(Puck), "Awake")]
    public static class PuckAwakePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Puck __instance)
        {
            lineRenderer = __instance.gameObject.AddComponent<LineRenderer>();

            lineRenderer.startWidth = 0.2f;
            lineRenderer.endWidth = 0.2f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Or another appropriate shader
            lineRenderer.material.SetColor("_Color", shade);
            lineRenderer.positionCount = 11;
            trajectoryShadow = Functions.CreateCircle();

            lineRenderer.enabled = false;
            trajectoryShadow.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(Puck), "FixedUpdate")]
    public static class PuckUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(Puck __instance)
        {
            lineRenderer = __instance.gameObject.GetComponent<LineRenderer>();

            Vector3 puckPosition = __instance.Rigidbody.transform.position;
            //Vector3 puckVelocity = __instance.SynchronizedObject.PredictedLinearVelocity;
            Vector3 puckVelocity = __instance.Rigidbody.linearVelocity; // I think Only Server Side could retrieve this? Seems to work in the offline practice as well.
            float targetY = 0.0f;

            if (puckPosition.y > 0.11f)
            {
                lineRenderer.enabled = true;
                trajectoryShadow.SetActive(true);
                

                endTrajectory = Functions.GetTimeForReachingYOnTheWayDown(puckPosition, puckVelocity, puckdrag, targetY);

                var time = endTrajectory.Item1;
                timeSequence = Functions.GenerateArithmeticSequence(time, 15);
                timeSequence.Sort();

                predictedPuckPosList.Add(puckPosition);

                for (int i = 0; i < 16; i++)
                {
                    predictionPuckPos = Functions.GetTrajectoryPosition(puckPosition, puckVelocity, puckdrag, timeSequence[i]);
                    predictedPuckPosList.Add(predictionPuckPos);
                }

                trajectoryShadow.transform.position = new Vector3(predictedPuckPosList[16].x, 0.01f, predictedPuckPosList[16].z);
                trajectoryShadow.transform.rotation = Quaternion.Euler(180f, 0.0f, 0.0f);

                Functions.UpdateLineRenderer(predictedPuckPosList);

                timeSequence.Clear();
                predictedPuckPosList.Clear();
            }
            else
            {
                lineRenderer.enabled = false;
                trajectoryShadow.SetActive(false);
            }
        }
    }
    
    
}   