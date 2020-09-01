using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectClickHandler : MonoBehaviour
{
	void Start ()
    {
        EventSink.SingleClickEvent += EventSink_MouseClickEvent;
        EventSink.DoubleClickEvent += EventSink_DoubleClickEvent;
	}

    private void EventSink_DoubleClickEvent(MouseClickEventArgs args)
    {
        HandleDoubleClick(args.TransformHit, args.MouseIndexClicked);
    }

    private void EventSink_MouseClickEvent(MouseClickEventArgs args)
    {
        HandleSingleClick(args.TransformHit, args.MouseIndexClicked);
    }

    void HandleSingleClick(Transform transform, int index)
    {
        switch(index)
        {
            case 0:
                {
                    ProcessSingleLeftClick(transform);
                    break;
                }
            case 1:
                {
                    goto default;
                }
            default:
                {
                    Debug.Log("Mouse Click Event Index Unhandled: " + index);
                    break;
                }
        }
    }

    void HandleDoubleClick(Transform transform, int index)
    {
        switch (index)
        {
            case 0:
                {
                    ProcessDoubleLeftClick(transform);
                    break;
                }
            case 1:
                {
                    goto default;
                }
            default:
                {
                    Debug.Log("Mouse Click Event Index Unhandled: " + index);
                    break;
                }
        }
    }

    void ProcessSingleLeftClick(Transform transform)
    {
        Component[] componets = transform.gameObject.GetComponents<MonoBehaviour>();

        for(int i = 0; i < componets.Length; i++)
            ProcessComponentSingleClick(componets[i]);
    }

    void ProcessDoubleLeftClick(Transform transform)
    {
        Component[] componets = transform.gameObject.GetComponents<MonoBehaviour>();

        for (int i = 0; i < componets.Length; i++)
            ProcessComponentDoubleClick(componets[i]);
    }

    void ProcessComponentSingleClick(Component component)
    {
        switch(component.GetType().ToString().ToLowerInvariant())
        {
            case "gameboardtile":
                {
                    ((GameBoardTile)component).HandleSingleClickEvent();
                    break;
                }
            case "gamepiece":
                {
                    ((GamePiece)component).SelectGamePiece();
                    break;
                }
            default:
                {
                    Debug.Log
                        ("Attempting To Process Unspecified Component: " + component.GetType().ToString().ToLowerInvariant());
                    break;
                }
        }
    }

    void ProcessComponentDoubleClick(Component component)
    {
        Debug.LogFormat("{0} Double Clicked..", component.gameObject.name);

        switch (component.GetType().ToString().ToLowerInvariant())
        {
            case "gameboardtile":
                {
                    ((GameBoardTile)component).HandleDoubleClick();
                    break;
                }

            case "gamepiece":
                {
                    ((GamePiece)component).HandleDoubleClick();
                    break;
                }
            default:
                {
                    Debug.Log
                        ("Attempting To Process Unspecified Component: " + component.GetType().ToString().ToLowerInvariant());
                    break;
                }
        }
    }
}
