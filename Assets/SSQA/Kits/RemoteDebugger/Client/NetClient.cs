using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetClient : IDisposable {
    private TcpClient tcp_client = null;
    private CmdParsing cmd_parser = new CmdParsing();

    public bool IsConnected {
        get {
            if (tcp_client != null) {
                return tcp_client.Connected;
            }
            return false;
        }
    }

    public void Connect(string host, int port) {
        if (IsConnected) {
            return;
        }
        tcp_client = new TcpClient();
        tcp_client.BeginConnect(host, port, OnConnect, null);
    }

    public void Disconnect() {
        if (tcp_client != null) {
            tcp_client.Close();
            tcp_client = null;
        }
    }

    public void RegisterHandler(NetCmd cmd, CmdHandler handler) {
        cmd_parser.RegisterHandler(cmd, handler);
    }

    public void SendCmd(Cmd cmd) {
        try {
            byte[] cmdLenBytes = BitConverter.GetBytes(cmd.WrittenLen);
            tcp_client.GetStream().Write(cmdLenBytes, 0, cmdLenBytes.Length);
            tcp_client.GetStream().Write(cmd.Buffer, 0, cmd.WrittenLen);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }
    }

    public void Update() {
        if (tcp_client == null) {
            return;
        }
        
        while (tcp_client.Available > 0) {
            byte[] by_len = new byte[4];

            int read_byte_count = tcp_client.GetStream().Read(by_len, 0, by_len.Length);
            int data_len = BitConverter.ToInt32(by_len, 0);

            if (read_byte_count > 0 && data_len > 0) {
                byte[] by_data = new byte[data_len];
                int offset = 0;

                while (data_len != 0) {
                    try {
                        byte[] byRead = new byte[data_len];
                        read_byte_count = tcp_client.GetStream().Read(byRead, 0, data_len);
                        
                        byRead.CopyTo(by_data, offset);

                        offset += (ushort)read_byte_count;
                        data_len -= (ushort)read_byte_count;
                    }
                    catch (Exception ex) {
                        Debug.LogException(ex);
                    }
                }

                if (0 == data_len) {
                    CmdExecResult ret = cmd_parser.Execute(new Cmd(by_data));
                    switch (ret) {
                        case CmdExecResult.Succ:
                            break;
                        case CmdExecResult.Failed:
                            Debug.Log("net cmd execution failed");
                            break;
                        case CmdExecResult.HandlerNotFound:
                            Debug.Log("net cmd unknown");
                            break;
                    }

                }
                else {
                    Debug.Log(string.Format("Read data error - {0} ", data_len));
                }
            }
        }
    }

    private void OnConnect(IAsyncResult asyncResult) {
        // Retrieving TcpClient from IAsyncResult

        try {
            if (tcp_client.Connected) {
                Debug.Log("connect successfully");
                S2CHandlers.Instance.Init(this);
            }
            else {
                throw new Exception();
            }
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }
    }

    public void Dispose() {
        Disconnect();
    }

}

