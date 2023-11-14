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
        //����ŏ��ɔF�������t���[���̊�̊�_���A���f���̊�̐��ʈʒu�ɐݒ肷��B
        //�A�v���P�[�V�������ă��[�h���邱�ƂŁA���ʈʒu��ݒ肵�Ȃ�����悤�ɂ���B
        if(faceRicognition.FacePositionX != 0 && !OneTimeBool)
        {
            OneTimeBool = true;
            firstFacePositionX = faceRicognition.FacePositionX;
            firstFacePositionY = faceRicognition.FacePositionY;
        }
    }

    private void NeckYMove()
    {
        //��̓�������̐��ʂ̊����ǂꂾ���Y���Ă��邩�Ŋ�̓����𐧌�
        //��̔��肪�s����ł��Ȃ��̃u�������邽�߁A���ʂ̃f�b�h�]�[����ݒ�
        if(OneTimeBool && faceRicognition.FacePositionY != firstFacePositionY)
        {
            int faceMinus = faceRicognition.FacePositionY - firstFacePositionY;

            if (lastFaceMinusY - faceMinus < -80) return;

            lastFaceMinusY = faceMinus;

            if(faceMinus > 10)
            {
                //��l����̃Y���̒l���A���f����̓����̒l�Ɋۂ߂đ��
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
        //NeckYMove�Ɠ��l
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