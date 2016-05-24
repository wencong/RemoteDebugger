using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MainServer : IDisposable {
    private NetServer net_server = null;

    private long _currentTimeInMilliseconds = 0;
    private long _tickNetLast = 0;
    private long _tickNetInterval = 200;

    private MainServer() {
    }

    private static MainServer inst = null;
    public static MainServer Instance {
        get {
            if (inst == null) {
                inst = new MainServer();
            }
            return inst;
        }
    }

    public void Init(int port) {
        net_server = new NetServer(port);
        C2SHandlers.Instance.Init(net_server);
    }

    public void UnInit() {
        net_server.Close();
        net_server = null;
    }

    public void Update() {
        _currentTimeInMilliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

        if (_currentTimeInMilliseconds - _tickNetLast > _tickNetInterval) {
            if (net_server != null) {
                net_server.Update();
            }
            _tickNetLast = _currentTimeInMilliseconds;
        }
    }

    public void Dispose() {
        
    }
}

