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
    //表情と身体のシェイプキー設定のための配列を作成
    [SerializeField] public KeyCode[] keyCodes = new KeyCode[8];
    #endregion
    #region Variable declaration
    //スタティックなブールで、モデルが無いときに全てのアップデートを止める処理を作成
    //UI周りのみブールがオフでも触れるように調整
    public static bool ModelNull = false;
    public int FrameRate = 60;
    private FaceRicognition faceRicognition;
    private VRMModelContraoler vrmmodelContraoler;
    private UIManager uiManager;
    private bool Alpha1 = false, Alpha2 = false, Alpha3 = false, Alpha4 = false;
    public static bool isEmotion = false;
    //uniVRMのモデルインポートクラスの配列を作成
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
        //シーンロード、アプリケーション終了処理とUIのショートカット機能を分かりやすく管理するため
        //ゲームマネージャー内で全てのショートカットを管理
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
            Debug.Log("シーンロード");
            if (faceRicognition.webCamTexture.isPlaying) faceRicognition.webCamTexture.Stop();
            SceneManager.sceneLoaded += GameSceneLoadProsess;
            SceneManager.LoadScene("main");
        }
    }

    public void OnGameSceneLoad()
    {
        Debug.Log("シーンロード");
        if (faceRicognition.webCamTexture.isPlaying) faceRicognition.webCamTexture.Stop();
        SceneManager.sceneLoaded += GameSceneLoadProsess;
        SceneManager.LoadScene("main");
    }

    private void GameSceneLoadProsess(Scene next, LoadSceneMode mode)
    {
        //シーンを再ロードした際に引き継ぎ処理などを記載
        //モデルをデフォルト以外に設定した際の処理を記述する予定だったが、
        //アプリケーションを終了時にも引き継げるように今後アップデートを検討中のため未実装
        //後学のために処理を残す。
        Debug.Log("ロード後の実行");
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

    //ショートカットキーの条件式が複雑になるのを避けるために処理を差別化
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
        //音や目などの入力にコルーチンを使用している関係上、BlendShapesの入力値をリセット後に
        //BlendShapesの値が動いてしまう場合があるため、数フレーム待ってから再度実行。
        //コルーチン処理を待ってから入力値をリセットしてしまうため、1～2フレームだけBrendShapeが混ざって表示されてしまう。
        //解決策としては、BlendShapesの入力値をリセットしたらという条件式に入れるなど。
        //今後のhandShapekeyを作成時に修正予定
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
        //BlendShapesが複数被らないようにそれまでのBlendShapesの入力値をリセットしておく
        vrmmodelContraoler.blink.blink_right = 0f;
        vrmmodelContraoler.blink.blink_left = 0f;
        vrmmodelContraoler.mouth.mouth_value = 0f;
    }

    //unity simple file browserという外部プラグインを使用してVRMファイル選択を実現
    //streamingAssetsPath内のmodelファイル内にモデルを手動で配置（実行ファイル内のREADMEにパスを記載）することで自作モデルを動かすことが可能
    //モデルファイルを検索してインポートする（modelファイル内にアプリケーション側でモデルを配置する）処理を書きたかったが
    //unityではビルド後にstreamingAssets以外の外部のインポートが出来ないため上記の処理になった。
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

        //モデルが正常に切り替わるまで、LoadAsyncを使用して処理を待つようにしている。
        vrmInstance.Add(await context.LoadAsync(new RuntimeOnlyAwaitCaller()));
        vrmInstance[vrmInstance.Count - 1].ShowMeshes();
        vrmInstance[vrmInstance.Count - 1].gameObject.tag = "model";
        vrmInstance[vrmInstance.Count - 1].gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
    }

}
