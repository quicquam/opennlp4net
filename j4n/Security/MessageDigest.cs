﻿using System.Security.Cryptography;

namespace j4n.Security
{
    public class MessageDigest
    {
        private byte[] _digestBuffer;
        private int _offset;
        private readonly HashAlgorithm _hashAlgorithm;
        public MessageDigest(string name)
        {
            _hashAlgorithm = name == "MD5" 
                ? (HashAlgorithm) new MD5CryptoServiceProvider() : new SHA1Managed();
        }
        
        public void update(byte[] buffer)
        {
            var baselenth = _digestBuffer != null ? _digestBuffer.GetLength(0) : 0;
            _digestBuffer = new byte[baselenth + buffer.GetLength(0)];
            _offset += _hashAlgorithm.TransformBlock(buffer, 0, buffer.GetLength(0), _digestBuffer, _offset);
        }

        public static MessageDigest getInstance(string name)
        {
            return new MessageDigest(name);
        }

        public byte[] digest()
        {
            return _hashAlgorithm.TransformFinalBlock(_digestBuffer, 0, _digestBuffer.GetLength(0));
        }
    }
}