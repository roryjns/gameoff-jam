using UnityEngine;

public class DoorLight : MonoBehaviour
{
    public void Start()
    {
        Toggle(false);
    }
    bool IsOn = false;
    public void Toggle(bool? force)
    {
        IsOn = force.GetValueOrDefault(!IsOn);

        transform.GetChild(0).gameObject.SetActive(IsOn);
    }
}
