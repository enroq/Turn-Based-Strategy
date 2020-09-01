using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RotationSelectorBehavior : MonoBehaviour
{
    [SerializeField]
    List<Button> m_DirectionButtons;

    Quaternion m_DeltaRotation;

    private void Start()
    {
        foreach(Button button in m_DirectionButtons)
        {
            button.onClick.AddListener(() => SetDirection(button));
        }
    }

    void SetDirection(Button button)
    {
        m_DeltaRotation = Quaternion.LookRotation
            (button.transform.position - transform.parent.transform.position);

        m_DeltaRotation.z = 0;
        m_DeltaRotation.x = 0;

        transform.parent.transform.rotation = m_DeltaRotation;

        GamePiece piece = transform.parent.GetComponent<GamePiece>();
        piece.Handler.HandleRotationChange(piece);

        if (piece.UnderLocalControl || piece.TestMode)
            TurnStateHandler.FinishStep();

        gameObject.SetActive(false);
    }
}
