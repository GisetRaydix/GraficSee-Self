using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingScript : MonoBehaviour
{
    #region Variable declaration
    private FaceRicognition faceRicognition;
    private VRMModelContraoler vrmmodelControler;
    private int firstFacePositionX,firstFacePositionY;
    private int lastFaceMinusX = 0, lastFaceMinusY = 0;
    private bool OneTimeBool = false;
    #endregion

    void Start()
    {
        FindScripts();
    }

    void Update()
    {
        if (GameManager.ModelNull) return;
        if (!faceRicognition.SetWebcame) return;

        GetFirstPosition();
        NeckXMove();
        NeckYMove();
    }

    private void FindScripts()
    {
        faceRicognition = GameObject.Find("FaceRicognition").GetComponent<FaceRicognition>();
        vrmmodelControler = GameObject.Find("ModelControler").GetComponent<VRMModelContraoler>();
    }

    private void GetFirstPosition()
    {
        //顔を最初に認識したフレームの顔の基点を、モデルの顔の正面位置に設定する。
        //アプリケーションを再ロードすることで、正面位置を設定しなおせるようにする。
        if(faceRicognition.FacePositionX != 0 && !OneTimeBool)
        {
            OneTimeBool = true;
            firstFacePositionX = faceRicognition.FacePositionX;
            firstFacePositionY = faceRicognition.FacePositionY;
        }
    }

    private void NeckYMove()
    {
        //首の動きを顔の正面の基準からどれだけズレているかで顔の動きを制御
        //顔の判定が不安定でかなり顔のブレがあるため、正面のデッドゾーンを設定
        if(OneTimeBool && faceRicognition.FacePositionY != firstFacePositionY)
        {
            int faceMinus = faceRicognition.FacePositionY - firstFacePositionY;

            if (lastFaceMinusY - faceMinus < -80) return;

            lastFaceMinusY = faceMinus;

            if(faceMinus > 10)
            {
                //基準値からのズレの値を、モデル首の動きの値に丸めて代入
                float quarterValue = faceMinus / 4;
                if (quarterValue > 25) vrmmodelControler.head.Head_X = 25;
                else vrmmodelControler.head.Head_X = (int)quarterValue;
            }
            else
            {
                vrmmodelControler.head.Head_X = 0;
            }
        }
    }

    private void NeckXMove()
    {
        //NeckYMoveと同様
        if (OneTimeBool && faceRicognition.FacePositionX != firstFacePositionX)
        {
            int faceMinus = faceRicognition.FacePositionX - firstFacePositionX;

            if (lastFaceMinusX - faceMinus < -80) return;

            lastFaceMinusX = faceMinus;

            if (faceMinus > 5)
            {
                if (faceMinus > 20) vrmmodelControler.head.Head_Y = 20;
                else vrmmodelControler.head.Head_Y = faceMinus;
            }
            else if (faceMinus < -5)
            {
                if (faceMinus < -20) vrmmodelControler.head.Head_Y = -20;
                else vrmmodelControler.head.Head_Y = faceMinus;
            }
            else
            {
                vrmmodelControler.head.Head_Y = 0;
            }
        }
    }
}