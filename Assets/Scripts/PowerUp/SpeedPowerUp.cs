using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPowerUp : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        try{
            var targetRigidbody = other.GetComponent<Rigidbody>();
            if (targetRigidbody.CompareTag("Player")){
                var targetHealth = targetRigidbody.GetComponent<TankMovement>();
                targetHealth.SpeedUp();
                Destroy(gameObject);
            }
        }
        catch{
        }
            
    }

}
