using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlanePilot : MonoBehaviour
{
    //Declarations
    PlayerControls controls;
    Vector2  move;    
    public float speed, maxSpeed, minspeed, rotSpeed1, rotSpeed2;
    
    public Text alt;
    public Text planeSpeed;
    public GameObject sp, start, run, stop, dive;
    private AudioSource startSound, runSound, stopSound, diveSound;
    
    private bool wheelsdown, enginerunning;
    private IEnumerator coroutine;    

    // Awake is called bfore start
    void Awake(){
        //plane controls fron Input.System
        controls = new PlayerControls();
    
        controls.Gameplay.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => move =Vector2.zero;
        this.transform.position = sp.transform.position;
        this.transform.rotation = sp.transform.rotation;

        controls.Gameplay.Wheels.performed += ctx => SetWheel();
        controls.Gameplay.StartEngine.performed += ctx => EngineRunning();
        controls.Gameplay.Reset.performed += ctx => ResetToSP();

        //bool initializations
        wheelsdown = true;
        enginerunning = false;
        if(wheelsdown == true){
            minspeed = 0;
        }
        //get sound components
        startSound = start.gameObject.GetComponent<AudioSource>();
        runSound = run.gameObject.GetComponent<AudioSource>();
        stopSound = stop.gameObject.GetComponent<AudioSource>();
        diveSound = dive.gameObject.GetComponent<AudioSource>();            
    }

    void SetWheel(){
        if(wheelsdown == true){
            wheelsdown = false;
            minspeed = 10;
        }else if(wheelsdown == false){
            wheelsdown = true;            
        }
        Debug.Log("wheels down is: " + wheelsdown);
        Debug.Log("min speed is: " + minspeed.ToString());
    }

    void EngineRunning(){
        if(enginerunning == true){
            KillEngine();
            Debug.Log("Engine was killed");                      
        }else if(enginerunning == false){
            StartProp();   
            Debug.Log("Engine was started");
        }        
    }

    private IEnumerator WaitAndPlay(float waitTime){
        while(true){
            yield return new WaitForSeconds(waitTime);
            runSound.Play();               
            startSound.Stop();                               
        }        
    }
    private void KillEngine(){
        enginerunning = false; 
        minspeed = 0;
        speed = 0;
        stopSound.time = 24.5f;        
        stopSound.Play();    
        runSound.Stop();  
        Debug.Log("engine running:" + enginerunning);
    }

    private void StartProp(){
        enginerunning = true;
        startSound.time = 15.5f;
        startSound.Play();            
        coroutine = WaitAndPlay(8.0f);
        StartCoroutine(coroutine);  
        Debug.Log("engine running:");
    }

    void ResetToSP(){
        this.transform.position = sp.transform.position;
        this.transform.rotation = sp.transform.rotation;
    }
    void OnEnable(){
        controls.Gameplay.Enable();
    }

    void OnDisable(){
        controls.Gameplay.Disable();
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        //this will allow you to climb, dive, and roll.
        Vector3 m = new Vector3(move.y, 0.0f, -move.x) * rotSpeed1 * Time.deltaTime;        
        transform.Rotate(m, Space.Self);
        //displays altitude
        alt.text = transform.position.y.ToString("f2");
        //constantly moves the plan forward unless on ground
        if(enginerunning == true && wheelsdown == false){
            transform.position += transform.forward * Time.deltaTime * speed;        
        }else if(enginerunning == true && wheelsdown == true){
            transform.position += transform.forward * Time.deltaTime * (speed)/4;
        }else{
            transform.position += transform.forward * minspeed;
        }
        //transform.position += transform.forward * Time.deltaTime * speed;            
        //lose speed as you climb
        speed -= transform.forward.y * Time.deltaTime * 50;        
        //prevents you from going slower than minspeed
        if(speed < minspeed){
            speed = minspeed;
        }
        //sets a max speed with gravity assist
        if(speed > 300){
            speed = 300.0f;
        }
        //displays speed
        planeSpeed.text = speed.ToString("f2");        
       
        //************Keyboard Controls**********************//////////////
        //speed up
        if(Input.GetKey(KeyCode.W) || Input.GetAxis("Accelerate") > 0.0f){
           if(speed < maxSpeed){
               speed++;
           }
        }
        //slow down
        if(Input.GetKey(KeyCode.S) || Input.GetAxis("Decelerate") > 0.0f){           
           if(wheelsdown && speed > minspeed){
               speed--;
           }
        }
        //allows the plane to roll left
        if(Input.GetKey(KeyCode.LeftArrow)){            
            transform.Rotate(Vector3.forward * rotSpeed1 * Time.deltaTime);            
        }
        //allows the plane to roll right
        if(Input.GetKey(KeyCode.RightArrow) ){            
            transform.Rotate(Vector3.back * rotSpeed1 * Time.deltaTime);            
        }
        //allows the plan to dive
        if(Input.GetKey(KeyCode.DownArrow)){            
            transform.Rotate(Vector3.left * rotSpeed2 * Time.deltaTime);            
        }       
        //allows the plane to climb
        if(Input.GetKey(KeyCode.UpArrow)){            
            transform.Rotate(Vector3.right * rotSpeed2 * Time.deltaTime);
        }        
        //allows the plane to shift left
        if(Input.GetKey(KeyCode.D) || Input.GetAxis("TurnRight") > 0.0f){
            transform.Rotate(Vector3.up * rotSpeed2 * Time.deltaTime);
        }
        //allows the plane to shift right
        if(Input.GetKey(KeyCode.A) || Input.GetAxis("TurnLeft") > 0.0f){
            transform.Rotate(Vector3.down * rotSpeed2 * Time.deltaTime);
        }
    }
}
