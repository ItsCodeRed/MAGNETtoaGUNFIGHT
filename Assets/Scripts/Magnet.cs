using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    [SerializeField] private LayerMask metalLayer;
    [SerializeField] private float spinSpeed = 0.3f;
    [SerializeField] private float spinRadius = 0.3f;
    [SerializeField] private float magnetRadius = 0.5f;
    [SerializeField] private float minPullDist = 0.1f;
    [SerializeField] private float magnetAngle = 30;
    [SerializeField] private float closeMagnetAngle = 45;
    [SerializeField] private float magnetDistance = 5f;
    [SerializeField] private float verticalPush = 1f;
    [SerializeField] private float shootStrength = 10;
    [SerializeField] private float pushStrength = 100;
    [SerializeField] private float pullStrength = 50;
    [SerializeField] private float dropTime = 0.3f;
    [SerializeField] private Transform attractPoint;
    [SerializeField] private Animation anim;
    public List<Rigidbody> grabbedBodies;
    [SerializeField] private int maxWeight = 3;
    [SerializeField] private int currentWeight = 0;

    [SerializeField] private MagnetState state;
    [SerializeField] private bool wasPulling = false;

    private float angle = 0f;
    private float dropTimer = 0f;

    private void Update()
    {
        HoldCurrentObjects();
        
        MagnetState newState = GetMagnetState();
        if (newState != state)
        {
            if (newState == MagnetState.Pushing && wasPulling)
            {
                anim.Play("BlueSpin");
                wasPulling = false;
            }
            if (newState == MagnetState.Pulling && !wasPulling)
            {
                anim.Play("RedSpin");
                wasPulling = true;
            }
        }
        state = newState;

        switch (state)
        {
            case MagnetState.Inactive:
                break;
            case MagnetState.Pushing:
                DropCurrentObjects();
                Push();
                break;
            case MagnetState.Pulling:
                if (currentWeight < maxWeight)
                    Pull();
                break;
        }
    }

    private void HoldCurrentObjects()
    {
        dropTimer += Time.deltaTime;
        angle += spinSpeed * Time.deltaTime;
        if (angle > Mathf.PI * 2)
        {
            angle = 0;
        }

        int weightSoFar = 0;
        for (int i = 0; i < grabbedBodies.Count; i++)
        {
            if (grabbedBodies[i] == null)
            {
                grabbedBodies.RemoveAt(i);
                i--;
                continue;
            }

            if (grabbedBodies.Count == 1)
            {
                grabbedBodies[i].transform.position = attractPoint.position;
                grabbedBodies[i].velocity = Vector3.zero;
            }
            else
            {
                Weapon weapon = grabbedBodies[i].transform.GetComponent<Weapon>();
                int weight = weapon != null ? weapon.weightValue : 1;
                weightSoFar += weight;
                float additionalAngle = 2 * Mathf.PI * weightSoFar / currentWeight;
                grabbedBodies[i].transform.position = attractPoint.position + new Vector3(Mathf.Cos(angle + additionalAngle), 0, Mathf.Sin(angle + additionalAngle)) * spinRadius;
                grabbedBodies[i].velocity = Vector3.zero;
            }
        }
    }


    private void DropCurrentObjects()
    {
        if (grabbedBodies.Count == 0 || dropTimer < dropTime) return;

        dropTimer = 0;

        if (grabbedBodies.Count == 1)
        {
            grabbedBodies.Clear();
            currentWeight = 0;
            return;
        }

        float throwScore = 0;
        Rigidbody throwBody = grabbedBodies[0];
        for (int i = 1; i < grabbedBodies.Count; i++)
        {
            if (grabbedBodies[i] == null)
            {
                grabbedBodies.RemoveAt(i);
                i--;
                continue;
            }

            float score = Vector3.Dot(grabbedBodies[i].position, attractPoint.position);
            if (Vector3.Dot(grabbedBodies[i].position - attractPoint.position, transform.parent.right) < 0 && score > throwScore)
            {
                throwScore = score;
                throwBody = grabbedBodies[i];
            }
        }

        Weapon weapon = throwBody.transform.GetComponent<Weapon>();
        currentWeight -= weapon != null ? weapon.weightValue : 1;

        Bullet bullet = throwBody.transform.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Shoot();
        }

        grabbedBodies.Remove(throwBody);
    }

    private MagnetState GetMagnetState()
    {
        bool isLeft = Input.GetMouseButton(0);
        bool isRight = Input.GetMouseButton(1);

        if (isLeft)
        {
            return MagnetState.Pushing;
        }
        if (isRight)
        {
            return MagnetState.Pulling;
        }

        return MagnetState.Inactive;
    }

    private void Push()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.parent.position, magnetRadius, transform.parent.forward, magnetDistance, metalLayer.value);

        for (int i = 0; i < hits.Length; i++)
        {
            float angle = Vector2.Angle(Utils.WorldToGameSpace(hits[i].collider.transform.position - transform.parent.position).normalized, Utils.WorldToGameSpace(transform.parent.forward).normalized);
            float dist = Vector3.Distance(hits[i].collider.transform.position, attractPoint.position);
            if ((angle > magnetAngle && dist >= minPullDist) || angle > closeMagnetAngle || grabbedBodies.Contains(hits[i].rigidbody))
                continue;

            Vector3 newVel = Utils.GameToWorldSpace(Utils.WorldToGameSpace(hits[i].collider.transform.position - transform.position).normalized * pushStrength / (1 + dist + angle / 6));
            if (dist < minPullDist)
            {
                Vector2 aimPosition = new Vector2(0, 0);
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitInfo))
                {
                    Vector3 point = hitInfo.point;
                    aimPosition = new Vector2(point.x, point.z);
                }
                Vector3 finalAimPoint = Utils.GameToWorldSpace(aimPosition) + Vector3.up * hits[i].rigidbody.position.y;
                Vector3 finalAimDirection = (finalAimPoint - hits[i].rigidbody.position).normalized;
                finalAimDirection = Vector3.Dot(finalAimDirection, transform.parent.forward) < 0 ? transform.parent.forward : finalAimDirection;
                hits[i].rigidbody.velocity = finalAimDirection * shootStrength + Vector3.up * (hits[i].rigidbody.useGravity ? verticalPush : 0);
            }
            else if (hits[i].rigidbody.velocity.magnitude < newVel.magnitude)
            {
                hits[i].rigidbody.velocity = Vector3.Lerp(hits[i].rigidbody.velocity, newVel, 2 * Time.deltaTime / Mathf.Max(Mathf.Pow(dist, 1.2f) / 12, 1));
            }
        }
    }

    private void Pull()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.parent.position, magnetRadius, transform.parent.forward, magnetDistance, metalLayer.value);

        for (int i = 0; i < hits.Length; i++)
        {
            float angle = Vector2.Angle(Utils.WorldToGameSpace(hits[i].collider.transform.position - transform.parent.position).normalized, Utils.WorldToGameSpace(transform.parent.forward).normalized);
            float dist = Vector2.Distance(Utils.WorldToGameSpace(hits[i].collider.transform.position), Utils.WorldToGameSpace(attractPoint.position));

            if ((angle > magnetAngle && dist >= minPullDist) || angle > closeMagnetAngle || grabbedBodies.Contains(hits[i].rigidbody))
                continue;

            Weapon weapon = hits[i].collider.transform.GetComponent<Weapon>();
            if (weapon != null)
            {
                weapon.isMagnitized = true;
                if (weapon.IsHeld)
                    continue;
            }

            int weight = weapon != null ? weapon.weightValue : 1;
            if (currentWeight + weight > maxWeight)
            {
                continue;
            }

            if (dist < minPullDist)
            {
                currentWeight += weight;
                hits[i].collider.transform.position = attractPoint.position;
                hits[i].rigidbody.velocity = Vector3.zero;
                grabbedBodies.Add(hits[i].rigidbody);
                return;
            }

            Vector3 newVel = (attractPoint.position - hits[i].collider.transform.position).normalized * pullStrength / (1 + dist + angle / 10);

            hits[i].rigidbody.velocity = Vector3.Lerp(hits[i].rigidbody.velocity, newVel, 2 * Time.deltaTime / Mathf.Max(dist / 10, 1));
        }
    }
}

public enum MagnetState
{
    Inactive = 0,
    Pushing,
    Pulling,
}
