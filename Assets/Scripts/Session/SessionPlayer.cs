using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Netcode;


public class SessionPlayer : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private MeshRenderer meshRenderer = null;
    private CharacterController controller = null;
    private string _colorHex = "";
    private string _id = "";

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError("[SessionPlayer] CharacterController is missing!");
        }
        if (meshRenderer == null)
        {
            Debug.LogError("[SessionPlayer] MeshRenderer is not assigned!");
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            controller.Move(new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime * moveSpeed);

        }

    }

    [Rpc(target: SendTo.Everyone)]
    public void ApplyDataRpc(string id, string colorHex)
    {
        ColorUtility.TryParseHtmlString(colorHex, out Color color);
        meshRenderer.material.color = color;
        _colorHex = colorHex;
        _id = id;
    }

    public void ApplyDataRpc()
    {
        ApplyDataRpc(_id, _colorHex);
    }

     
    
}

