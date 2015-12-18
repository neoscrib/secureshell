// Decompiled with JetBrains decompiler
// Type: System.Security.Cryptography.ECDiffieHellmanPublicKey
// Assembly: System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// MVID: A135FD92-29D5-498A-97C5-B5E2FCD4DDB2
// Assembly location: C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Core.dll

using System;
using System.Security.Permissions;
using System.Security.Cryptography;

namespace SSH.DiffieHellman
{
  /// <summary>
  /// Provides an abstract base class from which all <see cref="T:System.Security.Cryptography.ECDiffieHellmanCngPublicKey"/> implementations must inherit.
  /// </summary>
  [Serializable]
  [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
  public abstract class ECDiffieHellmanPublicKey : IDisposable
  {
    private byte[] m_keyBlob;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey"/> class.
    /// </summary>
    /// <param name="keyBlob">A byte array that represents an <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey"/> object.</param><exception cref="T:System.ArgumentNullException"><paramref name="keyBlob"/> is null.</exception>
    protected ECDiffieHellmanPublicKey(byte[] keyBlob)
    {
      if (keyBlob == null)
        throw new ArgumentNullException("keyBlob");
      this.m_keyBlob = keyBlob.Clone() as byte[];
    }

    /// <summary>
    /// Releases all resources used by the current instance of the <see cref="T:System.Security.Cryptography.ECDiffieHellman"/> class.
    /// </summary>
    public void Dispose()
    {
      this.Dispose(true);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="T:System.Security.Cryptography.ECDiffieHellman"/> class and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey"/> key BLOB to a byte array.
    /// </summary>
    /// 
    /// <returns>
    /// A byte array that contains the serialized Elliptic Curve Diffie-Hellman (ECDH) public key.
    /// </returns>
    public virtual byte[] ToByteArray()
    {
      return this.m_keyBlob.Clone() as byte[];
    }

    /// <summary>
    /// Serializes the <see cref="T:System.Security.Cryptography.ECDiffieHellmanPublicKey"/> public key to an XML string.
    /// </summary>
    /// 
    /// <returns>
    /// An XML string that contains the serialized Elliptic Curve Diffie-Hellman (ECDH) public key.
    /// </returns>
    public abstract string ToXmlString();
  }
}

