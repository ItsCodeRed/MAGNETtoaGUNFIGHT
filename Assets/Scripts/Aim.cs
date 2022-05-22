using System.Collections;
using UnityEngine;

public class Aim : MonoBehaviour
{
    public Vector2 aimPosition;
    Plane plane = new Plane(Vector3.up, 0);

    private void Update()
    {
        if (!GameManager.instance.player.canMove) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            aimPosition = new Vector2(point.x, point.z);
        }

        transform.LookAt(Utils.GameToWorldSpace(aimPosition) + Vector3.up * transform.position.y, Vector3.up);
    }
}