using System;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AnimatedButton : UIBehaviour, IPointerClickHandler
{
    [Serializable]
    public class ButtonClickedEvent : UnityEvent
    {
    }

    public bool interactable = true;

    [SerializeField] private ButtonClickedEvent m_onClick = new ButtonClickedEvent();

    private Animator animator;

    private bool blockInput;

    public ButtonClickedEvent onClick
    {
        get { return m_onClick; }
        set { m_onClick = value; }
    }

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || !interactable)
        {
            return;
        }

        if (!blockInput)
        {
            blockInput = true;
            Press();
            StartCoroutine(BlockInputTemporarily());
        }
    }

    private void Press()
    {
        if (!IsActive())
        {
            return;
        }

        animator.SetTrigger("Pressed");
        StartCoroutine(InvokeOnClickAction());
    }

    private IEnumerator InvokeOnClickAction()
    {
        yield return new WaitForSeconds(0.1f);
        m_onClick.Invoke();
    }

    private IEnumerator BlockInputTemporarily()
    {
        yield return new WaitForSeconds(0.5f);
        blockInput = false;
    }
}
