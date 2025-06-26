using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    // Holds the currently interactable object
    private IInteractable interactableInRange = null;
    
    // Tracks all valid interactables that are within trigger range
    private List<IInteractable> interactablesInRange = new List<IInteractable>();

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactableInRange?.Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Make sure the collision reference is good and that it has an interactable component
        if (collision != null && collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            // Only add if not already in the list
            if (!interactablesInRange.Contains(interactable))
            {
                interactablesInRange.Add(interactable);
            }

            // If there isn’t a current interactable set, update it
            if (interactableInRange == null)
            {
                UpdateInteractableInRange();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check for a valid collision and an associated interactable
        if (collision != null && collision.TryGetComponent(out IInteractable interactable))
        {
            // Remove from our tracker if present
            if (interactablesInRange.Contains(interactable))
            {
                interactablesInRange.Remove(interactable);
            }

            // If the leaving object was the current interactable, update the current interactable
            if (interactable == interactableInRange)
            {
                UpdateInteractableInRange();
            }
        }
    }

    /// <summary>
    /// Update the current interactable based on the available ones.
    /// This method selects the first available interactable that can still interact.
    /// </summary>
    private void UpdateInteractableInRange()
    {
        // Use LINQ to find the first interactable still available for interaction.
        interactableInRange = interactablesInRange.FirstOrDefault(x => x.CanInteract());
    }
}
