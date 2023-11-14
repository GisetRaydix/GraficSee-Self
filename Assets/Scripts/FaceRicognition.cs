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

    //�ȉ��Ńv���p�e�B���g�p���Ă���̂͊O����bool���Q�Ƃ���ۂ̈��S�ȕ��@�̕׋��Ŏg�p��������
    //����A��{�I�ɂ�bool�ȊO�̕ϐ���static��public�Ŏg�p

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
        //����̏����Ŏg���N���X�̏�����
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

        //��̔F���͈͂��ŏ��l100px�~100px�Ō��o����悤�ɂ���B
        var faces = faceCascade.DetectMultiScale(mat, scaleFactor, minNeighbors, HaarDetectionType.ScaleImage, new Size(100, 100));

        if (faces.Length > 0)
        {
            //�J�����f�����ŔF�����������ɍi�邽�߁A�F������Ă��̍ŏ��������g�p����
            var face = faces[0];

            //��̈ʒu�ƃT�C�Y�Ɋ�Â��Ďl�p�`��`��B
            Cv2.Rectangle(mat, face, Scalar.Red, 3);

            //���̎��̊�̍���̍��W����̊�_�Ƃ��ĕϐ��Ƃ��Ċ�̓����̃g���b�L���O�ɗp����B
            FacePositionX = face.X;
            FacePositionY = face.Y;

            //��̗̈悩��ڂ����o�ׂɊ�͈̔͂���؂�
            //��̊�_����X�������Ɍ����Ċ�̔����̑傫�����E�ڂ͈̔͂Ƃ���B
            upperFaceRight.X = face.X;
            upperFaceRight.Y = face.Y;
            upperFaceRight.Width = face.Width / 2;
            upperFaceRight.Height = face.Height / 2;

            //�E��X�~�������̏I�_������X�������Ɍ����Ċ�̔����̑傫�������ڂ͈̔͂Ƃ���B
            upperFaceLeft.X = face.X + face.Width / 2;
            upperFaceLeft.Y = face.Y;
            upperFaceLeft.Width = face.Width / 2;
            upperFaceLeft.Height = face.Height / 2;

            //�E�ڂ͈͍̔��ڂ͈̔͂Ƃ��Ɋ��Y�������Ŕ����ɂ������̏㑤�ł�����Ƃ���B

            //���ꂼ��̖ڂ͈̔͂�؂���A�ʂ�Mat�^�̃e�N�X�`��(OpenCV�Ŏg�p����e�N�X�`���N���X)�ɑ������
            mat.SubMat(upperFaceRight).CopyTo(upperFaceRightRegion);
            mat.SubMat(upperFaceLeft).CopyTo(upperFaceLeftRegion);

            //�ڂ̔F���͈͂��ŏ��l1px�~1px�Ō��o����悤�ɂ���B
            OpenCvSharp.Rect[] righteyes = rightEyesCascade.DetectMultiScale(upperFaceRightRegion, scaleFactor, minNeighbors, HaarDetectionType.ScaleImage, new Size(1, 1));
            OpenCvSharp.Rect[] lefteyes = leftEyesCascade.DetectMultiScale(upperFaceLeftRegion, scaleFactor, minNeighbors, HaarDetectionType.ScaleImage, new Size(1, 1));

            //���o���ꂽ�ڂ̈ʒu�ƃT�C�Y���擾

            if (righteyes.Length > 0)
            {
                //��Ɠ��l�ɖڂ���ɍi��B
                OpenCvSharp.Rect eyeRect = righteyes[0];
                eyeRect.X += upperFaceRight.X;
                eyeRect.Y += upperFaceRight.Y;
                Cv2.Rectangle(mat, eyeRect, Scalar.Green, 3);
                //�F�������ڂ̑傫�����p�u���b�N�ϐ��ŕۑ����Ďg�p����
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

        //�`�悵��Mat���f�o�b�N�p��Plane�̃}�e���A���ɉf��(�A�v�����ł̓f�o�b�N��ʂ͉f��Ȃ�)
        OpenCvSharp.Unity.MatToTexture(mat,(Texture2D)newTex);
        GetComponent<Renderer>().material.mainTexture = newTex;
    }

    private IEnumerator FirstProgress()
    {
        //�E�F�u�J�������������ݒ肳��Ă��邩�̃`�F�b�N
        if (WebCamTexture.devices.Length != 0)
        {
            //��{�I��web�J�����̃��X�g��0�Ԗڂ̃f�o�C�X���f�t�H���g�ɂȂ�B
            //���̃c�[����OBS��O��Ƃ��Ă��邽�߁AOBS�ȊO�̃J������F������悤�ɂ���B

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
                //Update�ɓ��ꂸ��5�b�����Ɏ��s����̂͏������y�����邽�߁B
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

