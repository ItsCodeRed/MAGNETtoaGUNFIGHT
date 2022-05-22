using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PointParticle : MonoBehaviour
{
    [SerializeField] private float riseSpeed = 4f;
    [SerializeField] private float timeLiving = 2f;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private TMP_Text text;

    private float livingTimer = 0f;

    void Update()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);
        livingTimer += Time.deltaTime;

        transform.Translate(0, riseSpeed * Time.deltaTime, 0);

        text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Min((timeLiving - livingTimer) * fadeSpeed, 1));

        if (livingTimer > timeLiving)
        {
            Destroy(gameObject);
        }
    }

    public void SetScoreValue(int value)
    {
        text.text = "+" + value;
    }
}
