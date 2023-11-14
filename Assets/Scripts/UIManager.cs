using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region Class declaration
    //UI設定をJSONデータとして扱うためのClass
    [Serializable] public class SettingData
    {
        public int BackGroundValue = 0;
        public float ModelRotate = 0.5f;
        public float CameraPosX = 0.5f, CameraPosY = 0.5f, CameraPosZ = 0f;
        public float CameraRotX = 0.36f, CameraRotY = 0.5f, CameraRotZ = 0.5f;
        public bool MouthTracking = true, MouthTrckingMode = false, BlinkDivide = false;
        public int FrameRate = 60, RicognizeFrameRate = 2, MinimumDB = -15;
        public string EmotionJoy = "1", EmotionAnger = "2", EmotionSad = "3", EmotionSuprise = "4";
    }
    #endregion
    #region Constant declaration
    //enum 
    #endregion
    #region Variable declaration
    [SerializeField] private GameObject[] settingImg;
    [SerializeField] private Material[] materials = new Material[3];
    [SerializeField] private GameObject BackGround,Field;
    private VRMModelContraoler vrmmodelControler;
    private CameraControler cameraControler;
    private TrackingScript trackingScript;
    private GameManager gameManager;
    private ShapeKeySet shapeKeySet;
    private SettingData settingData;
    private bool settingActive = false;
    private bool StopBool = false;
    [SerializeField] private Dropdown BackGroundDropdown;
    [SerializeField] private Slider ModelRotateSlider;
    [SerializeField] private Slider CameraPosXSlider, CameraPosYSlider, CameraPosZSlider;
    [SerializeField] private Slider CameraRotXSlider, CameraRotYSlider, CameraRotZSlider;
    [SerializeField] private Toggle MTtoggle, MTMtoggle, BDtoggle;
    [SerializeField] private InputField FRfield, RFRfield, MDBfield;
    [SerializeField] private InputField Joyfield, Angerfield, Sadfield, Suprisefield;
    [SerializeField] private List<GameObject> tooltipPanel = new List<GameObject>();
    #endregion

    private void Awake()
    {
        settingData = LoadData();
    }

    void Start()
    {
        vrmmodelControler = GameObject.Find("ModelControler").GetComponent<VRMModelContraoler>();
        cameraControler = GameObject.Find("Main Camera").GetComponent<CameraControler>();
        trackingScript = GameObject.Find("FacaTracking").GetComponent<TrackingScript>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        shapeKeySet = GameObject.Find("GameManager").GetComponent<ShapeKeySet>();
        DataOutSert();
    }

    void Update()
    {
        if (StopBool) return;
        if (Input.GetKeyDown(KeyCode.Space)) StartCoroutine(OnSettingActive());
    }

    private IEnumerator OnSettingActive()
    {
        //設定UIを表示する時のアニメーションをコルーチンを使用して表現
        if (!settingActive)
        {
            settingImg[1].SetActive(true);
            float fadeDuration = 0.3f;
            float fadeTime = Time.time;
            Vector3 fadevalue = new Vector3(500, 0, 0);
            while (Time.time - fadeTime < fadeDuration)
            {
                if(settingImg[0].transform.position.x < 290)
                {
                    settingImg[0].transform.Translate(fadevalue * (Time.time - fadeTime));
                    yield return null;
                }
                else yield return null;
            }
            settingImg[0].transform.position = new Vector3(375, 540, 0);
            settingActive = true;
        }
        else
        {
            settingImg[1].SetActive(false);
            float fadeDuration = 0.3f;
            float fadeTime = Time.time;
            Vector3 fadevalue = new Vector3(500, 0, 0);
            while (Time.time - fadeTime < fadeDuration)
            {
                settingImg[0].transform.Translate(-fadevalue * (Time.time - fadeTime));
                yield return null;
            }
            settingImg[0].transform.position = new Vector3(-375, 540, 0);
            settingActive = false;
        }
    }

    public void OnModelSelectButton()
    {
        gameManager.OpenFilePanel();
    }

    //UIでのドロップダウンで背景色を選択可能に
    //インポートが出来るのであれば、背景をインポート可能にしたかったが、不可能だった。
    //また、OBSで撮影することを想定で作っているため、背景を透過させればいいだけなので断念した。
    public void backcolorSelect(Dropdown dropdownInt)
    {
        BackGround.GetComponent<MeshRenderer>().material = materials[dropdownInt.value];
        Field.GetComponent<MeshRenderer>().material = materials[dropdownInt.value];
    }

    //モデルの位置、カメラの位置をUIで操作することで任意の角度でモデルを動かすことが可能。
    //あくまでもモデルの正面はZ軸方向に垂直になる。
    public void ModelRotationSlider(Slider slider)
    {
        float newModelRotateY = (slider.value - 0.5f) * 180;
        vrmmodelControler.modelSetting.modelRotateY = newModelRotateY;
    }

    public void CameraPosition(Slider slider)
    {
        switch(slider.gameObject.tag)
        {
            case "X":
                float newTrasformX = (slider.value - 0.5f) * 6;
                cameraControler.TrasformX = newTrasformX;
                break;
            case "Y":
                float newTrasformY = (slider.value * 2.3f) + 0.2f;
                cameraControler.TrasformY = newTrasformY;
                break; 
            case "Z":
                float newTrasformZ = (slider.value * -3) - 0.5f;
                cameraControler.TrasformZ = newTrasformZ;
                break;
        }
    }

    public void CameraRotation(Slider slider)
    {
        switch (slider.gameObject.tag)
        {
            case "X":
                float newTrasformX = (slider.value * 50) - 20;
                cameraControler.RotationX = newTrasformX;
                break;
            case "Y":
                float newRotationY = (slider.value * 50) - 25;
                cameraControler.RotationY = newRotationY;
                break;
            case "Z":
                float newRotationZ = (slider.value * 360) - 180;
                cameraControler.RotationZ = newRotationZ;
                break;
        }
    }

    //口のトラッキングのオンオフ
    public void MouthTracking(Toggle toggle)
    {
        trackingScript.Setting.MouseTracking = toggle.isOn;
    }

    //口のトラッキングモードの切り替え
    public void MouthTrackingMode(Toggle toggle)
    {
        trackingScript.Setting.ModeSwitcing = toggle.isOn;
    }

    //両目それぞれで瞬き
    public void BlinkDivide(Toggle toggle)
    {
        trackingScript.Setting.blink_Eyes = toggle.isOn;
    }

    //FrameRateを変更
    //かなり多くの処理に影響するため普段は触るのを非推奨だが、
    //自身で使用する際などに使用したい場合使用
    public void InputFrameRate(InputField input)
    {
        if(int.TryParse(input.text, out int num))
        {
            gameManager.FrameRate = num;
        }
    }

    //判定のFrameRateを設定。こちらも上記と同じ。
    public void InputRicognizeFrameRate(InputField input)
    {
        if (int.TryParse(input.text, out int num))
        {
            trackingScript.RicognizeFrame = num;
        }
    }

    //自身の声の最低音量を設定することで、口の動きを制御
    public void InputMinimumRecognizedVolume(InputField input)
    {
        if (int.TryParse(input.text, out int num))
        {
            trackingScript.RicognizeDB = num;
        }
    }

    //Emotionと未実装のHnadSignのShapeKeyを自身で設定する。
    //InputFieldの一文字目をShapeKeySetに送って、返り値のキーコードをセットする。
    public void ShapeKeyChange(InputField input)
    {
        if (input.text.Length == 0) return;
        char charKey = input.text[0];
        switch(input.gameObject.tag)
        {
            case "0":
                gameManager.keyCodes[0] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
            case "1":
                gameManager.keyCodes[1] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
            case "2":
                gameManager.keyCodes[2] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
            case "3":
                gameManager.keyCodes[3] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
            case "4":
                gameManager.keyCodes[4] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
            case "5":
                gameManager.keyCodes[5] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
            case "6":
                gameManager.keyCodes[6] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
            case "7":
                gameManager.keyCodes[7] = shapeKeySet.GetKeyCodeFromChar(charKey);
                break;
        }
    }

    //現在のUIのInputField内の値をすべて取得（未入力の場合はデフォルト値を取得）し、
    //JsonUtilityを使用してJSONデータに置き換える
    public void SaveButton()
    {
        DataInsert();
        string jsonData = JsonUtility.ToJson(settingData);
        string filePath = Application.persistentDataPath + "/data.json";
        File.WriteAllText(filePath, jsonData);
    }

    //保存されたJSONデータを読み込む処理
    private SettingData LoadData()
    {
        string filePath = Application.persistentDataPath + "/data.json";
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SettingData>(jsonData);
        }
        else return new SettingData();
    }

    public void DataInsert()
    {
        settingData.BackGroundValue = BackGroundDropdown.value;
        settingData.ModelRotate = ModelRotateSlider.value;
        settingData.CameraPosX = CameraPosXSlider.value;
        settingData.CameraPosY = CameraPosYSlider.value;
        settingData.CameraPosZ = CameraPosZSlider.value;
        settingData.CameraRotX = CameraRotXSlider.value;
        settingData.CameraRotY = CameraRotYSlider.value;
        settingData.CameraRotZ = CameraRotZSlider.value;
        settingData.MouthTracking = MTtoggle.isOn;
        settingData.MouthTrckingMode = MTMtoggle.isOn;
        settingData.BlinkDivide = BDtoggle.isOn;
        if(int.TryParse(FRfield.text, out int num1)) settingData.FrameRate = num1;
        if (int.TryParse(RFRfield.text, out int num2)) settingData.RicognizeFrameRate = num2;
        if (int.TryParse(MDBfield.text, out int num3)) settingData.MinimumDB = num3;
        settingData.EmotionJoy = Joyfield.text;
        settingData.EmotionAnger = Angerfield.text;
        settingData.EmotionSad = Sadfield.text;
        settingData.EmotionSuprise = Suprisefield.text;
    }

    public void DataOutSert()
    {
        BackGroundDropdown.value = settingData.BackGroundValue;
        ModelRotateSlider.value = settingData.ModelRotate;
        CameraPosXSlider.value = settingData.CameraPosX;
        CameraPosYSlider.value = settingData.CameraPosY;
        CameraPosZSlider.value = settingData.CameraPosZ;
        CameraRotXSlider.value = settingData.CameraRotX;
        CameraRotYSlider.value = settingData.CameraRotY;
        CameraRotZSlider.value = settingData.CameraRotZ;
        MTtoggle.isOn = settingData.MouthTracking;
        MTMtoggle.isOn = settingData.MouthTrckingMode;
        BDtoggle.isOn = settingData.BlinkDivide;
        FRfield.text = settingData.FrameRate.ToString();
        RFRfield.text = settingData.RicognizeFrameRate.ToString();
        MDBfield.text = settingData.MinimumDB.ToString();
        Joyfield.text = settingData.EmotionJoy;
        Angerfield.text = settingData.EmotionAnger;
        Sadfield.text = settingData.EmotionSad;
        Suprisefield.text = settingData.EmotionSuprise;
        ShapeKeyChange(Joyfield);
        ShapeKeyChange(Angerfield);
        ShapeKeyChange(Sadfield);
        ShapeKeyChange(Suprisefield);
    }

    public void OnDefaultButton()
    {
        settingData = new SettingData();
        DataOutSert();
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    //UIの内容説明をカーソルをあてると表示させる
    public void OnPointerEnter(int num)
    {
        tooltipPanel[num].SetActive(true);
    }

    public void OnPointerExit(int num)
    {
        tooltipPanel[num].SetActive(false);
    }
}