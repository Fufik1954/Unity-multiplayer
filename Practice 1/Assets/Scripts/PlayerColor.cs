using UnityEngine;
using Unity.Netcode;
using System.Globalization;

public class PlayerColor : NetworkBehaviour
{
    void Start()
    {
        if (IsOwner)
        {
            GetComponent<MeshRenderer>().material.color = Color.green; // свой игрок
        }
        else
        {
            GetComponent<MeshRenderer>().material.color = Color.red; // чужой игрок
        }
    }
}