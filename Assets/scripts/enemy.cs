using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemy : MonoBehaviour
{
    private bool encounterResolved;
    private Collider2D encounterTrigger;
    private bool pollForPlayerContacts;
    private ContactFilter2D contactFilter;
    private readonly List<Collider2D> overlapResults = new List<Collider2D>(8);

    private void Awake()
    {
        foreach (Collider2D ownCollider in GetComponents<Collider2D>())
        {
            if (ownCollider.isTrigger)
            {
                encounterTrigger = ownCollider;
                break;
            }
        }

        // Without a Rigidbody2D on this GameObject, Unity sends compound-collider
        // trigger callbacks to the Rigidbody2D owner (the parent branch), not here.
        // Keep the caterpillar prefab free of an extra body and query its trigger instead.
        pollForPlayerContacts = GetComponent<Rigidbody2D>() == null;
        contactFilter = new ContactFilter2D();
        contactFilter.NoFilter();
    }

    private void FixedUpdate()
    {
        if (encounterResolved || !pollForPlayerContacts || encounterTrigger == null)
            return;

        overlapResults.Clear();
        // Query both colliders at their physics poses. Transforms may be ahead of Physics2D.
        encounterTrigger.Overlap(contactFilter, overlapResults);
        foreach (Collider2D other in overlapResults)
        {
            if (TryResolveEncounter(other))
                return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryResolveEncounter(other);
    }

    private bool TryResolveEncounter(Collider2D other)
    {
        if (encounterResolved || other == null || !other.CompareTag("Player"))
            return false;

        Debug.Log("who touch me " + other.tag);
        encounterResolved = true;

        FlowerProtection protection = other.GetComponentInParent<FlowerProtection>();
        if (protection != null && protection.TryConsumeAndTransform(this))
        {
            Debug.Log("Caterpillar hit was absorbed and transformed into a butterfly.");
            return true;
        }

        Debug.Log("change enabled");
        EventControllerScr.Instance.PlayerLose();
        return true;
    }
}
