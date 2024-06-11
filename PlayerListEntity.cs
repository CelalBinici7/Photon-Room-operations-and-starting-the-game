using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerListEntity : MonoBehaviour
{
    public TMP_Text playerName;
    public Button playerReadyButton;
    public Image playerSprite;
    private int playerId;
    private bool isReady;

    void Start()
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber != playerId)
        {
            playerReadyButton.gameObject.SetActive(false);
        }
        else
        {
            playerReadyButton.onClick.AddListener(() =>
            {
                isReady = !isReady;
                SetPlayerReady(isReady);
                Hashtable props = new Hashtable()
            {
                {"IsPlayerReady",isReady}

            };

                PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                if (PhotonNetwork.IsMasterClient)
                {
                    FindAnyObjectByType<RoomOperations>().LocalPlayerPropertiesUpdated();
                }

            });
            

        }
    }

    public void Initialize(int playerId, string playerName)
    {
       this.playerId = playerId;
       this. playerName.text = playerName;
    }
    public void SetPlayerReady(bool playerReady)
    {
        playerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "OK !" : "Ready";
        playerSprite.enabled = playerReady;
    }
}
