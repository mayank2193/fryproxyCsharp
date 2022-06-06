namespace ProxyTest
{
    public interface IBrowserProxyFactory
    {

        /// <summary>
        ///     Create new browser proxy
        /// </summary>
        /// <returns>instance of <see cref="IBrowserProxy" /></returns>
        IBrowserProxy Create();

    }
}