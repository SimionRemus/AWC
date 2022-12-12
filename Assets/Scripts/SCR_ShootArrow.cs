using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SCR_ShootArrow : NetworkBehaviour
{
    [SerializeField] private SCR_Arrow prefab;
    [SerializeField] private Transform lookDirection;

    private SCR_Arrow currentArrow;

    private float reloadTime;

    [SerializeField] private Transform arrowSpawnPoint;

    private bool isReloading;

    private void Start()
    {
        reloadTime=2f;
        currentArrow=Instantiate(original: prefab,arrowSpawnPoint.position,arrowSpawnPoint.rotation);
        currentArrow.transform.SetParent(arrowSpawnPoint);
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
        if(Input.GetMouseButtonUp(0))
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
        currentArrow.transform.SetParent(arrowSpawnPoint);
        isReloading=false;
    }

    public void Fire(float firepower)
    {
        if(isReloading||currentArrow==null) return;
        currentArrow.transform.rotation = lookDirection.rotation;
        var force=lookDirection.TransformDirection(Vector3.forward * firepower);
        currentArrow.transform.SetParent(null);
        currentArrow.Fly(force);
        currentArrow=null;
        Reload();
    }

}
