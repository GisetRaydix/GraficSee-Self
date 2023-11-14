using System;
using System.Collections;
using System.Collections.Generic;
using SimpleFileBrowser;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRM;
using UniGLTF;
using VRMShaders;

public class GameManager : MonoBehaviour
{
    #region Constant declaration
    //�\��Ɛg�̂̃V�F�C�v�L�[�ݒ�̂��߂̔z����쐬
    [SerializeField] public KeyCode[] keyCodes = new KeyCode[8];
    #endregion
    #region Variable declaration
    //�X�^�e�B�b�N�ȃu�[���ŁA���f���������Ƃ��ɑS�ẴA�b�v�f�[�g���~�߂鏈�����쐬
    //UI����̂݃u�[�����I�t�ł��G���悤�ɒ���
    public static bool ModelNull = false;
    public int FrameRate = 60;
    private FaceRicognition faceRicognition;
    private VRMModelContraoler vrmmodelContraoler;
    private UIManager uiManager;
    private bool Alpha1 = false, Alpha2 = false, Alpha3 = false, Alpha4 = false;
    public static bool isEmotion = false;
    //uniVRM�̃��f���C���|�[�g�N���X�̔z����쐬
    public List<UniGLTF.RuntimeGltfInstance> vrmInstance = new List<RuntimeGltfInstance>();
    #endregion

    void Start()
    {
        Application.targetFrameRate = FrameRate;
        faceRicognition =  GameObject.Find("FaceRicognition").GetComponent<FaceRicognition>();
        vrmmodelContraoler = GameObject.Find("ModelControler").GetComponent<VRMModelContraoler>();
        uiManager = this.GetComponent<UIManager>();
        ModelNull = false;
        isEmotion = false;
    }

    void Update()
    {
        ShortCut();
    }

    private void ShortCut()
    {
        //�V�[�����[�h�A�A�v���P�[�V�����I��������UI�̃V���[�g�J�b�g�@�\�𕪂���₷���Ǘ����邽��
        //�Q�[���}�l�[�W���[���őS�ẴV���[�g�J�b�g���Ǘ�
        GameSceneLoad();
        TrackingPause();
        EmotionKey();
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            uiManager.SaveButton();
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.I))
            uiManager.OnDefaultButton();
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Escape))
            uiManager.OnQuitButton();
    }

    private void GameSceneLoad()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKey(KeyCode.R))
        {
            Debug.Log("�V�[�����[�h");
            if (faceRicognition.webCamTexture.isPlaying) faceRicognition.webCamTexture.Stop();
            SceneManager.sceneLoaded += GameSceneLoadProsess;
            SceneManager.LoadScene("main");
        }
    }

    public void OnGameSceneLoad()
    {
        Debug.Log("�V�[�����[�h");
        if (faceRicognition.webCamTexture.isPlaying) faceRicognition.webCamTexture.Stop();
        SceneManager.sceneLoaded += GameSceneLoadProsess;
        SceneManager.LoadScene("main");
    }

    private void GameSceneLoadProsess(Scene next, LoadSceneMode mode)
    {
        //�V�[�����ă��[�h�����ۂɈ����p�������Ȃǂ��L��
        //���f�����f�t�H���g�ȊO�ɐݒ肵���ۂ̏������L�q����\�肾�������A
        //�A�v���P�[�V�������I�����ɂ������p����悤�ɍ���A�b�v�f�[�g���������̂��ߖ�����
        //��w�̂��߂ɏ������c���B
        Debug.Log("���[�h��̎��s");
    }

    private void TrackingPause()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P) && !ModelNull)
        {
            ModelNull = true;
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.P) && ModelNull)
        {
            ModelNull = false;
        }
    }

    //�V���[�g�J�b�g�L�[�̏����������G�ɂȂ�̂�����邽�߂ɏ��������ʉ�
    public void OnTrackingPause()
    {
        if (!ModelNull)
        {
            ModelNull = true;
        }
        else
        {
            ModelNull = false;
        }
    }

    private void EmotionKey()
    {
        if (Input.GetKeyDown(keyCodes[0]))
        {
            if (!Alpha1 && !isEmotion)
            {
                EmotionInitialize();
                vrmmodelContraoler.emotions.Blend_smile = 100f;
                Alpha1 = true;
                isEmotion = true;
                StartCoroutine(MonitorEmotion());
            }
            else if(Alpha1 && isEmotion)
            {
                vrmmodelContraoler.emotions.Blend_smile = 0f;
                Alpha1 = false;
                isEmotion = false;
            }
        }
        else if (Input.GetKeyDown(keyCodes[1]))
        {
            if (!Alpha2 && !isEmotion)
            {
                EmotionInitialize();
                vrmmodelContraoler.emotions.Blend_anger = 100f;
                Alpha2 = true;
                isEmotion = true;
                StartCoroutine(MonitorEmotion());
            }
            else if (Alpha2 && isEmotion)
            {
                vrmmodelContraoler.emotions.Blend_anger = 0f;
                Alpha2 = false;
                isEmotion = false;
            }
        }
        else if (Input.GetKeyDown(keyCodes[2]))
        {
            if (!Alpha3 && !isEmotion)
            {
                EmotionInitialize();
                vrmmodelContraoler.emotions.Blend_sad = 100f;
                Alpha3 = true;
                isEmotion = true;
                StartCoroutine(MonitorEmotion());
            }
            else if(Alpha3 && isEmotion)
            {
                vrmmodelContraoler.emotions.Blend_sad = 0f;
                Alpha3 = false;
                isEmotion = false;
            }
        }
        else if (Input.GetKeyDown(keyCodes[3]))
        {
            if (!Alpha4 && !isEmotion)
            {
                EmotionInitialize();
                vrmmodelContraoler.emotions.Blend_suprise = 100f;
                Alpha4 = true;
                isEmotion = true;
                StartCoroutine(MonitorEmotion());
            }
            else if(Alpha4 && isEmotion)
            {
                vrmmodelContraoler.emotions.Blend_suprise = 0f;
                Alpha4 = false;
                isEmotion = false;
            }
        }
    }

    private IEnumerator MonitorEmotion()
    {
        //����ڂȂǂ̓��͂ɃR���[�`�����g�p���Ă���֌W��ABlendShapes�̓��͒l�����Z�b�g���
        //BlendShapes�̒l�������Ă��܂��ꍇ�����邽�߁A���t���[���҂��Ă���ēx���s�B
        //�R���[�`��������҂��Ă�����͒l�����Z�b�g���Ă��܂����߁A1�`2�t���[������BrendShape���������ĕ\������Ă��܂��B
        //������Ƃ��ẮABlendShapes�̓��͒l�����Z�b�g������Ƃ����������ɓ����ȂǁB
        //�����handShapekey���쐬���ɏC���\��
        for (int i = 0;i < 5;i++) yield return null;

        if (vrmmodelContraoler.blink.blink_right != 0)
        {
            EmotionInitialize();
            yield break;
        }
        else if(vrmmodelContraoler.blink.blink_left != 0)
        {
            EmotionInitialize();
            yield break;
        }
        else if(vrmmodelContraoler.mouth.mouth_value != 0)
        {
            EmotionInitialize();
            yield break;
        }
    }

    private void EmotionInitialize()
    {
        //BlendShapes���������Ȃ��悤�ɂ���܂ł�BlendShapes�̓��͒l�����Z�b�g���Ă���
        vrmmodelContraoler.blink.blink_right = 0f;
        vrmmodelContraoler.blink.blink_left = 0f;
        vrmmodelContraoler.mouth.mouth_value = 0f;
    }

    //unity simple file browser�Ƃ����O���v���O�C�����g�p����VRM�t�@�C���I��������
    //streamingAssetsPath����model�t�@�C�����Ƀ��f�����蓮�Ŕz�u�i���s�t�@�C������README�Ƀp�X���L�ځj���邱�ƂŎ��샂�f���𓮂������Ƃ��\
    //���f���t�@�C�����������ăC���|�[�g����imodel�t�@�C�����ɃA�v���P�[�V�������Ń��f����z�u����j��������������������
    //unity�ł̓r���h���streamingAssets�ȊO�̊O���̃C���|�[�g���o���Ȃ����ߏ�L�̏����ɂȂ����B
    public void OpenFilePanel()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;
        FileBrowser.SetFilters(true, new FileBrowser.Filter("VRM Files", ".vrm"));
        FileBrowser.ShowLoadDialog((paths) => { LoadVRM(paths[0]); }, null, FileBrowser.PickMode.Files, false, streamingAssetsPath + "\\model", "Select VRM", "Select");
    }

    private async void LoadVRM(string path)
    {
        var bytes = File.ReadAllBytes(path);
        using var gltfData = new AutoGltfFileParser(path).Parse();
        var vrm = new VRMData(gltfData);
        using var context = new VRMImporterContext(vrm);

        Destroy(vrmmodelContraoler.Model);

        //���f��������ɐ؂�ւ��܂ŁALoadAsync���g�p���ď�����҂悤�ɂ��Ă���B
        vrmInstance.Add(await context.LoadAsync(new RuntimeOnlyAwaitCaller()));
        vrmInstance[vrmInstance.Count - 1].ShowMeshes();
        vrmInstance[vrmInstance.Count - 1].gameObject.tag = "model";
        vrmInstance[vrmInstance.Count - 1].gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
    }

}
