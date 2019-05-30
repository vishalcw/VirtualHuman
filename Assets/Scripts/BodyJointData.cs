﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;

public class BodyJointData : MonoBehaviour 
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;
    
    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
        
        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };


    [Header("left")]
    public Kinect.JointType L_JointTrackRef;
    public Kinect.JointType L_JointToTrack;
    public Kinect.JointType L2_JointToTrack;
    public Vector3 L_JointToTrackPos;
    public Vector3 L2_JointToTrackPos;
    public Text L_VectorToDisplay;
    public Text L2_VectorToDisplay;


    [Header("right")]
    public Kinect.JointType R_JointTrackRef;
    public Kinect.JointType R_JointToTrack;
    public Kinect.JointType R2_JointToTrack;
    public Vector3 R_JointToTrackPos;
    public Vector3 R2_JointToTrackPos;
    public Text R_VectorToDisplay;
    public Text R2_VectorToDisplay;
    
    void Update () 
    {
        if (BodySourceManager == null)
        {
            return;
        }
        
        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }
        
        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }
        
        List<ulong> trackedIds = new List<ulong>();
        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
              }
                
            if(body.IsTracked)
            {
                trackedIds.Add (body.TrackingId);
            }
        }
        
        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);
        
        // First delete untracked bodies
        foreach(ulong trackingId in knownIds)
        {
            if(!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach(var body in data)
        {
            if (body == null)
            {
                continue;
            }
            
            if(body.IsTracked)
            {
                if(!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }
                
                RefreshBodyObject(body, _Bodies[body.TrackingId]);


            }
        }
    }
    
    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            LineRenderer lr = jointObj.AddComponent<LineRenderer>();
            lr.SetVertexCount(2);
            lr.material = BoneMaterial;
            lr.SetWidth(0.05f, 0.05f);
            
            jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            jointObj.name = jt.ToString();
            jointObj.transform.parent = body.transform;
        }
        
        return body;
    }
    
    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {   
        Vector3 L_TrackPtSrc  = Vector3.zero; 
        Vector3 L_TrackPtDest = Vector3.zero;
        Vector3 L2_TrackPtDest = Vector3.zero;

        Vector3 R_TrackPtSrc  = Vector3.zero; 
        Vector3 R_TrackPtDest = Vector3.zero;
        Vector3 R2_TrackPtDest = Vector3.zero;
       
        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        {
            Kinect.Joint sourceJoint = body.Joints[jt];
            Kinect.Joint? targetJoint = null;
            
            if(_BoneMap.ContainsKey(jt))
            {
                targetJoint = body.Joints[_BoneMap[jt]];
            }
            
            Transform jointObj = bodyObject.transform.Find(jt.ToString());
            jointObj.localPosition = GetVector3FromJoint(sourceJoint);
            
            LineRenderer lr = jointObj.GetComponent<LineRenderer>();
            if(targetJoint.HasValue)
            {
                lr.SetPosition(0, jointObj.localPosition);
                lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));



                 // ***********************                  Left_Tracker       ********************************* //

                if(jt == L_JointTrackRef)
                {
                    L_TrackPtSrc = GetVector3FromJoint(targetJoint.Value) - jointObj.localPosition;
                }

                if(jt == L_JointToTrack) 
                {  

                    L_TrackPtDest = GetVector3FromJoint(targetJoint.Value) - jointObj.localPosition;
                }

                if(jt == L2_JointToTrack) 
                {  
                    L2_TrackPtDest = GetVector3FromJoint(targetJoint.Value) - jointObj.localPosition;
                }

                //L_VectorToDisplay.text = (L_TrackPtDest-L_TrackPtSrc).ToString();
                L_JointToTrackPos = (L_TrackPtDest-L_TrackPtSrc);
                L2_JointToTrackPos = (L2_TrackPtDest-L_TrackPtDest);

                L_VectorToDisplay.text = (GetAngleFromJoint(L_JointToTrackPos)).ToString();
                L2_VectorToDisplay.text = (GetAngleFromJoint(L2_JointToTrackPos)).ToString();

                //L_VectorToDisplay.text = (L_TrackPtDest).ToString();
                

                
                // ***********************              Right_Tracker       ********************************* //
 
                if(jt == R_JointTrackRef)
                {
                    R_TrackPtSrc = GetVector3FromJoint(targetJoint.Value) - jointObj.localPosition;
                }

                if(jt == R_JointToTrack) 
                {

                    R_TrackPtDest = GetVector3FromJoint(targetJoint.Value) - jointObj.localPosition;
                }

                if(jt == R2_JointToTrack) 
                {

                    R2_TrackPtDest = GetVector3FromJoint(targetJoint.Value) - jointObj.localPosition;
                }

                //R_VectorToDisplay.text = (R_TrackPtDest - R_TrackPtSrc).ToString();                
                R_JointToTrackPos = (R_TrackPtDest - R_TrackPtSrc);
                R2_JointToTrackPos = (R2_TrackPtDest - R_TrackPtDest);
                
                VirtualHumanBehaviour.Instance.R_Ang = GetAngleFromJoint(R_JointToTrackPos);
                VirtualHumanBehaviour.Instance.R2_Ang = GetAngleFromJoint(R2_JointToTrackPos);

                R_VectorToDisplay.text = (GetAngleFromJoint(R_JointToTrackPos)).ToString();
                R2_VectorToDisplay.text = (GetAngleFromJoint(R2_JointToTrackPos)).ToString();

                //R_VectorToDisplay.text = (R_TrackPtDest).ToString();


                 // ******************************************************************************************** //



                lr.SetColors(GetColorForState (sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
            }
            else
            {
                lr.enabled = false;
            }
        }
    }
    
    private Vector2 GetAngleFromJoint(Vector3 _jointsdata)
    {   
        Vector2 _angle = Vector2.zero;

        float _r = Mathf.Pow( _jointsdata.x * _jointsdata.x + _jointsdata.y * _jointsdata.y + _jointsdata.z * _jointsdata.z, 0.5f);
        
        if(_r != 0)
            _angle = new Vector2( Mathf.Atan2(_jointsdata.y, _jointsdata.x) * Mathf.Rad2Deg ,  Mathf.Acos(_jointsdata.z/(_r)) * Mathf.Rad2Deg);

        return _angle;
    }


    private static Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
        case Kinect.TrackingState.Tracked:
            return Color.green;

        case Kinect.TrackingState.Inferred:
            return Color.red;

        default:
            return Color.black;
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }

}