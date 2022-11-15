using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Redirection;

public class RightOnlyRedirector : Redirector
{
    //variables copied from STeerToRedirector
    // Testing Parameters
    bool useBearingThresholdBasedRotationDampeningTimofey = true;
    bool dontUseDampening = false;

    // User Experience Improvement Parameters
    private const float MOVEMENT_THRESHOLD = 0.2f; // meters per second
    private const float ROTATION_THRESHOLD = 1.5f; // degrees per second
    private const float CURVATURE_GAIN_CAP_DEGREES_PER_SECOND = 15;  // degrees per second
    private const float ROTATION_GAIN_CAP_DEGREES_PER_SECOND = 30;  // degrees per second
    private const float DISTANCE_THRESHOLD_FOR_DAMPENING = 1.25f; // Distance threshold to apply dampening (meters)
    private const float BEARING_THRESHOLD_FOR_DAMPENING = 45f; // TIMOFEY: 45.0f; // Bearing threshold to apply dampening (degrees) MAHDI: WHERE DID THIS VALUE COME FROM?
    private const float SMOOTHING_FACTOR = 0.125f; // Smoothing factor for redirection rotations

    // Reference Parameters
    protected Transform currentTarget; //Where the participant  is currently directed?
    protected GameObject tmpTarget;

    // State Parameters
    protected bool noTmpTarget = true;

    // Auxiliary Parameters
    private float rotationFromCurvatureGain; //Proposed curvature gain based on user speed
    private float rotationFromRotationGain; //Proposed rotation gain based on head's yaw
    private float lastRotationApplied = 0f;


    public override void InjectRedirection()
    {

        // Get Required Data
        Vector3 deltaPos = redirectionManager.deltaPos;
        float deltaDir = redirectionManager.deltaDir;

        rotationFromCurvatureGain = 0;

        // Prevent having gains if user is stationary
        Vector3 desiredFacingDirection = Utilities.FlattenedPos3D(redirectionManager.targetWaypoint.position) - redirectionManager.currPos;
        int desiredSteeringDirection = (-1) * (int)Mathf.Sign(Utilities.GetSignedAngle(redirectionManager.currDir, desiredFacingDirection));
        float rotationProposed = desiredSteeringDirection * Mathf.Max(rotationFromRotationGain, rotationFromCurvatureGain);
        //if (Mathf.Approximately(rotationProposed, 0))
        //   return;

        if (deltaPos.magnitude / redirectionManager.GetDeltaTime() > MOVEMENT_THRESHOLD) //User is moving
        {
            InjectRotation(0.2f);
        }

        //Compute desired facing vector for redirection 
        desiredFacingDirection = Vector3.right;
        desiredSteeringDirection = (-1) * (int)Mathf.Sign(Utilities.GetSignedAngle(redirectionManager.currDir, desiredFacingDirection)); // We have to steer to the opposite direction so when the user counters this steering, she steers in right direction

        //Compute proposed rotation gain
        rotationFromRotationGain = 0;

        if (Mathf.Abs(deltaDir) / redirectionManager.GetDeltaTime() >= ROTATION_THRESHOLD)  //if User is rotating
        {
            rotationFromRotationGain = Mathf.Min(Mathf.Abs(deltaDir * globalConfiguration.MIN_ROT_GAIN), ROTATION_GAIN_CAP_DEGREES_PER_SECOND * redirectionManager.GetDeltaTime());
        }

        rotationProposed = desiredSteeringDirection * Mathf.Max(rotationFromRotationGain, rotationFromCurvatureGain);
        //curvatureGainUsed = rotationFromCurvatureGain > rotationFromRotationGain;


        // Prevent having gains if user is stationary
        if (Mathf.Approximately(rotationProposed, 0))
            return;

        // Implement additional rotation with smoothing
        float finalRotation = (1.0f - SMOOTHING_FACTOR) * lastRotationApplied + SMOOTHING_FACTOR * rotationProposed;
        lastRotationApplied = finalRotation;

        InjectRotation(finalRotation);
    }
}
