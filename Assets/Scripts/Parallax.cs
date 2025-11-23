using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] Transform cam;
    [SerializeField] float parallaxEffect, width;
    float startPos;

    private void Start()
    {
        startPos = transform.position.x;
    }

    private void FixedUpdate()
    {
        if (parallaxEffect == 0) return;

        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);
        transform.position = new(startPos + distance, transform.position.y, transform.position.z);

        // If background has reached the end of its width then move it for infinite scrolling
        if (movement > startPos + width) { startPos += width; }
        else if (movement < startPos - width) { startPos -= width; }
    }
}