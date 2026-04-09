using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayCarousel : MonoBehaviour
{
    [System.Serializable]
    public class HowToPlaySlide
    {
        public Sprite image;

        [TextArea(2, 6)]
        public string tip;
    }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image displayImage;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Slides")]
    [SerializeField] private HowToPlaySlide[] slides;
    [SerializeField] private int startIndex;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;
    [SerializeField] private bool resetToFirstSlideOnClose = true;
    [SerializeField] private bool hideTipWhenEmpty = true;

    private int _index;

    private int SlideCount => slides?.Length ?? 0;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();

        _index = Mathf.Clamp(startIndex, 0, Mathf.Max(0, SlideCount - 1));

        if (hideOnStart)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        else if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        Refresh();
    }

    public void Hide()
    {
        if (panelRoot != null && !panelRoot.activeSelf)
            panelRoot.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Wire the Back button to this. Hides the tutorial overlay so the main menu underneath is usable again.
    /// </summary>
    public void CloseTutorial()
    {
        if (resetToFirstSlideOnClose)
        {
            int max = Mathf.Max(0, SlideCount - 1);
            _index = Mathf.Clamp(startIndex, 0, max);
        }

        Hide();
    }

    public void Next()
    {
        int max = Mathf.Max(0, SlideCount - 1);
        if (_index >= max)
        {
            Refresh();
            return;
        }

        _index++;
        Refresh();
    }

    public void Prev()
    {
        if (_index <= 0)
        {
            Refresh();
            return;
        }

        _index--;
        Refresh();
    }

    private void Refresh()
    {
        int count = SlideCount;
        int max = Mathf.Max(0, count - 1);
        _index = Mathf.Clamp(_index, 0, max);

        if (displayImage != null)
        {
            Sprite s = count > 0 && slides[_index] != null ? slides[_index].image : null;
            displayImage.sprite = s;
            displayImage.enabled = s != null;
        }

        if (tipText != null)
        {
            string tip = count > 0 && slides[_index] != null ? slides[_index].tip : string.Empty;
            tipText.text = tip ?? string.Empty;
            if (hideTipWhenEmpty)
                tipText.gameObject.SetActive(!string.IsNullOrEmpty(tip));
        }

        bool hasSlides = count > 0;
        bool canGoLeft = hasSlides && _index > 0;
        bool canGoRight = hasSlides && _index < max;

        if (leftButton != null)
            leftButton.interactable = canGoLeft;
        if (rightButton != null)
            rightButton.interactable = canGoRight;
    }
}
