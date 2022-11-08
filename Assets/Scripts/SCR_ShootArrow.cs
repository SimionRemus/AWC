using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SCR_ShootArrow : NetworkBehaviour
{
    [SerializeField] private SCR_Arrow prefab;

    private SCR_Arrow currentArrow;

    private float reloadTime;

    [SerializeField] private Transform arrowSpawnPoint;

    private bool isReloading;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    private void Start()
    {
        reloadTime=2f;
        currentArrow=Instantiate(original: prefab,arrowSpawnPoint.position,arrowSpawnPoint.rotation);
        currentArrow.transform.SetParent(transform.GetChild(1));
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {
        if(!IsOwner)
        {
            return;
        }
        if(Input.GetMouseButtonDown(0))
        {
            Fire(15f);
        }
    }

    public void Reload()
    {
        if(isReloading||currentArrow!=null) return;
        isReloading=true;
        StartCoroutine(ReloadAfterTime());

    }

    private IEnumerator ReloadAfterTime()
    {
        yield return new WaitForSeconds(reloadTime);
        currentArrow=Instantiate(prefab,arrowSpawnPoint.position,arrowSpawnPoint.rotation);
        currentArrow.transform.SetParent(transform.GetChild(1));
        isReloading=false;
    }

    public void Fire(float firepower)
    {
        if(isReloading||currentArrow==null) return;
        var force=arrowSpawnPoint.TransformDirection(Vector3.forward * firepower);
        currentArrow.transform.SetParent(null);
        currentArrow.Fly(force);
        currentArrow=null;
        Reload();
    }

}
