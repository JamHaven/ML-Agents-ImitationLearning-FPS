using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    private List<Pickup> pickupList;
    // Start is called before the first frame update
    void Start()
    {
        pickupList = new List<Pickup>();
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(pickupList.Count);
    }

    public void RegisterPickup(Pickup pickupToRegister)
    {
        //Debug.Log("Item registered");
        pickupList.Add(pickupToRegister);
    }

    public void RemovePickup(Pickup pickupToRemove)
    {
        //Debug.Log("Item removed");
        pickupList.Remove(pickupToRemove);
    }

    public void ClearAllPickups()
    {
        //Debug.Log("Item List cleared!");
        foreach (var pickupItem in pickupList)
        {
            Destroy(pickupItem.gameObject);
        }
        pickupList.Clear();
    }
}