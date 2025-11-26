using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour
{
    [SerializeField] GameObject emptySegment;
    [SerializeField] Sprite filledSegment;
    readonly List<Image> segments = new();

    public void Initialise(int maxHealth)
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        segments.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            var segObj = Instantiate(emptySegment, transform);
            var img = segObj.GetComponent<Image>();
            segments.Add(img);
        }
    }

    public void UpdateHealth(int currentHealth)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (i < currentHealth) segments[i].sprite = filledSegment;
        }
    }
}