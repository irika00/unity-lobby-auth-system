using System.Collections; 
using System.Collections.Generic;
using UnityEngine;

public class Panel : MonoBehaviour
{
    [SerializeField] private string id = "";
    public string ID { get { return id; } }

    [SerializeField] private RectTransform container = null; 

    private bool initialized = false;
    public bool IsInitialized { get { return initialized; } }

    private bool isOpen = false;
    public bool IsOpen { get { return isOpen; } } 

    private Canvas canvas = null;
    public Canvas Canvas { get { return canvas; } set { canvas = value; } }

    public virtual void Awake()
    {
        Initialize();
    }

    public virtual void Initialize()
    {
        if (initialized) { return; }
        initialized = true;
        Close();
    }

    public virtual void Open()
    {
        if (!initialized) { Initialize(); }
        transform.SetAsLastSibling();
        if (container != null)
            container.gameObject.SetActive(true);
        isOpen = true;
    }

    public virtual void Close()
    {
        if (!initialized) { Initialize(); }
        if (container != null)
            container.gameObject.SetActive(false);
        isOpen = false;
    }
}
