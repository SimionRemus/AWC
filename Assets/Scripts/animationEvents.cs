using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationEvents : MonoBehaviour
{
    [SerializeField] private GameObject playerGO;
    private SCR_ShootArrow shootArrowScript;

    void Start()
    {
        shootArrowScript = playerGO.GetComponent<SCR_ShootArrow>();
    }


    public void NewArrow()
    {
        
        if (!shootArrowScript.isReloading)
        {
            
            shootArrowScript.InstantiateNewArrow();
        }
    }
}
