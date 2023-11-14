using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceRicognition : MonoBehaviour
{
    #region Variable declaration
    public WebCamTexture webCamTexture;
    private CascadeClassifier faceCascade;
    private CascadeClassifier rightEyesCascade;
    private CascadeClassifier leftEyesCascade;
    [SerializeField, Range(0, 5)] private float scaleFactor = 1.1f;
    [SerializeField, Range(0, 5)] private int minNeighbors = 2;
    public int RightEyeSize, LeftEyeSize;
    public int FacePositionX, FacePositionY;
    private Texture2D tex;
    private OpenCvSharp.Rect upperFaceRight;
    private OpenCvSharp.Rect upperFaceLeft;
    private Mat upperFaceRightRegion;
    private Mat upperFaceLeftRegion;
    private Mat mat;
    private Texture newTex;

    //以下でプロパティを使用しているのは外部でboolを参照する際の安全な方法の勉強で使用したため
    //今回、基本的にはbool以外の変数もstaticやpublicで使用

    private bool setWebcame = false;
    public bool SetWebcame
    {
        get 
        { 
            return setWebcame; 
        }
        private set 
        { 
            setWebcame = value; 
        }
    }
    #endregion

    private void Start()
    {
        StartCoroutine(FirstProgress());
    }

    private void Update()
    {
        if (GameManager.ModelNull) return;
        if (!SetWebcame) return;

        UpdatingProcess();
    }

    private void FirstInitialize()
    {
        //今回の処理で使うクラスの初期化
        faceCascade = new CascadeClassifier(Application.streamingAssetsPath + "\\haarcascade_frontalface_default.xml");
        rightEyesCascade = new CascadeClassifier(Application.streamingAssetsPath + "\\haarcascade_righteye_2splits.xml");
        leftEyesCascade = new CascadeClassifier(Application.streamingAssetsPath + "\\haarcascade_lefteye_2splits.xml");
        tex = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
        upperFaceRight = new OpenCvSharp.Rect();
        upperFaceLeft = new OpenCvSharp.Rect();
        upperFaceRightRegion = new Mat();
        upperFaceLeftRegion = new Mat();
        mat = new Mat();
        newTex = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
    }

    private void UpdatingProcess()
    {
        if (webCamTexture.isPlaying)
        {
            tex.SetPixels(webCamTexture.GetPixels());
            tex.Apply();
            mat = OpenCvSharp.Unity.TextureToMat(tex);
        }
        else return;

        //顔の認識範囲を最小値100px×100pxで検出するようにする。
        var faces = faceCascade.DetectMultiScale(mat, scaleFactor, minNeighbors, HaarDetectionType.ScaleImage, new Size(100, 100));

        if (faces.Length > 0)
        {
            //カメラ映像内で認識される顔を一つに絞るため、認識されてる顔の最初だけを使用する
            var face = faces[0];

            //顔の位置とサイズに基づいて四角形を描画。
            Cv2.Rectangle(mat, face, Scalar.Red, 3);

            //この時の顔の左上の座標を顔の基点として変数として顔の動きのトラッキングに用いる。
            FacePositionX = face.X;
            FacePositionY = face.Y;

            //顔の領域から目を検出為に顔の範囲を区切る
            //顔の基点からX軸方向に向けて顔の半分の大きさを右目の範囲とする。
            upperFaceRight.X = face.X;
            upperFaceRight.Y = face.Y;
            upperFaceRight.Width = face.Width / 2;
            upperFaceRight.Height = face.Height / 2;

            //右目X敷く方向の終点から顔のX軸方向に向けて顔の半分の大きさを左目の範囲とする。
            upperFaceLeft.X = face.X + face.Width / 2;
            upperFaceLeft.Y = face.Y;
            upperFaceLeft.Width = face.Width / 2;
            upperFaceLeft.Height = face.Height / 2;

            //右目の範囲左目の範囲ともに顔をY軸方向で半分にした時の上側でもあるとする。

            //それぞれの目の範囲を切り取り、別のMat型のテクスチャ(OpenCVで使用するテクスチャクラス)に代入する
            mat.SubMat(upperFaceRight).CopyTo(upperFaceRightRegion);
            mat.SubMat(upperFaceLeft).CopyTo(upperFaceLeftRegion);

            //目の認識範囲を最小値1px×1pxで検出するようにする。
            OpenCvSharp.Rect[] righteyes = rightEyesCascade.DetectMultiScale(upperFaceRightRegion, scaleFactor, minNeighbors, HaarDetectionType.ScaleImage, new Size(1, 1));
            OpenCvSharp.Rect[] lefteyes = leftEyesCascade.DetectMultiScale(upperFaceLeftRegion, scaleFactor, minNeighbors, HaarDetectionType.ScaleImage, new Size(1, 1));

            //検出された目の位置とサイズを取得

            if (righteyes.Length > 0)
            {
                //顔と同様に目も一つに絞る。
                OpenCvSharp.Rect eyeRect = righteyes[0];
                eyeRect.X += upperFaceRight.X;
                eyeRect.Y += upperFaceRight.Y;
                Cv2.Rectangle(mat, eyeRect, Scalar.Green, 3);
                //認識した目の大きさをパブリック変数で保存して使用する
                RightEyeSize = eyeRect.Width;
            }
            else RightEyeSize = 0;
            if (lefteyes.Length > 0)
            {
                OpenCvSharp.Rect eyeRect = lefteyes[0];
                eyeRect.X += upperFaceLeft.X;
                eyeRect.Y += upperFaceLeft.Y;
                Cv2.Rectangle(mat, eyeRect, Scalar.Green, 3);
                LeftEyeSize = eyeRect.Width;
            }
            else LeftEyeSize = 0;
        }

        //描画したMatをデバック用のPlaneのマテリアルに映す(アプリ内ではデバック画面は映らない)
        OpenCvSharp.Unity.MatToTexture(mat,(Texture2D)newTex);
        GetComponent<Renderer>().material.mainTexture = newTex;
    }

    private IEnumerator FirstProgress()
    {
        //ウェブカメラが正しく設定されているかのチェック
        if (WebCamTexture.devices.Length != 0)
        {
            //基本的にwebカメラのリストの0番目のデバイスがデフォルトになる。
            //このツールはOBSを前提としているため、OBS以外のカメラを認識するようにする。

            WebCamDevice[] devices = WebCamTexture.devices;
            webCamTexture = new WebCamTexture();

            if (devices[0].name != "OBS Virtual Camera")
            {
                GetComponent<Renderer>().material.mainTexture = webCamTexture;
                webCamTexture.Play();
                FirstInitialize();
                SetWebcame = true;
                yield return null;
            }
            else
            {
                //Updateに入れずに5秒おきに実行するのは処理を軽くするため。
                webCamTexture = null;
                yield return new WaitForSeconds(5.0f);
                Debug.Log("one more");
                StartCoroutine(FirstProgress());
            }
        }
        else
        {
            yield return new WaitForSeconds(5.0f);
            Debug.Log("one more");
            StartCoroutine(FirstProgress());
        }
    }
}

