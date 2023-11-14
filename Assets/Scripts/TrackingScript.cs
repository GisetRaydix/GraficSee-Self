using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class TrackingScript : MonoBehaviour
{
    #region Class declaration
    //�����ŃN���X���g���Ă���̂́AEditor��Ŏ��s�����Ƀf�o�b�N����ۂ�Inspector��Ō��₷�����邽��
    [Serializable] public class setting
    {
        [SerializeField] public bool blink_Eyes = true;
        [SerializeField] public bool MouseTracking = true;
        [SerializeField] public bool ModeSwitcing = false;
    }
    #endregion
    #region Variable declaration
    //������Inspector��Őݒ荀�ڂ�G��₷�����Ă���̂�UI�Ɏ�������Ƃ���Inspector��UI���Q�l�ɂ��悤�Ƃ�����
    [SerializeField] public setting Setting; 
    [SerializeField,Range(60, 120)] public int FrameRate = 60;
    [SerializeField,Range(1,120)] public int RicognizeFrame;
    [SerializeField,Range(-60,60)] public int RicognizeDB;
    [SerializeField,Range(-60,60)] int DB1,DB2,DB3;
    [SerializeField] private bool dbDisplay = true;
    private FaceRicognition faceRicognition;
    private VRMModelContraoler vrmmodelControler;
    private AudioSource audioSource;
    private float[] audioBuffer = new float[1024];
    private bool RightEyeClose = true, LeftEyeClose = true;
    private int FrameCount = 0;
    private int RightEyeZeroCount = 0, RightEyeElseCount = 0, LeftEyeZeroCount = 0, LeftEyeElseCount = 0;
    #endregion

    void Start()
    {
        FindScripts();
        StartCoroutine(AudioSetUp());
        Application.targetFrameRate = FrameRate;
    }

    void Update()
    {
        if (GameManager.ModelNull) return;
        if (!faceRicognition.SetWebcame)
        {
            NotSetWebcam();
            return;
        }
        if (GameManager.isEmotion) return;
        
        RicognizeTrackingFrame();
        if(AudioOn()) AudioRecognize();
    }

    private void FindScripts()
    {
        faceRicognition = GameObject.Find("FaceRicognition").GetComponent<FaceRicognition>();
        vrmmodelControler = GameObject.Find("ModelControler").GetComponent<VRMModelContraoler>();
    }

    private void RicognizeTrackingFrame()
    {
        FrameCount++;
        CalculateRicognizeFrameEye();
        if(FrameCount == RicognizeFrame)
        {
            //��̏����͂̓��e�͓��l�̕�
            StartCoroutine(TrackingRightEye());
            StartCoroutine(TrackingLeftEye());
            FrameCount = 0;
        }
    }

    private void CalculateRicognizeFrameEye()
    {
        //OpenCV�̖ڂ̔F���̓����ŁA�ڂ����Ɩڂ̔F�������Ȃ��Ȃ邱�Ƃ�����B
        //�t���[���P�ʂŖڂ̓��͒l�����邩�Ȃ������Ď�
        if(faceRicognition.RightEyeSize == 0) RightEyeZeroCount++;
        else if(faceRicognition.RightEyeSize > 0) RightEyeElseCount++;

        if (faceRicognition.LeftEyeSize == 0) LeftEyeZeroCount++;
        else if (faceRicognition.LeftEyeSize > 0) LeftEyeElseCount++;

        //���t���[���Ɉ�x�A�ڂ̓��͒l���ǂꂾ�������������r
        if (FrameCount == RicognizeFrame)
        {
            if(!Setting.blink_Eyes)
            {
                //���͒l���[����̃t���[���������Ƃ����f���̖ڂ��J������Ԃ������or�ڂ��������
                if (LeftEyeZeroCount >= LeftEyeElseCount)
                {
                    RightEyeClose = false;
                    LeftEyeClose = false;
                }
                else
                {
                    RightEyeClose = true;
                    LeftEyeClose = true;
                }
            }
            else
            {
                //�ڂ������Ԃ�����t�ɊJ����or�J��������
                if (RightEyeZeroCount >= RightEyeElseCount) RightEyeClose = false;
                else RightEyeClose = true;

                if (LeftEyeZeroCount >= LeftEyeElseCount) LeftEyeClose = false;
                else LeftEyeClose = true;
            }

            RightEyeZeroCount = 0; RightEyeElseCount = 0;
            LeftEyeZeroCount = 0;LeftEyeElseCount = 0;
        }
    }

    private IEnumerator TrackingRightEye()
    {
        //�ڂ̊J���߂��鏈�����R���[�`���Ŏ������邱�ƂŁA�J���܂ł̓���Ƀ��A���e�B�����������A�j���[�V����������
        if (!RightEyeClose)
        {
            if(vrmmodelControler.blink.blink_right == 100) yield return null;
            else
            {
                for(int i=0;i< RicognizeFrame-1;i++)
                {
                    vrmmodelControler.blink.blink_right += (100 / RicognizeFrame);
                    yield return null;
                }
                vrmmodelControler.blink.blink_right = 100;
            }
        }
        else
        {
            if (vrmmodelControler.blink.blink_right == 0) yield return null;
            else
            {
                for (int i = 0; i < RicognizeFrame - 1; i++)
                {
                    vrmmodelControler.blink.blink_right -= (100 / RicognizeFrame);
                    yield return null;
                }
                vrmmodelControler.blink.blink_right = 0;
            }
        }
    }

    private IEnumerator TrackingLeftEye()
    {
        if (!LeftEyeClose)
        {
            if (vrmmodelControler.blink.blink_left == 100) yield return null;
            else
            {
                for (int i = 0; i < RicognizeFrame - 1; i++)
                {
                    vrmmodelControler.blink.blink_left += (100 / RicognizeFrame);
                    yield return null;
                }
                vrmmodelControler.blink.blink_left = 100;
            }
        }
        else
        {
            if (vrmmodelControler.blink.blink_left == 0) yield return null;
            else
            {
                for (int i = 0; i < RicognizeFrame - 1; i++)
                {
                    vrmmodelControler.blink.blink_left -= (100 / RicognizeFrame);
                    yield return null;
                }
                vrmmodelControler.blink.blink_left = 0;
            }
        }
    }

    private IEnumerator AudioSetUp()
    {
        //�J�����Ɠ��l�}�C�N���������ݒ肳��Ă��邩�̃`�F�b�N
        if (AudioOn())
        {
            //�}�C�N����E��������AudioClip�ōĐ�����B
            //�����̖��_��AudioRecognize()�ɋL�q
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = Microphone.Start(null, true, 1, 44100);
            audioSource.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            audioSource.Play();
        }
        else
        {
            yield return new WaitForSeconds(5);
            StartCoroutine(AudioSetUp());
        }
    }

    private void AudioRecognize()
    {
        if (!Setting.MouseTracking) return;
        audioSource.GetOutputData(audioBuffer, 0);
        float levelMax = 0;

        //�ȉ���AudioClip�Ń}�C�N����̓��͉����Đ������ۂ̉��̍ł��傫���l�𒲂ׂĂ���
        //���̖��_�́A������x�A�v�����ōĐ����Ȃ���΂Ȃ�Ȃ��_��
        //�f�t�H���g�Ŏ��g�̐��������Ă��܂��B
        //�A�v���P�[�V�����̉��ʂ�OS���Ő؂�Ζ��Ȃ��̂ŁA�d�l�Ƃ��Ă���B
        for (int i = 0; i < audioBuffer.Length; i++)
        {
            float wavePeak = audioBuffer[i] * audioBuffer[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }

        //�ő剹�ʂ��d�������ɂ��āA���ʂ����ق�0�`1�ɃX�P�[�����O������
        //�ΐ����g���ĉ��̃f�V�x�������o���Ă���
        float level = Mathf.Sqrt(Mathf.Sqrt(levelMax));
        float db = 20 * Mathf.Log10(level);

        //�����J���߂��鏈�����R���[�`���Ŏ������邱�ƂŁA�J���܂ł̓���Ƀ��A���e�B�����������A�j���[�V����������
        if (db > RicognizeDB) 
        {
            //�f�o�b�N�Ńf�V�x������\������悤�̏���
            if(dbDisplay) Debug.Log("Detected sound at " + db + " dB");
            StartCoroutine(MouseTracking(db));
        }
        else
        {
            StartCoroutine(MouseTrackingSubmit());
        }
    }

    private IEnumerator MouseTracking(float db) 
    {
        //���̑傫���ɍ��킹�Č��̊J���ʂ�ς��郂�[�h�ƈ��ŏ�ɊJ�����[�h��I���\�ɂ���
        if(Setting.ModeSwitcing)
        {
            if (db < DB1)
            {
                for (int i = 0; i < 2; i++)
                {
                    vrmmodelControler.mouth.mouth_value += 5;
                    yield return null;
                }
                vrmmodelControler.mouth. mouth_value = 22;
            }
            else if (db < DB2)
            {
                for (int i = 0; i < 2; i++)
                {
                    vrmmodelControler.mouth.mouth_value += 5;
                    yield return null;
                }
                vrmmodelControler.mouth.mouth_value = 55;
            }
            else if (db < DB3)
            {
                for (int i = 0; i < 2; i++)
                {
                    vrmmodelControler.mouth.mouth_value += 5;
                    yield return null;
                }
                vrmmodelControler.mouth.mouth_value = 80;
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                vrmmodelControler.mouth.mouth_value += 5;
                yield return null;
            }
            vrmmodelControler.mouth.mouth_value = 80;
        }
    }

    private IEnumerator MouseTrackingSubmit()
    {
        if(vrmmodelControler.mouth.mouth_value != 0)
        {
            while (vrmmodelControler.mouth.mouth_value < 3)
            {
                vrmmodelControler.mouth.mouth_value -= 2;
                yield return null;
            }
            vrmmodelControler.mouth.mouth_value = 0;
        }
    }

    private void NotSetWebcam()
    {
        //�J�������Ȃ��Ƃ��Ƀ��f���̖ڂ��J���Ă���悤�ɒ���
        vrmmodelControler.blink.blink_left = 0;
        vrmmodelControler.blink.blink_right = 0;
    }

    private bool AudioOn()
    {
        //�E�F�u�J�����ƈႢ�A�}�C�N�͂ǂ�Ȃ��̂ł����͂��[���ɂȂ邱�Ƃ͂��܂�Ȃ��̂�
        //�P���Ƀ}�C�N�����Ă��邩�ǂ��������̊��m������
        if (Microphone.devices.Length != 0) return true;
        else return false;
    }
}
