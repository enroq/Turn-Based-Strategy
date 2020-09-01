using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrivateMessageButtonRepresenation : MonoBehaviour
{
    [SerializeField]
    Text m_NotificationLabel;

    GameObject m_PrivateMessageObject;
    PrivateMessageInstance m_MessageInstance;

    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => MaximizePrivateMessage());
    }

    void MaximizePrivateMessage()
    {
        PrivateMessageHandler.MaximizeMessage(m_MessageInstance.AccountToFrom.Identity);
    }

    internal void SetPrivateMessageObject(GameObject obj)
    {
        if (obj != null)
        {
            m_PrivateMessageObject = obj;
            m_MessageInstance = m_PrivateMessageObject.GetComponent<PrivateMessageInstance>();
            m_NotificationLabel.text = m_MessageInstance.AccountToFrom.Username;
        }
        else
            Debug.LogError("Attempting To Set Private Message Object To Null");
    }

    internal GameObject GetPrivateMessageObject()
    {
        return m_PrivateMessageObject;
    }
}
