﻿using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityWebSocket;
using QFramework;

public interface IWebSocketSystem : ISystem {
    void Connect(string url);
    void SendMsgToServer(string msg);
    void CloseConnect();
}

public class WebSocketSystem : AbstractSystem, IWebSocketSystem {
    private string address;
    private int receiveCount;
    private IWebSocket socket;
    private JsonData recJD = new JsonData();
    private PlayerStateMsg playerStateMsg = new PlayerStateMsg();

    public void Connect(string url) {
        address = url;
        socket = new WebSocket(address);
        socket.OnOpen += Socket_OnOpen;
        socket.OnMessage += Socket_OnMessage;
        socket.OnClose += Socket_OnClose;
        socket.OnError += Socket_OnError;
        socket.ConnectAsync();
    }

    public void SendMsgToServer(string msg) {
        if (socket != null) {
            socket.SendAsync(msg);
        }
    }

    public void CloseConnect() {
        if (socket != null && socket.ReadyState != WebSocketState.Closed) {
            socket.CloseAsync();
        }
    }

    private void Socket_OnOpen(object sender, OpenEventArgs e) {
        Debug.Log(string.Format("Connected: {0}\n", address));
    }

    private void Socket_OnMessage(object sender, MessageEventArgs e) {
        if (e.IsBinary) {
            Debug.Log(string.Format("Receive Bytes ({1}): {0}\n", e.Data, e.RawData.Length));
        } else if (e.IsText) {
            recJD = JsonMapper.ToObject(e.Data);
            if (this.GetModel<IGameControllerModel>().CurGameType == GameType.TimeTrial ||
                this.GetModel<IGameControllerModel>().CurGameType == GameType.Match) {
                playerStateMsg = this.GetSystem<INetworkSystem>().ParsePlayerStateMsg(recJD);
                this.GetSystem<IPlayerManagerSystem>().RespondAction(playerStateMsg);
            }
        }
        receiveCount += 1;
    }

    private void Socket_OnClose(object sender, CloseEventArgs e) {
        Debug.Log(string.Format("Closed: StatusCode: {0}, Reason: {1}\n", e.StatusCode, e.Reason));
    }

    private void Socket_OnError(object sender, ErrorEventArgs e) {
        Debug.Log(string.Format("Error: {0}\n", e.Message));
    }

    protected override void OnInit() {
        
    }   
}
