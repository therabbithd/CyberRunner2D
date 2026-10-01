using UnityEngine;

public class CrashDetector : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        int layerIndex = LayerMask.NameToLayer("Floor");
        if (other.gameObject.layer == layerIndex)
        {
            Debug.Log("Player has crashed!");
            //TODO: You can add additional logic here, such as triggering a game over condition or restarting the level.
        }
    }
}