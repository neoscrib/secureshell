namespace SSH.Processor
{
    interface IKeyExchangeProcessor : IPacketProcessor
    {
        byte[] SharedSecret { get; set; }
        void Initialize();
        byte[] ExchangeHash { get; set; }
        StatusCode Verify();
    }
}
