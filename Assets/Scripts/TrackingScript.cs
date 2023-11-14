using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class TrackingScript : MonoBehaviour
{
    #region Class declaration
    //ここでクラスを使っているのは、Editor上で実行せずにデバックする際にInspector上で見やすくするため
    [Serializable] public class setting
    {
        [SerializeField] public bool blink_Eyes = true;
        [SerializeField] public bool MouseTracking = true;
        [SerializeField] public bool ModeSwitcing = false;
    }
    #endregion
    #region Variable declaration
    //ここでInspector上で設定項目を触りやすくしているのはUIに実装するときにInspectorのUIを参考にしようとした為
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
            //二つの処理はの内容は同様の物
            StartCoroutine(TrackingRightEye());
            StartCoroutine(TrackingLeftEye());
            FrameCount = 0;
        }
    }

    private void CalculateRicognizeFrameEye()
    {
        //OpenCVの目の認識の特徴で、目を閉じると目の認識をしなくなることがある。
        //フレーム単位で目の入力値があるかないかを監視
        if(faceRicognition.RightEyeSize == 0) RightEyeZeroCount++;
        else if(faceRicognition.RightEyeSize > 0) RightEyeElseCount++;

        if (faceRicognition.LeftEyeSize == 0) LeftEyeZeroCount++;
        else if (faceRicognition.LeftEyeSize > 0) LeftEyeElseCount++;

        //数フレームに一度、目の入力値がどれだけあったかを比較
        if (FrameCount == RicognizeFrame)
        {
            if(!Setting.blink_Eyes)
            {
                //入力値がゼロ回のフレームが多いときモデルの目を開いた状態から閉じるor目を閉じ続ける
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
                //目を閉じた状態かから逆に開けるor開け続ける
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
        //目の開け閉めする処理をコルーチンで実装することで、開くまでの動作にリアリティを持たせたアニメーションを実装
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
        //カメラと同様マイクが正しく設定されているかのチェック
        if (AudioOn())
        {
            //マイクから拾った音をAudioClipで再生する。
            //ここの問題点はAudioRecognize()に記述
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

        //以下でAudioClipでマイクからの入力音を再生した際の音の最も大きい値を調べている
        //この問題点は、音を一度アプリ内で再生しなければならない点で
        //デフォルトで自身の声が入ってしまう。
        //アプリケーションの音量をOS側で切れば問題ないので、仕様としている。
        for (int i = 0; i < audioBuffer.Length; i++)
        {
            float wavePeak = audioBuffer[i] * audioBuffer[i];
            if (levelMax < wavePeak)
            {
                levelMax = wavePeak;
            }
        }

        //最大音量を二重平方根にして、音量のﾚﾍﾞﾙを0～1にスケーリングした後
        //対数を使って仮のデシベル数を出している
        float level = Mathf.Sqrt(Mathf.Sqrt(levelMax));
        float db = 20 * Mathf.Log10(level);

        //口を開け閉めする処理をコルーチンで実装することで、開くまでの動作にリアリティを持たせたアニメーションを実装
        if (db > RicognizeDB) 
        {
            //デバックでデシベル数を表示するようの処理
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
        //声の大きさに合わせて口の開く量を変えるモードと一定で常に開くモードを選択可能にする
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
        //カメラがないときにモデルの目が開いているように調節
        vrmmodelControler.blink.blink_left = 0;
        vrmmodelControler.blink.blink_right = 0;
    }

    private bool AudioOn()
    {
        //ウェブカメラと違い、マイクはどんなものでも入力がゼロになることはあまりないので
        //単純にマイクがついているかどうかだけの感知をする
        if (Microphone.devices.Length != 0) return true;
        else return false;
    }
}
