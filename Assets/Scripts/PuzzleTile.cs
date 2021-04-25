using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PuzzleTile : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int startIndex = 0;
    private Vector2 _lastPosition;
    private int currentIndex;
    private bool isLast = false;
    private GameManager gameManager;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void SetStartIndex(int _index)
    {
        startIndex = _index;
        SetCurrentIndex(_index);
    }

    public void SetCurrentIndex(int _index)
    {
        currentIndex = _index;
    }

    public int GetIndex()
    {
        return currentIndex;
    }

    public void SetLast(bool _isLast = true)
    {
        isLast = true;
        this.GetComponent<Image>().color = Color.clear;
        Destroy(this.GetComponentInChildren<Text>().gameObject);
    }

    public bool IsLast()
    {
        return isLast;
    }

    public bool IsInPosition()
    {
        return currentIndex == startIndex;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _lastPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Can't drag empty space
        if(this.IsLast()) { return; }

        // If is alredy moving
        if(gameManager.IsMoving()) { return; }

        Vector2 direction = eventData.position - _lastPosition;
        gameManager.Move(this, direction);
    }
}
