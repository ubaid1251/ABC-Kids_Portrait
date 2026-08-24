using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class DragAlpha : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    private RectTransform _rect;
    private Vector2 _pos;
    private GameObject _child;
    private Image _my;
    private Canvas _myCan;

    [Header("Correct Drop Target")]
    public RectTransform moveTo;

    private bool _completed = false;
    private bool _up = false;

    private void Start()
    {
        _my = GetComponent<Image>();
        _myCan = GetComponent<Canvas>();
        _rect = GetComponent<RectTransform>();

        _child = transform.GetChild(0).gameObject;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _up = false;

        // Save original position
        _pos = _rect.anchoredPosition;

        // Bring object to front
        _myCan.sortingOrder = 10;

        _child.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Keep the working portrait drag
        _rect.anchoredPosition += eventData.delta / _myCan.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _up = true;

        // Check whether the dragged object is actually over
        // the assigned moveTo object.
        bool correctDrop = IsDraggedObjectOverTarget();

        Debug.Log(
            gameObject.name +
            " | Correct Drop = " +
            correctDrop
        );

        // =====================================================
        // CORRECT DROP
        // =====================================================

        if (correctDrop && !_completed)
        {
            Vibration.Vibrate(50);

            if (MatchingManager.Instance.myS.enabled)
            {
                MatchingManager.Instance.myS.PlayOneShot(
                    MatchingManager.Instance.correct
                );
            }

            if (MatchingManager.Instance.myS.enabled)
            {
                MatchingManager.Instance.myS.PlayOneShot(
                    MatchingManager.Instance.voiceOver[
                        Random.Range(
                            0,
                            MatchingManager.Instance.voiceOver.Length
                        )
                    ]
                );
            }

            _completed = true;

            _my.raycastTarget = false;

            // Move exactly to target
            _rect.DOMove(
                moveTo.position,
                0.5f
            );

            IndicationHandler.Instance.timeSinceLastInput = 0;

            // Remove indication
            RectTransform indication =
                transform.GetChild(1).GetComponent<RectTransform>();

            IndicationHandler.Instance.allIndi.Remove(indication);

            // Scale down
            _rect.DOScale(
                Vector3.zero,
                0.5f
            ).OnComplete(() =>
            {
                MatchingManager.Instance.matched++;

                gameObject.SetActive(false);

                if (MatchingManager.Instance.size ==
                    MatchingManager.Instance.matched)
                {
                    MatchingManager.Instance.EndAnim();
                }
            });
        }

        // =====================================================
        // WRONG DROP
        // =====================================================

        else
        {
            WrongDrop();
        }
    }

    // =========================================================
    // CHECK IF DRAGGED OBJECT IS OVER moveTo
    // =========================================================

    private bool IsDraggedObjectOverTarget()
    {
        if (moveTo == null)
        {
            Debug.LogError(
                gameObject.name +
                " : moveTo is NOT assigned!"
            );

            return false;
        }

        Camera cam = null;

        // For Screen Space - Camera / World Space Canvas
        if (_myCan.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            cam = _myCan.worldCamera;
        }

        // -----------------------------------------------------
        // Get dragged object's CENTER in screen coordinates
        // -----------------------------------------------------

        Vector2 draggedCenter =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                _rect.position
            );

        // -----------------------------------------------------
        // Get moveTo's four WORLD corners
        // -----------------------------------------------------

        Vector3[] corners = new Vector3[4];

        moveTo.GetWorldCorners(corners);

        // Convert corners to SCREEN coordinates
        Vector2 bottomLeft =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[0]
            );

        Vector2 topLeft =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[1]
            );

        Vector2 topRight =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[2]
            );

        Vector2 bottomRight =
            RectTransformUtility.WorldToScreenPoint(
                cam,
                corners[3]
            );

        // -----------------------------------------------------
        // Calculate target rectangle
        // -----------------------------------------------------

        float minX = Mathf.Min(
            bottomLeft.x,
            topLeft.x,
            topRight.x,
            bottomRight.x
        );

        float maxX = Mathf.Max(
            bottomLeft.x,
            topLeft.x,
            topRight.x,
            bottomRight.x
        );

        float minY = Mathf.Min(
            bottomLeft.y,
            topLeft.y,
            topRight.y,
            bottomRight.y
        );

        float maxY = Mathf.Max(
            bottomLeft.y,
            topLeft.y,
            topRight.y,
            bottomRight.y
        );

        // -----------------------------------------------------
        // Check dragged object's CENTER
        // is inside moveTo
        // -----------------------------------------------------

        bool inside =
            draggedCenter.x >= minX &&
            draggedCenter.x <= maxX &&
            draggedCenter.y >= minY &&
            draggedCenter.y <= maxY;

        Debug.Log(
            "Dragged Center: " +
            draggedCenter +
            " | Target Rect: " +
            minX + "," +
            minY +
            " -> " +
            maxX + "," +
            maxY +
            " | Inside: " +
            inside
        );

        return inside;
    }

    // =========================================================
    // WRONG DROP ANIMATION
    // =========================================================

    private void WrongDrop()
    {
        _child.SetActive(false);

        _my.raycastTarget = false;

        Vibration.Vibrate(50);

        if (MatchingManager.Instance.myS.enabled)
        {
            MatchingManager.Instance.myS.PlayOneShot(
                MatchingManager.Instance.wrong
            );
        }

        // Shake left
        _rect.DORotate(
            new Vector3(0, 0, -10),
            0.1f
        ).OnComplete(() =>
        {
            // Shake right
            _rect.DORotate(
                new Vector3(0, 0, 10),
                0.1f
            ).OnComplete(() =>
            {
                // Back to normal
                _rect.DORotate(
                    new Vector3(0, 0, 0),
                    0.1f
                ).OnComplete(() =>
                {
                    // Return to original position
                    _rect.DOAnchorPos(
                        _pos,
                        0.5f
                    ).OnComplete(() =>
                    {
                        _myCan.sortingOrder = 5;

                        _my.raycastTarget = true;
                    });
                });
            });
        });
    }
}