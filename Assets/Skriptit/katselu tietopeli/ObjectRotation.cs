using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ObjectRotation : MonoBehaviour
{
    [SerializeField] private InputAction pressed, axis;
    private Transform cam;

    [SerializeField] private float speed = 1f;
    [SerializeField] private bool inverted;

    [SerializeField] private RawImage targetImage;       // RawImage, jonka päällä rotaatio alkaa
    [SerializeField] private GraphicRaycaster raycaster; // Canvasin raycaster
    

    private Vector2 rotation;
    private bool rotateAllowed;

    private void Awake()
    {
        cam = Camera.main.transform;
        pressed.Enable();
        axis.Enable();
    }

    private void Update()
    {
        
        if (Mouse.current.leftButton.wasPressedThisFrame && !rotateAllowed && IsPointerOverRawImage())
        {
            StartCoroutine(Rotate());
        }

     
        if (rotateAllowed)
        {
            rotation = Mouse.current.delta.ReadValue(); 
        }
        else
        {
            rotation = Vector2.zero;
        }
    }

    private bool IsPointerOverRawImage()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        foreach (var res in results)
            if (res.gameObject == targetImage.gameObject)
                return true;

        return false;
    }

    private IEnumerator Rotate()
    {
        rotateAllowed = true;

        while (rotateAllowed)
        {
            Vector2 rot = rotation * speed * Time.deltaTime;

            transform.Rotate(Vector3.up * (inverted ? 1 : -1), rot.x, Space.World);
            transform.Rotate(cam.right * (inverted ? -1 : 1), rot.y, Space.World);

            if (!Mouse.current.leftButton.isPressed)
                rotateAllowed = false;

            yield return null;
        }
    }
}