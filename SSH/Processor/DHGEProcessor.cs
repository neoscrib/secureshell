using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using SSH.IO;
using SSH.Packets;

namespace SSH.Processor
{
    class DHGEProcessor : DiffieHellmanProcessor
    {
        private byte[] p;
        private byte[] g;
        private byte[] ClientPrivateKey;
        private byte[] ServerPublicKey;
        private byte[] ClientPublicKey;
        private SshDHGexRequest geRequest;

        public DHGEProcessor(Session session) : base(session) { }

        public override bool InternalProcessPacket(IPacket p)
        {
            switch (p.Code)
            {
                case MessageCode.SSH_MSG_KEXDH_REPLY:
                    {
                        var msg = (SshDHKexReply)p;
                        ClientPublicKey = CalculateE(msg.P, msg.G);
                        var msg1 = new SshDHGexInit(ClientPublicKey);
                        session.Socket.WritePacket(msg1);
                        return true;
                    }
                case MessageCode.SSH_MSG_KEX_DH_GEX_REPLY:
                    {
                        var msg = (SshDHGexReply)p;
                        ServerHostKey = msg.K;
                        ServerSignature = msg.S;
                        ServerPublicKey = msg.F;
                        SharedSecret = DeriveKeyMaterial(ServerPublicKey);
                        ExchangeHash = CalculateExchangeHash();
                        Close(Verify());
                        return true;
                    }
            }
            return false;
        }

        public override void Initialize()
        {
            geRequest = new SshDHGexRequest();
            session.Socket.WritePacket(geRequest);
        }

        private byte[] CalculateExchangeHash()
        {
            using (var pw = HashWriter.Create(session.Algorithms.KexAlgorithms[0]))
            {
                pw.WriteString(session.ClientIdentifier.ToSshMessage());
                pw.WriteString(session.ServerIdentifier.ToSshMessage());
                pw.WriteString(session.KexProcessor.ClientKexInit.ToSshMessage());
                pw.WriteString(session.KexProcessor.ServerKexInit.ToSshMessage());
                pw.WriteString(ServerHostKey);
                pw.WriteUInt32(geRequest.Min);
                pw.WriteUInt32(geRequest.N);
                pw.WriteUInt32(geRequest.Max);
                pw.WriteString(p);
                pw.WriteString(g);
                pw.WriteString(ClientPublicKey);
                pw.WriteString(ServerPublicKey);
                pw.WriteString(SharedSecret);
                return pw.Hash;
            }
        }

        internal byte[] DeriveKeyMaterial(byte[] f)
        {
            var p1 = BigInteger.Zero.AddUnsignedBigEndian(p);
            var x1 = BigInteger.Zero.AddUnsignedBigEndian(ClientPrivateKey);
            var f1 = BigInteger.Zero.AddUnsignedBigEndian(f);
            return BigInteger.ModPow(f1, x1, p1).ToByteArray().Reverse().ToArray();
        }

        internal byte[] CalculateE(byte[] p, byte[] g)
        {
            // reverse from big endian (ssh) to little endian (biginteger)
            this.p = p;
            this.g = g;
            var p1 = BigInteger.Zero.AddUnsignedBigEndian(p);
            var g1 = BigInteger.Zero.AddUnsignedBigEndian(g);
            var x1 = BigInteger.Zero;
            var q1 = (p1 - 1) / 2;
            var m = q1.ToByteArray().Reverse().ToArray()[0];

            ClientPrivateKey = new byte[p.Length];
            var rng = new RNGCryptoServiceProvider();
            while (x1 <= 1 || x1 >= q1)
            {
                rng.GetBytes(ClientPrivateKey);
                ClientPrivateKey[0] %= m;
                x1 = BigInteger.Zero.AddUnsignedBigEndian(ClientPrivateKey);
            }

            // reverse from little endian (biginteger) to big endian (ssh)
            return BigInteger.ModPow(g1, x1, p1).ToByteArray().Reverse().ToArray();
        }
    }
}
