using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using UnityEngine.UI;

public class SCR_PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    Camera cam;
    private Camera sceneCamera;

    
    #region DEBUG
    public Text debugText;
    #endregion
    #region Movement Variables
        [SerializeField]
        float speed;
        [SerializeField]
        float turnrate;

        private Canvas connectUI;

        [SerializeField]
        Behaviour[] compsToDisable;

        Vector3 transDisplacement;
        Vector3 rotatDisplacement;
        private float currentCamRotX;
        Rigidbody rb;

        
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb=GetComponent<Rigidbody>();
        connectUI=GameObject.Find("ConnectUI").GetComponent<Canvas>();
        connectUI.enabled=false;
        debugText=GameObject.Find("DebugText").GetComponent<Text>();
        if(!IsOwner)
        {
            for(int i=0;i<compsToDisable.Length;i++)
            {
                compsToDisable[i].enabled=false;
            }
        } 
        else
        {
            sceneCamera=Camera.main;
            if(sceneCamera!=null)
                sceneCamera.gameObject.SetActive(false);
        }
    }

    
    private void OnDisable()
    {
        if(sceneCamera!=null)
        {
            sceneCamera.gameObject.SetActive(true);
        }
        if(connectUI!=null)
        {
            connectUI.enabled=true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner)
        {
            return;
        }
        transDisplacement=(Input.GetAxisRaw("Horizontal")*transform.right+Input.GetAxisRaw("Vertical")*transform.forward).normalized *speed;
        rotatDisplacement=new Vector3(Input.GetAxisRaw("Mouse Y"),Input.GetAxisRaw("Mouse X"),0)*turnrate;
    }

    private void FixedUpdate()
    {
        if(transDisplacement!=Vector3.zero)
        {
            rb.MovePosition(rb.position + transDisplacement * Time.fixedDeltaTime);
        }
        if(rotatDisplacement.y!=0)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0,rotatDisplacement.y,0));
        }
        if(cam!=null && rotatDisplacement.x!=0)
        {
            //cam.transform.Rotate(-rotatDisplacement.x,0,0);
            currentCamRotX-=rotatDisplacement.x*turnrate;
            currentCamRotX=Mathf.Clamp(currentCamRotX,-85,85);
            cam.transform.localEulerAngles=new Vector3(currentCamRotX,0,0);


            debugText.text="Debug Text: "+cam.transform.localEulerAngles.x.ToString();
            
        }
    }
}
