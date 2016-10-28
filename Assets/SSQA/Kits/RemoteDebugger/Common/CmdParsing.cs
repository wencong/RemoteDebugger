/*!lic_info

The MIT License (MIT)

Copyright (c) 2015 SeaSunOpenSource

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

*/

namespace RemoteDebugger {
    using System.Collections.Generic;
    using System;

    public delegate bool CmdHandler(NetCmd cmd, Cmd c);

    public enum CmdExecResult {
        Succ,
        Failed,
        HandlerNotFound,
    }

    public class CmdParsing {
        public void RegisterHandler(NetCmd cmd, CmdHandler handler) {
            m_handlers[cmd] = handler;
        }

        public CmdExecResult Execute(Cmd c) {
            try {
                NetCmd cmd = c.ReadNetCmd();
                CmdHandler handler;
                if (!m_handlers.TryGetValue(cmd, out handler)) {
                    return CmdExecResult.HandlerNotFound;
                }

                if (handler(cmd, c)) {
                    return CmdExecResult.Succ;
                }
                else {
                    return CmdExecResult.Failed;
                }
            }
            catch (Exception ex) {
                Console.WriteLine("[cmd] Execution failed. ({0})", ex.Message);
                return CmdExecResult.Failed;
            }
        }

        Dictionary<NetCmd, CmdHandler> m_handlers = new Dictionary<NetCmd, CmdHandler>();
    }
}