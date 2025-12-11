using UnityEngine;

public class WindowClamp : MonoBehaviour
{
    private RectTransform rectTransform;
    private RectTransform canvasRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        ClampToScreen();
    }

    void ClampToScreen()
    {
        Vector3 pos = rectTransform.localPosition;

        // Half sizes
        float halfWidth = rectTransform.rect.width / 2f;
        float halfHeight = rectTransform.rect.height / 2f;

        float canvasHalfWidth = canvasRect.rect.width / 2f;
        float canvasHalfHeight = canvasRect.rect.height / 2f;

        // Clamp X and Y so window stays inside canvas
        pos.x = Mathf.Clamp(pos.x, -canvasHalfWidth + halfWidth, canvasHalfWidth - halfWidth);
        pos.y = Mathf.Clamp(pos.y, -canvasHalfHeight + halfHeight, canvasHalfHeight - halfHeight);

        rectTransform.localPosition = pos;
    }
}