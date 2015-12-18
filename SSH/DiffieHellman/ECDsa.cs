// Decompiled with JetBrains decompiler
// Type: System.Security.Cryptography.ECDsa
// Assembly: System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: A135FD92-29D5-498A-97C5-B5E2FCD4DDB2
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll

using System;
using System.Security.Permissions;
using System.Security.Cryptography;

namespace SSH.DiffieHellman
{
  /// <summary>
  /// Provides an abstract base class that encapsulates the Elliptic Curve Digital Signature Algorithm (ECDSA).
  /// </summary>
  [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
  public abstract class ECDsa : AsymmetricAlgorithm
  {
    /// <summary>
    /// Gets the name of the key exchange algorithm.
    /// </summary>
    /// 
    /// <returns>
    /// Always null.
    /// </returns>
    public override string KeyExchangeAlgorithm
    {
      get
      {
        return (string) null;
      }
    }

    /// <summary>
    /// Gets the name of the signature algorithm.
    /// </summary>
    /// 
    /// <returns>
    /// The string "ECDsa".
    /// </returns>
    public override string SignatureAlgorithm
    {
      get
      {
        return "ECDsa";
      }
    }

    /// <summary>
    /// Creates a new instance of the default implementation of the Elliptic Curve Digital Signature Algorithm (ECDSA).
    /// </summary>
    /// 
    /// <returns>
    /// A new instance of the default implementation (<see cref="T:System.Security.Cryptography.ECDsaCng"/>) of this class.
    /// </returns>
    public static ECDsa Create()
    {
      return ECDsa.Create(typeof (ECDSAManaged).FullName);
    }

    /// <summary>
    /// Creates a new instance of the specified implementation of the Elliptic Curve Digital Signature Algorithm (ECDSA).
    /// </summary>
    /// 
    /// <returns>
    /// A new instance of the specified implementation of this class. If the specified algorithm name does not map to an ECDSA implementation, this method returns null.
    /// </returns>
    /// <param name="algorithm">The name of an ECDSA implementation. The following strings all refer to the same implementation, which is the only implementation currently supported in the .NET Framework:- "ECDsa"- "ECDsaCng"- "System.Security.Cryptography.ECDsaCng"You can also provide the name of a custom ECDSA implementation.</param><exception cref="T:System.ArgumentNullException">The <paramref name="algorithm"/> parameter is null.</exception>
    public static ECDsa Create(string algorithm)
    {
      if (algorithm == null)
        throw new ArgumentNullException("algorithm");
      return CryptoConfig.CreateFromName(algorithm) as ECDsa;
    }

    /// <summary>
    /// Generates a digital signature for the specified hash value.
    /// </summary>
    /// 
    /// <returns>
    /// A digital signature that consists of the given hash value encrypted with the private key.
    /// </returns>
    /// <param name="hash">The hash value of the data that is being signed.</param><exception cref="T:System.ArgumentNullException">The <paramref name="hash"/> parameter is null.</exception>
    public abstract byte[] SignHash(byte[] hash);

    /// <summary>
    /// Verifies a digital signature against the specified hash value.
    /// </summary>
    /// 
    /// <returns>
    /// true if the hash value equals the decrypted signature; otherwise, false.
    /// </returns>
    /// <param name="hash">The hash value of a block of data.</param><param name="signature">The digital signature to be verified.</param>
    public abstract bool VerifyHash(byte[] hash, byte[] signature);
  }
}

