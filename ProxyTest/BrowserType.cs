using System;

namespace ProxyTest
{
    [Flags]
    public enum BrowserType
    {

        Chrome = 0x2,

        Any = 0xffff
    }
}