using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] GameObject msgPanel;
    [SerializeField] Text msgText;
    [SerializeField] GameObject gameOverPanel;

    //[Space(10)]
    //public Transform cameraTransform;

    [Space(10)]
    public Button accelerometerButton;
    public Button physicalButton;
    public Slider movePixelSlider;
    public Text movePixelSliderText;

    private void Awake()
    {
        instance = this;

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        ChangeInput(false);

        if (Application.isEditor) movePixelSlider.value = movePixelSlider.maxValue;

        UpdateSliderValue();
    }

    public void UpdateSliderValue()
    {
        movePixelSliderText.text = movePixelSlider.value.ToString();

        GameManager.instance.player.GetComponent<PlayerManager>().movePixelValue = movePixelSlider.value / 100f;
    }

    public async void ShowUIMessage(string msgString)
    {
        //you found a flame!
        //you found the key! now, find the exit
        //congratulations \n you passed!

        msgPanel.SetActive(true);
        msgText.text = msgString;

        await Task.Delay(2000);

        msgText.text = "";
        msgPanel.SetActive(false);
    }

    public void ChangeInput(bool anyValue)
    {
        if (anyValue)
        {
            physicalButton.gameObject.GetComponent<Image>().color = Color.green;
            accelerometerButton.gameObject.GetComponent<Image>().color = Color.white;

            PlayerManager.instance.movePhysically = true;
            //GameManager.instance.arrowObject.SetActive(true);
        }
        else
        {
            accelerometerButton.gameObject.GetComponent<Image>().color = Color.green;
            physicalButton.gameObject.GetComponent<Image>().color = Color.white;

            PlayerManager.instance.movePhysically = false;

            //cameraTransform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.Linear);
            //cameraTransform.DOMove(new Vector3(0, 0, -10), 0.3f);
            //GameManager.instance.arrowObject.SetActive(false);
        }
    }

    public void OpenGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        GameManager.instance.player.SetActive(false);
        GameManager.instance.gridMap.SetActive(false);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
