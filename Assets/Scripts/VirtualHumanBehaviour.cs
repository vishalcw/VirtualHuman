using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VirtualHumanBehaviour : MonoBehaviour
{   
    private Animator VHAnimator;
    private AudioSource VHAudio;

    [SerializeField] private bool isAnimating = false;

    private GameObject BodyViewRef;
    public Vector3 R_JointRefVector;

    [SerializeField] private Dictionary<string, AnimationClip> AnimClips;

    public AudioClip[] AudioClips;


    public Vector2 L_Ang;
    public Vector2 L2_Ang;

    public Vector2 R_Ang;
    public Vector2 R2_Ang;





    public static VirtualHumanBehaviour Instance;


    void Awake()
    {
        if(Instance == null) Instance=this;
    }


     void Start()
    {   
        BodyViewRef = GameObject.FindGameObjectWithTag("BodyView");
        VHAnimator = GetComponent<Animator>();
        VHAudio = GetComponent<AudioSource>();
    }

    void Update()
    {
        if(!isAnimating)
        {
            CheckForRShake();
            CheckForRHello();
        
        
        }
    }
   
   private void CheckForRShake()
   {
       if( R_Ang.x >= 25f && R_Ang.x <= 65f && R_Ang.y >= 25f && R_Ang.y <= 65f)
       {
           if( R2_Ang.x >= -65f && R2_Ang.x <= -25f && R2_Ang.y >= 70f && R2_Ang.y <= 110f)
           {
               StartCoroutine(VAnimate("shake", 0));
           }
       }
   }

   private void CheckForRHello()
   {
       if( R_Ang.x >= -155f && R_Ang.x <= -115f && R_Ang.y >= 70f && R_Ang.y <= 110f)
       {
           if( R2_Ang.x >= -65f && R2_Ang.x <= -25f && R2_Ang.y >= 70f && R2_Ang.y <= 110f)
           {
               StartCoroutine(VAnimate("hello", 0));
           }
       }
   }



    private IEnumerator VAnimate(string _state, int _astate)
    {
        isAnimating = true;

        VHAnimator.SetBool(_state, true);
        VHAudio.PlayOneShot( AudioClips[_astate] );
        yield return new WaitForSeconds(1f);
        VHAnimator.SetBool(_state, false);

        yield return new WaitForSeconds(5f);

        isAnimating = false;

    }

     /* 
    void Update()
    {
        R_JointRefVector = BodyViewRef.GetComponent<BodyJointData>().R_JointToTrackPos;

        if(!isAnimating)
        {
            if( R_JointRefVector.x <= -0.4f &&  R_JointRefVector.x >=-0.8f 
                && R_JointRefVector.y <= 1.3f &&  R_JointRefVector.y >=0.7f 
                    && R_JointRefVector.z <= 2.0f &&  R_JointRefVector.z >=1.7f)
            {
                StartCoroutine(VAnimate("shake", 0));
            }

            if( R_JointRefVector.x <= -1f &&  R_JointRefVector.x >=-1.8f 
                && R_JointRefVector.y <= 0.7f &&  R_JointRefVector.y >= -1.3f 
                    && R_JointRefVector.z <= 1.4f &&  R_JointRefVector.z >=0.7f)
            {
                StartCoroutine(VAnimate("salute", 1));
            }


        }

    }
    */
}