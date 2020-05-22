using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIElementDragger : MonoBehaviour
{
    public const string DRAGGABLE_TAG = "UIDraggable";

    private bool dragging = false;

    private Vector2 originalPosition;

    private Transform objectToDrag;
    private SVGImage objectToDragImage;
    public ToolTypes tooltype = ToolTypes.NONE;
    public FootTypes fooltype = FootTypes.NONE;

    List<RaycastResult> hitObjects = new List<RaycastResult>();

    public enum ToolTypes
    {
        NONE,
        SEED,
        GLOVES,

    }
    public enum FootTypes
    {
        NONE,
        FIELD,

    }


    #region Monobehaviour API

    void Start()
    {
        
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // press Mouse0
        {
            objectToDrag = GetDraggableTransformUnderMouse(); // select it

            if(objectToDrag != null)
            {
                dragging = true;

                objectToDrag.SetAsLastSibling();

                originalPosition = objectToDrag.position;
                objectToDragImage = objectToDrag.GetComponent<SVGImage>();
                objectToDragImage.raycastTarget = false;
            }
        }

        if (dragging)
        {
            objectToDrag.position = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0)) // release Mouse0
        {
            if(objectToDrag != null)
            {
                Transform objectToReplace = GetDraggableTransformUnderMouse();

                

                if (objectToReplace != null)
                {
                    switch (objectToReplace.name)
                    {
                        case "Field":
                            Debug.Log("Put on Field");
                            fooltype = FootTypes.FIELD;
                            NET_DragLogic.Instance.SetDrag(objectToDrag.gameObject,objectToReplace.gameObject);
                            break;
                        default:
                            Debug.LogError("Put on Unkown");
                            fooltype = FootTypes.NONE;
                            break;

                    }
                    if(tooltype == ToolTypes.SEED && fooltype == FootTypes.FIELD)
                    {
                        GameObject go = objectToReplace.GetChild(0).gameObject;
                        objectToDrag.position = go.transform.position;
                        Debug.Log("Put SEED on Field done!");
                        fooltype = FootTypes.NONE;
                        tooltype = ToolTypes.NONE;
                        // lunch grow
                    }
                    /*
                    objectToDrag.position = objectToReplace.position;
                    objectToReplace.position = originalPosition;
                    */
                } else
                {
                    Debug.LogError("Put on Nothing");
                    fooltype = FootTypes.NONE;

                    objectToDrag.position = originalPosition; // push back
                    //objectToDrag.position = Input.mousePosition; // put anywhere
                }

                objectToDragImage.raycastTarget = true;
                objectToDrag = null;
            }

            dragging = false;
        }
    }
    #endregion

    private GameObject GetObjectUnderMouse()
    {
        var pointer = new PointerEventData(EventSystem.current);

        pointer.position = Input.mousePosition;

        EventSystem.current.RaycastAll(pointer, hitObjects);

        if (hitObjects.Count <= 0) return null;

        return hitObjects.First().gameObject;
    }

    private Transform GetDraggableTransformUnderMouse()
    {
        GameObject clickedObject = GetObjectUnderMouse();
        Debug.Log(clickedObject.name);
        switch (clickedObject.name)
        {
            case "SEED":
                Debug.LogError("Seed Collected");
                tooltype = ToolTypes.SEED;
                break;
            case "Field":
                if(tooltype == ToolTypes.NONE)
                {
                    clickedObject = null;                               
                }
                Debug.LogError("Field NOT Collected");
                fooltype = FootTypes.FIELD;
                break;
            case "FlowerPrefab(Clone)":
                if (tooltype == ToolTypes.NONE)
                {
                    clickedObject = null;
                }
                Debug.LogError("Grown Plants NOT Collected");
                fooltype = FootTypes.NONE;
                break;
            case "GLOVES":
                Debug.LogError("Gloves Collected");
                tooltype = ToolTypes.GLOVES;
                break;
            case "BAGSEED":
                Debug.LogError("SEED BAG Opened");
                tooltype = ToolTypes.NONE;
                clickedObject = null;
                break;

            default:
                Debug.LogError("unkownen Collected");
                tooltype  = ToolTypes.NONE;
                break;

        }

        if(clickedObject != null && clickedObject.tag == DRAGGABLE_TAG)
        {
            return clickedObject.transform;
        }

        return null;
    }
}
