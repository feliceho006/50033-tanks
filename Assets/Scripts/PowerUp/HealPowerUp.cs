using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPowerUp : MonoBehaviour
{

    private BoxCollider healCollider;

    public void Start(){
    }


    private void OnTriggerEnter(Collider other)
    {
        try{
            var targetRigidbody = other.GetComponent<Rigidbody>();
            if (targetRigidbody.CompareTag("Player")){
                var targetHealth = targetRigidbody.GetComponent<TankHealth>();
                targetHealth.Heal();
                Destroy(gameObject);
            }
        }
        catch{
        }
        
        
            
    }
}

