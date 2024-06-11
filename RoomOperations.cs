using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomOperations : MonoBehaviourPunCallbacks
{
    [Header("Login Panel")]
    public GameObject loginPanel;
    public InputField playerNameInput;

    //Create a Room
    //Join RAndom Room
    //Room List
    [Header("Selection Panel")]
    public GameObject selectionPanel;

    [Header("Room Creation panel")]
    public GameObject roomCreationPanel;
    public InputField roomName›nput;
    public InputField maxPlayer›nput;

    [Header("Random Room Entrance Panel")]
    public GameObject randomRoomEntarencePanel;

    [Header("Room List Panel")]
    public GameObject roomlistPanel;
    public GameObject roomListContent;
    public GameObject roomListRowPrefab;

    [Header("In-room Panel")]
    public GameObject InRoomPanel;

    public Button startGameButton;
    public GameObject playerListRowPrefab;

    private Dictionary<string, RoomInfo> roomCacheList;
    private Dictionary<string, GameObject> roomListElements;
    private Dictionary<int, GameObject> playerListElements;
    private void Awake()
    {
        //allows the master client and other users to see the same scene
        PhotonNetwork.AutomaticallySyncScene = true;

        roomCacheList = new Dictionary<string, RoomInfo>();
        roomListElements = new Dictionary<string, GameObject>();
    }
    public void SetActivePanel(string activePanel)
    {
        loginPanel.SetActive(activePanel.Equals(loginPanel.name));
        selectionPanel.SetActive(activePanel.Equals(selectionPanel.name));
        roomCreationPanel.SetActive(activePanel.Equals(roomCreationPanel.name));
        randomRoomEntarencePanel.SetActive(activePanel.Equals(randomRoomEntarencePanel.name));
        roomlistPanel.SetActive(activePanel.Equals(roomlistPanel.name));
        InRoomPanel.SetActive(activePanel.Equals(InRoomPanel.name));
    }

    public override void OnConnectedToMaster()
    {
        SetActivePanel(loginPanel.name);
    }

    public override void OnJoinedLobby()
    {
        roomCacheList.Clear();
        // ClearRoomListView();
    }

    public override void OnLeftLobby()
    {
        roomCacheList.Clear();
        // ClearRoomListView();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetActivePanel(loginPanel.name);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetActivePanel(loginPanel.name);
    }

    //If it doesn't connect to any room, a new room will open.
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room" + Random.RandomRange(0, 1000);

        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 6 };

        PhotonNetwork.CreateRoom(roomName, roomOptions, null);
    }

    public override void OnJoinedRoom()
    {
        //Create list entity and add the player list
        //Check player is ready

        roomCacheList.Clear();
        SetActivePanel(InRoomPanel.name);

        if (playerListElements == null)
        {
            playerListElements = new Dictionary<int, GameObject>();
        }

        foreach (Player item in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerListRowPrefab);

            entry.transform.SetParent(InRoomPanel.transform);

            entry.transform.localScale = Vector3.one;

            entry.transform.GetComponent<PlayerListEntity>().Initialize(item.ActorNumber, item.NickName);

            if (item.CustomProperties.TryGetValue("IsPlayerReady", out object isPlayerReady))
            {
                entry.GetComponent<PlayerListEntity>().SetPlayerReady((bool)isPlayerReady);
            }
            playerListElements.Add(item.ActorNumber, entry);


        }

        startGameButton.gameObject.SetActive(CheckPlayersReady());

        Hashtable props2 = new Hashtable()
        {
              {"PlayerLoadedLevel",false }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props2);
    }
    //clear own player list structs
    public override void OnLeftRoom()
    {
        SetActivePanel(selectionPanel.name);
        foreach (GameObject entry in playerListElements.Values)
        {
            Destroy(entry.gameObject);
        }
        playerListElements.Clear();
        playerListElements = null;
    }
    public void LocalPlayerPropertiesUpdated()
    {
        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }
    //Add the player in panel
    //Check Players 
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(playerListRowPrefab);

        entry.transform.SetParent(InRoomPanel.transform);

        entry.transform.localScale = Vector3.one;

        entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName);

        playerListElements.Add(newPlayer.ActorNumber, entry);
        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListElements[otherPlayer.ActorNumber].gameObject);
        playerListElements.Remove(otherPlayer.ActorNumber);
        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.MasterClient.ActorNumber == newMasterClient.ActorNumber)
        {
            startGameButton.gameObject.SetActive(CheckPlayersReady());
        }

    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListElements == null)
        {
            playerListElements = new Dictionary<int, GameObject>();
        }
        if (playerListElements.TryGetValue(targetPlayer.ActorNumber, out GameObject obj))// we question the existence of such a player
        {
            if (changedProps.TryGetValue("IsPlayerReady", out object isPlayerReady))
            {
                obj.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
        }
        startGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnCreatedRoom()
    {
        string roomName = roomName›nput.text;

        roomName = (roomName.Equals(string.Empty))? "Room" + Random.Range(100, 90000) : roomName;

        byte.TryParse(maxPlayer›nput.text, out byte MaxPlayer);

        MaxPlayer = (byte)Mathf.Clamp(MaxPlayer, 2, 8);

        RoomOptions options = new RoomOptions { MaxPlayers = MaxPlayer };

        PhotonNetwork.CreateRoom(roomName, options, null);
    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Clear view
        //Update Cache
        //Update list
        ClearRoomListView();
        UpdateCachedRoomList(roomList);
        UpdateRoomListView();

    }

    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {

            return false;
        }
        foreach (Player item in PhotonNetwork.PlayerList)
        {
            if (item.CustomProperties.TryGetValue("IsPlayerREady", out object isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return true;
                }

            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private void ClearRoomListView()
    {

        foreach (GameObject entry in roomListElements.Values)
        {
            Destroy(entry.gameObject);
        }
        roomListElements.Clear();
    }

    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        foreach (RoomInfo info in roomList)
        {
            if (!info.IsVisible || !info.IsOpen || info.RemovedFromList)
            {
                if (roomCacheList.ContainsKey(info.Name))
                {
                    roomCacheList.Remove(info.Name);
                }

                continue;
            }

            if (roomCacheList.ContainsKey(info.Name))
            {
                roomCacheList[info.Name] = info;

            }
            else
            {
                roomCacheList.Add(info.Name, info);
            }
        }
    }

    private void UpdateRoomListView()
    {
        foreach (RoomInfo info in roomCacheList.Values)
        {
            GameObject entry = Instantiate(roomListRowPrefab);

            entry.transform.SetParent(roomListContent.transform);

            entry.transform.localScale = Vector3.one;

            entry.GetComponent<RoomListEntity>().Initialize(info.Name, (byte)info.PlayerCount, (byte)info.MaxPlayers, (info.MaxPlayers == info.PlayerCount));
            roomListElements.Add(info.Name, entry);
        }
    }
}
