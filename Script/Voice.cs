using System.Collections;
using System.Collections.Generic;
using Adrenak.UniVoice;
using UnityEngine;

using Adrenak.UniVoice.UniMicInput;
using Adrenak.UniVoice.AudioSourceOutput;
using System;
using Unity.Netcode;
using System.Linq;
using Adrenak.UniVoice.Samples;

public class Voice : NetworkBehaviour
{
    public static Voice Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

        }
    }
    public ChatroomAgent agent;
    public ntwrk ntwrkin;
    void Start()
    {
        ntwrkin = gameObject.AddComponent<ntwrk>();
        agent = new ChatroomAgent(
           ntwrkin,
            new UniVoiceUniMicInput(0, 16000, 100),
            new UniVoiceAudioSourceOutput.Factory());
        agent.Network.OnCreatedChatroom += () =>
        {
            Debug.Log($"Chatroom created!\nYou are Peer ID {agent.Network.OwnID}");

        };

        agent.Network.OnChatroomCreationFailed += ex =>
        {
            Debug.Log("Chatroom creation failed");
        };

        agent.Network.OnClosedChatroom += () =>
        {
            Debug.Log("You closed the chatroom! All peers have been kicked");

        };

        agent.Network.OnJoinedChatroom += id =>
        {
            Debug.Log("Joined chatroom ");
            Debug.Log("You are Peer ID " + id);

        };

        agent.Network.OnPeerJoinedChatroom += id =>
        {
            GameObject.Find("UniVoice Peer #" + id.ToString()).AddComponent<PlayerVoiceSource>().AttachToPlayerPrefab((ulong)id);
        };

        agent.Network.OnChatroomJoinFailed += ex =>
        {
            Debug.Log(ex);
        };

        agent.Network.OnLeftChatroom += () =>
        {
            Debug.Log("You left the chatroom");


        };




    }
    public bool actv = false;
    // Update is called once per frame
    public void ActivateVoice()
    {
        if (!actv)
        {
            actv = true;
            GameStateController.Instance.GameRestartEvent.AddListener(ActivateVoice);

        }

        if (IsHost)
        {
            HostChatroom();
        }
        else if (IsClient)
        {
            JoinChatroom();
        }

        if (agent == null || agent.PeerOutputs == null) return;
    }

    void HostChatroom()
    {
        var roomName = "1";
        agent.Network.HostChatroom(roomName);
    }

    void JoinChatroom()
    {

        var roomName = "1";
        if (string.IsNullOrEmpty(roomName))
            agent.Network.JoinChatroom();
        else
            agent.Network.JoinChatroom(roomName);
    }
}
public class ntwrk : NetworkBehaviour, IChatroomNetwork
{
    void init()
    {
        CachedPeerIDs = new List<short>();
        AllPeerIDs = new List<short>();
        NetworkManager.OnConnectionEvent += (networkManager, connectionEventData) => { HandleConnectionEvent(networkManager, connectionEventData); };

    }
    void HandleConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
    {
        if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
        {
            short dcID = (short)connectionEventData.ClientId;
            AllPeerIDs.Remove(dcID);
            CachedPeerIDs.Remove(dcID);
            OnPeerLeftChatroom.Invoke(dcID);
            RemoveIDRpc(dcID);
        }
        if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
        {
            AllPeerIDs.Clear();
            CachedPeerIDs.Clear();
            OnLeftChatroom.Invoke();
        }
    }
    [Rpc(SendTo.NotServer)]
    void RemoveIDRpc(short id)
    {
        AllPeerIDs.Remove(id);
        CachedPeerIDs.Remove(id);
        OnPeerLeftChatroom.Invoke(id);
    }

    public short OwnID => (short)NetworkManager.Singleton.LocalClientId;

    public List<short> PeerIDs => CachedPeerIDs;
    public List<short> CachedPeerIDs;
    public List<short> AllPeerIDs;

    public event Action OnCreatedChatroom;
    public event Action<Exception> OnChatroomCreationFailed;
    public event Action OnClosedChatroom;
    public event Action<short> OnJoinedChatroom;
    public event Action<Exception> OnChatroomJoinFailed;
    public event Action OnLeftChatroom;
    public event Action<short> OnPeerJoinedChatroom;
    public event Action<short> OnPeerLeftChatroom;
    public event Action<short, ChatroomAudioSegment> OnAudioReceived;
    public event Action<short, ChatroomAudioSegment> OnAudioSent;

    public void CloseChatroom(object data = null)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void HostChatroom(object data = null)
    {
        init();
        if (IsHost)
        {
            AllPeerIDs.Add((short)NetworkManager.Singleton.LocalClientId);
            // voiceIDtoNetworkID.Add(0, OwnerClientId);
            // RequestIDRpc(RpcTarget.Server);
            // RequestPeerDictRpc();

            //RenewIDRpc();

            OnCreatedChatroom.Invoke();
        }
        else
        {
            Debug.Log("only server can be host");
            throw new NotImplementedException();
        }
    }

    [Rpc(SendTo.Server)]
    void AddIDRpc(short id)
    {
        if (!AllPeerIDs.Contains(id))
        {
            AllPeerIDs.Add(id);
            CachedPeerIDs.Add(id);
            OnPeerJoinedChatroom.Invoke(id);
            foreach (var tempID in CachedPeerIDs)
            {
                if (id == tempID)
                {
                    SendAllIDRpc(AllPeerIDs.ToArray(), RpcTarget.Single((ulong)id, RpcTargetUse.Temp));
                }
                else
                {
                    SendIncrementalIDRpc(id, RpcTarget.Single((ulong)id, RpcTargetUse.Temp));

                }
            }

        }
        else
        {
            Debug.Log("Already has ID!");
        }

    }
    [Rpc(SendTo.SpecifiedInParams)]

    void SendAllIDRpc(short[] id, RpcParams rpcSendParams)
    {
        AllPeerIDs = new List<short>(id);
        CachedPeerIDs = new List<short>(AllPeerIDs);
        CachedPeerIDs.Remove((short)NetworkManager.Singleton.LocalClientId);
        foreach (var tempid in CachedPeerIDs)
        {
            OnPeerJoinedChatroom.Invoke(tempid);

        }
        OnJoinedChatroom?.Invoke(OwnID);

    }

    [Rpc(SendTo.SpecifiedInParams)]

    void SendIncrementalIDRpc(short id, RpcParams rpcSendParams)
    {
        AllPeerIDs.Add(id);
        CachedPeerIDs.Add(id);
        OnPeerJoinedChatroom.Invoke(id);
    }
    public void JoinChatroom(object data = null)
    {
        if (AllPeerIDs != null)
        {
            if (AllPeerIDs.Contains((short)NetworkManager.Singleton.LocalClientId))
            {
                Debug.LogError("Already joined with this ID!");
            }
        }


        AddIDRpc((short)NetworkManager.Singleton.LocalClientId);

    }


    public void LeaveChatroom(object data = null)
    {
        throw new NotImplementedException();
    }

    public void SendAudioSegment(short peerID, ChatroomAudioSegment data)
    {
        ulong id = (ulong)peerID;
        var segmentIndex = data.segmentIndex;
        var frequency = data.frequency;
        var channelCount = data.channelCount;
        var samples = data.samples;
        //Debug.Log("audiosend");
        sendingAudioRpc(segmentIndex, frequency, channelCount, samples, RpcTarget.Single(id, RpcTargetUse.Persistent));
        OnAudioSent?.Invoke(peerID, data);

    }
    RpcSendParams param;

    [Rpc(SendTo.SpecifiedInParams)]
    public void sendingAudioRpc(int index, int freq, int channelCount, float[] samples, RpcParams rpcSendParams)
    {
        OnAudioReceived?.Invoke((short)rpcSendParams.Receive.SenderClientId, new ChatroomAudioSegment
        {
            segmentIndex = index,
            frequency = freq,
            channelCount = channelCount,
            samples = samples
        });
    }
}