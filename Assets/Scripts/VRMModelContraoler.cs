using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
//using UniVRM10;

public class VRMModelContraoler : MonoBehaviour
{
    #region Class declaration
    //ここでクラスを使っているのは、Editor上で実行せずにデバックする際にInspector上で見やすくするため
    //InspectorのUIがどう表示されるかの勉強も兼ねている
    [Serializable] public class Blink
    {
        [SerializeField, Range(0, 100)] public float blink_right;
        [SerializeField, Range(0, 100)] public float blink_left;
    }
    [Serializable] public class Mouth
    {
        [SerializeField, Range(0, 80)] public float mouth_value;
    }
    [Serializable] public class Emotions
    {
        [SerializeField, Range(0, 100)] public float Blend_smile;
        [SerializeField, Range(0, 100)] public float Blend_anger;
        [SerializeField, Range(0, 100)] public float Blend_sad;
        [SerializeField, Range(0, 100)] public float Blend_suprise;
    }
    [Serializable] public class Head
    {
        [SerializeField] public Transform Head_rot;
        [SerializeField, Range(-20, 20)] public float Head_X = 0;
        [SerializeField, Range(-20, 25)] public float Head_Y = 0;
        [SerializeField, Range(-20, 20)] public float Head_Z = 0;
    }
    [Serializable] public class Shoulder
    {
        [SerializeField] public Transform LeftShoulder_rot;
        [SerializeField, Range(-100, 50)] public float LeftShoulder_X = 0;
        [SerializeField, Range(-1, 1)] public float LeftShoulder_Y = 0;
        [SerializeField, Range(-30, 65)] public float LeftShoulder_Z = 65;
        [SerializeField] public Transform RightShoulder_rot;
        [SerializeField, Range(-100, 50)] public float RightShoulder_X = 0;
        [SerializeField, Range(1, -1)] public float RightShoulder_Y = 0;
        [SerializeField, Range(30, -65)] public float RightShoulder_Z = -65;
    }
    [Serializable] public class Elbow
    {
        [SerializeField] public  Transform LeftElbow_rot;
        [SerializeField, Range(-100, 50)] public float LeftElbow_X = 0;
        [SerializeField, Range(-90, 90)] public float LeftElbow_Y = 0;
        [SerializeField, Range(-30, 65)] public float LeftElbow_Z = 65;
        [SerializeField] public Transform RightElbow_rot;
        [SerializeField, Range(-100, 50)] public float RightElbow_X = 0;
        [SerializeField, Range(90, -90)] public float RightElbow_Y = 0;
        [SerializeField, Range(30, -65)] public float RightElbow_Z = -65;
    }
    [Serializable] public class Wrist
    {
        [SerializeField] public Transform LestWrist_rot;
        [SerializeField, Range(-100, 50)] public float LestWrist_X = 0;
        [SerializeField, Range(-90, 90)] public float LestWrist_Y = 0;
        [SerializeField, Range(-30, 65)] public float LestWrist_Z = 65;
        [SerializeField] public Transform RightWrist_rot;
        [SerializeField, Range(-100, 50)] public float RightWrist_X = 0;
        [SerializeField, Range(90, -90)] public float RightWrist_Y = 0;
        [SerializeField, Range(30, -65)] public float RightWrist_Z = -65;
    }
    [Serializable] public class ModelSetting
    {
        [SerializeField, Range(-90, 90)] public float modelRotateY = 0;
    }
    #endregion 
    #region Constant declaration
    enum Test { test1,test2,test3 };
    #endregion
    #region Variable declaration
    [Header("BlendShapes")]
    [SerializeField] public Blink blink;
    [SerializeField] public Mouth mouth;
    [SerializeField] public Emotions emotions;
    [Header("ModelBody")]
    [SerializeField] public Head head;
    [SerializeField] public Shoulder shoulder;
    [SerializeField] public Elbow elbow;
    [SerializeField] public Wrist wrist;
    [SerializeField] public ModelSetting modelSetting;
    [Header("Others")]
    [SerializeField] private string ModelTag = "model";
    public GameObject Model;
    #endregion

    private void Update()
    {
        if(!MonitorModel()) return;
        if (GameManager.ModelNull) return;

        ValueUpdate();
    }

    //実行せずにデバックする用のメソッド
    private void OnValidate()
    {
        ValueUpdate();
    }

    public void ValueUpdate()
    {
        #region Blink
        GameObject.Find("Face").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(14, blink.blink_right);
        GameObject.Find("Face").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(15, blink.blink_left);
        #endregion
        #region Mouse
        GameObject.Find("Face").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(39, mouth.mouth_value);
        #endregion
        #region Emotions
        GameObject.Find("Face").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(3, emotions.Blend_smile);
        GameObject.Find("Face").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(1, emotions.Blend_anger);
        GameObject.Find("Face").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(5, emotions.Blend_suprise);
        GameObject.Find("Face").GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(4, emotions.Blend_sad);
        #endregion
        #region Body
        float rotate = -180;
        if (Model != null)
        {
            rotate = rotate + modelSetting.modelRotateY;
            Model.transform.rotation = Quaternion.Euler(0f, -rotate, 0f);
        }
        head.Head_rot.rotation = Quaternion.Euler(head.Head_X, head.Head_Y - rotate, head.Head_Z);
        shoulder.LeftShoulder_rot.rotation = Quaternion.Euler(shoulder.LeftShoulder_X, shoulder.LeftShoulder_Y - rotate, shoulder.LeftShoulder_Z);
        shoulder.RightShoulder_rot.rotation = Quaternion.Euler(shoulder.RightShoulder_X, shoulder.RightShoulder_Y - rotate, shoulder.RightShoulder_Z);
        elbow.LeftElbow_rot.rotation = Quaternion.Euler(elbow.LeftElbow_X, elbow.LeftElbow_Y - rotate, elbow.LeftElbow_Z);
        elbow.RightElbow_rot.rotation = Quaternion.Euler(elbow.RightElbow_X, elbow.RightElbow_Y - rotate, elbow.RightElbow_Z);
        wrist.LestWrist_rot.rotation = Quaternion.Euler(wrist.LestWrist_X, wrist.LestWrist_Y - rotate, wrist.LestWrist_Z);
        wrist.RightWrist_rot.rotation = Quaternion.Euler(wrist.RightWrist_X, wrist.RightWrist_Y - rotate, wrist.RightWrist_Z);
        #endregion
    }

    //モデルインポート時にモデルを消すが、その時にエラーを回避する。
    //同時にVRMモデルの標準の関節、BlendShapeなどを検索する。
    private bool MonitorModel()
    {
        if (Model == null)
        {
            GameManager.ModelNull = true;
            FindModel();
            return false;
        }
        return true;
    }

    private void FindModel()
    {
        Model = GameObject.FindWithTag(ModelTag);
        if(Model != null)
        {
            Debug.Log("yes");
            Debug.Log(Model.name);
            FindRot();
            GameManager.ModelNull = false;
        }
        else
        {
            Debug.Log("no");
        }
    }

    //VRMモデルの標準の関節名で検索
    private void FindRot()
    {
        head.Head_rot = GameObject.Find("J_Bip_C_Head").transform;
        shoulder.LeftShoulder_rot = GameObject.Find("J_Bip_L_UpperArm").transform;
        shoulder.RightShoulder_rot = GameObject.Find("J_Bip_R_UpperArm").transform;
        elbow.LeftElbow_rot = GameObject.Find("J_Bip_L_LowerArm").transform;
        elbow.RightElbow_rot = GameObject.Find("J_Bip_R_LowerArm").transform;
        wrist.LestWrist_rot = GameObject.Find("J_Bip_L_Hand").transform;
        wrist.RightWrist_rot = GameObject.Find("J_Bip_R_Hand").transform;
    }
}
