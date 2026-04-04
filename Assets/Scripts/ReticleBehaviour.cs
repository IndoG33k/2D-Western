using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class ReticleBehaviour : MonoBehaviour
{

    public enum ReticleStartMode
    {
        WaitForExternalActivate,
        DelaySecondsOnStart
    }

    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject reticleImage;
    [SerializeField] private ReticleStartMode startMode = ReticleStartMode.WaitForExternalActivate;
    [SerializeField] private float reticleDelay = 5f;

    private bool followMouse;
    
    void Start()
    {
        reticleImage.SetActive(false);
        followMouse = false;

        if (startMode == ReticleStartMode.DelaySecondsOnStart)
        {
            StartCoroutine(ShowReticleAfterDelay());
        }
    }

    public void PrepareForRound()
    {
        followMouse = false;
        if (reticleImage != null)
            reticleImage.SetActive(false);
    }

    public void ActivateReticle()
    {
        followMouse = true;
        if (reticleImage != null)
            reticleImage.SetActive(true);
    }

    private IEnumerator ShowReticleAfterDelay()
    {
        yield return new WaitForSeconds(reticleDelay);
        ActivateReticle();
    }

    // private IEnumerator ShowReticle()
    // {
    //     yield return new WaitForSeconds(reticleDelay);
    //     reticleImage.SetActive(true);
    //     SetFollowMouse();
    // }

    // private void SetFollowMouse()
    // {
    //     followMouse = true;
    // }

    private void LateUpdate()
    {
        if (!followMouse)
        {
            return;
        }

        Vector2 screenPoint = Mouse.current != null
        ? Mouse.current.position.ReadValue()
        : Vector2.zero;

        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
              canvas.transform as RectTransform,
              screenPoint,
              canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
              out localPoint))
        {
            reticleImage.transform.localPosition = localPoint;
        }
    }
}
