using UnityEngine;
using UnityEngine.UI;

public class HowToPlayCarousel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image displayImage;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Slides")]
    [SerializeField] private Sprite[] slides;
    [SerializeField] private int startIndex;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;

    private int _index;

    private void Awake()
    {
        if (panelRoot == null)
            panelRoot = gameObject;

        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();

        _index = Mathf.Clamp(startIndex, 0, Mathf.Max(0, (slides?.Length ?? 0) - 1));

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

    public void Next()
    {
        int max = Mathf.Max(0, (slides?.Length ?? 0) - 1);
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
        int count = slides?.Length ?? 0;
        int max = Mathf.Max(0, count - 1);
        _index = Mathf.Clamp(_index, 0, max);

        if (displayImage != null)
        {
            displayImage.sprite = count > 0 ? slides[_index] : null;
            displayImage.enabled = displayImage.sprite != null;
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

