using UnityEngine;

/// <summary>
/// Class for handling the gun in the ufo game.
/// </summary>
public class Gun : MonoBehaviour
{

    public float damage = 10f;
    public float range = 100f;
    public Camera fpsCam; // Reference to the camera for raycasting

    /// <summary>
    ///  Method to handle shooting action.
    /// </summary>
    public void Shoot()
    {
        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            Debug.Log(hit.transform.name);
            Target target = hit.transform.GetComponent<Target>(); // Call the Die method on the Target component if it exists
            if (target != null)
            {
                target.Die(); // Call the Die method on the Target component
            }
        }
    }
}
