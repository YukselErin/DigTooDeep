using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Core;
using System.Net.Security;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using System.Threading.Tasks;
public class TestRelay : MonoBehaviour
{
    public static TestRelay Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    async void Start()
    {/*
        await UnityServices.InitializeAsync();
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in at:" + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
*/
    }

    // Update is called once per frame
    public async Task<string> CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            RelayServerData relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
            return null;
        }
    }
    void startHostDelay()
    {
        NetworkManager.Singleton.StartHost();
    }
    public async void JoinRelay(string code)
    {

        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            Debug.Log("join allocate");
            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            Debug.Log("start client");

        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }
}
