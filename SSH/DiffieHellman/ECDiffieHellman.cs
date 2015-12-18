// Decompiled with JetBrains decompiler
// Type: System.Security.Cryptography.ECDiffieHellman
// Assembly: System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: A135FD92-29D5-498A-97C5-B5E2FCD4DDB2
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll

using System;
using System.Security.Permissions;
using System.Security.Cryptography;

namespace SSH.DiffieHellman
{
  /// <summary>
  /// Provides an abstract base class that Elliptic Curve Diffie-Hellman (ECDH) algorithm implementations can derive from. This class provides the basic set of operations that all ECDH implementations must support.
  /// </summary>
  [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
  public abstract class ECDiffieHellman : AsymmetricAlgorithm
  {
    /// <summary>
    /// Gets the name of the key exchange algorithm.
    /// </summary>
    /// 
    /// <returns>
    /// The name of the key exchange algorithm.
    /// </returns>
    public override string KeyExchangeAlgorithm
    {
      get
      {
        return "ECDiffieHellman";
      }
    }

    /// <summary>
    /// Gets the name of the signature algorithm.
    /// </summary>
    /// 
    /// <returns>
    /// Always null.
    /// </returns>
    public override string SignatureAlgorithm
    {
      get
      {
        return (string) null;
      }
    }

    /// <summary>
    /// Gets the public key that is being used by the current Elliptic Curve Diffie-Hellman (ECDH) instance.
    /// </summary>
    /// 
    /// <returns>
    /// The public part of the ECDH key pair that is being used by this <see cref="T:System.Security.Cryptography.ECDiffieHellman"/> instance.
    /// </returns>
    public abstract ECDiffieHellmanPublicKey PublicKey { get; }

    /// <summary>
    /// Creates a new instance of the default implementation of the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
    /// </summary>
    /// 
    /// <returns>
    /// A new instance of the default implementation of this class.
    /// </returns>
    public static ECDiffieHellman Create()
    {
      return ECDiffieHellman.Create(typeof (ECDiffieHellmanManaged).FullName);
    }

    /// <summary>
    /// Creates a new instance of the specified implementation of the Elliptic Curve Diffie-Hellman (ECDH) algorithm.
    /// </summary>
    /// 
    /// <returns>
    /// A new instance of the specified implementation of this class. If the specified algorithm name does not map to an ECDH implementation, this method returns null.
    /// </returns>
    /// <param name="algorithm">The name of an implementation of the ECDH algorithm. The following strings all refer to the same implementation, which is the only implementation currently supported in the .NET Framework:- "ECDH"- "ECDiffieHellman"- "ECDiffieHellmanCng"- "System.Security.Cryptography.ECDiffieHellmanCng"You can also provide the name of a custom ECDH implementation.</param><exception cref="T:System.ArgumentNullException">The <paramref name="algorithm"/> parameter is null. </exception>
    public static ECDiffieHellman Create(string algorithm)
    {
      if (algorithm == null)
        throw new ArgumentNullException("algorithm");
      return CryptoConfig.CreateFromName(algorithm) as ECDiffieHellman;
    }

    /// <summary>
    /// Derives bytes that can be used as a key, given another party's public key.
    /// </summary>
    /// 
    /// <returns>
    /// The key material from the key exchange with the other party’s public key.
    /// </returns>
    /// <param name="otherPartyPublicKey">The other party's public key.</param>
    public abstract byte[] DeriveKeyMaterial(ECDiffieHellmanPublicKey otherPartyPublicKey);
  }
}

