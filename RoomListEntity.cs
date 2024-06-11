using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
public class RoomListEntity : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text NumberofPlayersText;
    public Button loginButton;

    private string roomName;
    private bool roomState;
    void Start()
    {
        if (!roomState)
        {
            loginButton.gameObject.SetActive(true);
            loginButton.onClick.AddListener(() =>
            {
                if (PhotonNetwork.InLobby)
                {
                    PhotonNetwork.LeaveLobby();
                }
                PhotonNetwork.JoinRoom(roomName);
            });
        }
        else
        {
            loginButton.gameObject.SetActive(false);
        }
    }

    public void Initialize(string roomName, byte availablePlayer, byte maxPlayer, bool roomState)
    {
       this.roomState = roomState;

        this.roomName = roomName;

        roomNameText.text = roomName;

        NumberofPlayersText.text = availablePlayer + " / " + maxPlayer;
    }
}
