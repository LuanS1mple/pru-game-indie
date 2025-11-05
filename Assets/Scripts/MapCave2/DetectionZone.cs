using UnityEngine;
using Pathfinding;
using System.Collections;

public class DetectionZone : MonoBehaviour
{
    public AIDestinationSetter aiDestinationSetter;
    public AIPath aiPath;
    public Transform player;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (aiDestinationSetter != null)
            aiDestinationSetter.target = null;

        if (aiPath != null)
            aiPath.canMove = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(EnableChase());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            aiDestinationSetter.target = null;
            aiPath.canMove = false;
        }
    }

    private IEnumerator EnableChase()
    {
        yield return new WaitForEndOfFrame();
        aiDestinationSetter.target = player;
        aiPath.canMove = true;
        Debug.Log("Chase enabled!");
    }
}
