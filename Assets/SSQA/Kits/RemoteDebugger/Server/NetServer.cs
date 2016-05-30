using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetServer : IDisposable {
    private TcpListener tcp_listener = null;
    private TcpClient tcp_client = null;

    private CmdParsing cmd_parser = new CmdParsing();

    public NetServer(int port) {
        tcp_listener = new TcpListener(IPAddress.Any, port);
        tcp_listener.Start();
        tcp_listener.BeginAcceptTcpClient(OnAcceptTcpClient, null);
    }

    public void RegisterHandler(NetCmd cmd, CmdHandler handler) {
        cmd_parser.RegisterHandler(cmd, handler);
    }

    private void OnAcceptTcpClient(IAsyncResult ar) {
        try {
            if (tcp_listener == null) {
                return;
            }
            tcp_client = tcp_listener.EndAcceptTcpClient(ar);
            tcp_listener.BeginAcceptTcpClient(OnAcceptTcpClient, null);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }
    }

    public void LogMsgToClient(string szMsg) {
        try {
            Cmd usCmd = new Cmd(szMsg.Length);
            usCmd.WriteNetCmd(NetCmd.S2C_Log);
            usCmd.WriteString(szMsg);
            this.SendCommand(usCmd);
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }
    } 

    public void Update() {
        if (tcp_client == null) {
            return;
        }

        try {
            while (tcp_client.Available > 0) {
                byte[] by_len = new byte[4];
                
                int read_byte_count = tcp_client.GetStream().Read(by_len, 0, by_len.Length);
                int data_len = BitConverter.ToInt32(by_len, 0);

                if (read_byte_count > 0 && data_len > 0) {
                    byte[] by_data = new byte[data_len];
                    read_byte_count = tcp_client.GetStream().Read(by_data, 0, data_len);
                    if (read_byte_count == data_len) {
                        CmdExecResult ret = cmd_parser.Execute(new Cmd(by_data));
                        switch (ret) {
                            case CmdExecResult.Succ:
                                break;
                            case CmdExecResult.Failed:
                                //Debug.Log("net cmd execution failed");
                                LogMsgToClient("net cmd execution failed");
                                break;
                            case CmdExecResult.HandlerNotFound:
                                //Debug.Log("net cmd unknown");
                                LogMsgToClient("net cmd unknown");
                                break;
                        }
                        
                    }
                    else {
                        Debug.Log(string.Format("Read data error - {0} ", data_len));
                    }
                }
            }
        }
        catch (Exception ex) {
            Debug.LogException(ex);
        }
    }

    public void SendCommand(Cmd cmd) {
        if (tcp_client == null || tcp_client.GetStream() == null) {
            return;
        }

        byte[] cmdLenBytes = BitConverter.GetBytes((int)cmd.WrittenLen);
        tcp_client.GetStream().Write(cmdLenBytes, 0, cmdLenBytes.Length);
        tcp_client.GetStream().Write(cmd.Buffer, 0, cmd.WrittenLen);
        //Debug.Log (string.Format("cmd written, len ({0})", cmd.WrittenLen));
    }

    public void Close() {
        if (tcp_client != null) {
            tcp_client.Close();
            tcp_client = null;
        }

        if (tcp_listener != null) {
            tcp_listener.Stop();
            tcp_listener = null;
        }
    }

    public void Dispose() {

    }
}

