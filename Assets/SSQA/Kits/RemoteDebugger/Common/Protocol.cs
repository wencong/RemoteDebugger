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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public enum NetCmd
{
    None,

    C2S_CmdBegin             = 1000,
    C2S_CmdQueryAllObjs,
    C2S_CmdSetObjActive,
    C2S_CmdSetObjStatic,
    C2S_CmdSetObjTag,
    C2S_CmdSetObjLayer,
    C2S_QueryComponent,
    C2S_GetComponentProperty,
    C2S_EnableComponent,
    C2S_ModifyComponentProperty,
    C2S_Log,
    C2S_CustomCmd,

    S2C_CmdBegin             = 2000,
    S2C_CmdQueryAllObjs,
    S2C_CmdSetObjActive,
    S2C_CmdSetObjStatic,
    S2C_CmdSetObjTag,
    S2C_CmdSetObjLayer,
    S2C_QueryComponent,
    S2C_GetComponentProperty,
    S2C_EnableComponent,
    S2C_ModifyComponentProperty,
    S2C_Log,
    S2C_CustomCmd,

    S2C_FinishWait,
    SV_CmdEnd,
}




