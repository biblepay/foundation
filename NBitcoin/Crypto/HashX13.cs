using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HashLib;

namespace NBitcoin.Crypto
{
    public sealed class HashBiblePay
    {
        private readonly List<IHash> hashers;

        private readonly object hashLock;

        private static readonly Lazy<HashBiblePay> SingletonInstance = new Lazy<HashBiblePay>(LazyThreadSafetyMode.PublicationOnly);

        public HashBiblePay()
        {
            this.hashers = new List<IHash>
            {
                HashFactory.Crypto.SHA3.CreateBlake512(),
                HashFactory.Crypto.SHA3.CreateBlueMidnightWish512(),
                HashFactory.Crypto.SHA3.CreateGroestl512(),
                HashFactory.Crypto.SHA3.CreateSkein512_Custom(),
                HashFactory.Crypto.SHA3.CreateJH512(),
                HashFactory.Crypto.SHA3.CreateKeccak512(),
              };

            this.hashLock = new object();
            this.Multiplier = 1;
        }

        public uint Multiplier { get; private set; }

        /// <summary>
        /// using the instance method is not thread safe. 
        /// to calling the hashing method in a multi threaded environment use the create() method
        /// </summary>
        public static HashBiblePay Instance => SingletonInstance.Value;

        public static HashBiblePay Create()
        {
            return new HashBiblePay();
        }

        public uint256 Hash(byte[] input)
        {
            var buffer = input;

            Digest digestBiblePay = new BBP512();

            lock (this.hashLock)
            {
                foreach (var hasher in this.hashers)
                {
                    buffer = hasher.ComputeBytes(buffer).GetBytes();
                }

                buffer = digestBiblePay.digest(buffer);

            }
            uint256 myBBP = new uint256(buffer.Take(32).ToArray());
            return myBBP;
        }
    }












    public sealed class HashGroestlOnly
    {
        private readonly List<IHash> hashers;

        private readonly object hashLock;

        private static readonly Lazy<HashGroestlOnly> SingletonInstance = new Lazy<HashGroestlOnly>(LazyThreadSafetyMode.PublicationOnly);

        public HashGroestlOnly()
        {
            this.hashers = new List<IHash>
            {
                HashFactory.Crypto.SHA3.CreateGroestl512(),
            };

            this.hashLock = new object();
            this.Multiplier = 1;
        }

        public uint Multiplier { get; private set; }

        /// <summary>
        /// using the instance method is not thread safe. 
        /// to calling the hashing method in a multi threaded environment use the create() method
        /// </summary>
        public static HashGroestlOnly Instance => SingletonInstance.Value;

        public static HashGroestlOnly Create()
        {
            return new HashGroestlOnly();
        }

        public uint256 Hash(byte[] input)
        {
            var buffer = input;

            lock (this.hashLock)
            {
                foreach (var hasher in this.hashers)
                {
                    buffer = hasher.ComputeBytes(buffer).GetBytes();
                }
            }

            return new uint256(buffer.Take(32).ToArray());
        }
    }




    // this hashing class is not thread safe to use with static instances.
    // the hashing objects maintain state during hash calculation.
    // to use in a multi threaded environment create a new instance for every hash.

    public sealed class HashX11
    {
        private readonly List<IHash> hashers;

        private readonly object hashLock;

        private static readonly Lazy<HashX11> SingletonInstance = new Lazy<HashX11>(LazyThreadSafetyMode.PublicationOnly);

        public HashX11()
        {
            this.hashers = new List<IHash>
            {
                HashFactory.Crypto.SHA3.CreateBlake512(),
                HashFactory.Crypto.SHA3.CreateBlueMidnightWish512(),
                HashFactory.Crypto.SHA3.CreateGroestl512(),
                HashFactory.Crypto.SHA3.CreateSkein512_Custom(),
                HashFactory.Crypto.SHA3.CreateJH512(),
                HashFactory.Crypto.SHA3.CreateKeccak512(),
                HashFactory.Crypto.SHA3.CreateLuffa512(),
                HashFactory.Crypto.SHA3.CreateCubeHash512(),
                HashFactory.Crypto.SHA3.CreateSHAvite3_512_Custom(),
                HashFactory.Crypto.SHA3.CreateSIMD512(),
                HashFactory.Crypto.SHA3.CreateEcho512(),
            };

            this.hashLock = new object();
            this.Multiplier = 1;
        }

        public uint Multiplier { get; private set; }

        /// <summary>
        /// using the instance method is not thread safe. 
        /// to calling the hashing method in a multi threaded environment use the create() method
        /// </summary>
        public static HashX11 Instance => SingletonInstance.Value;

        public static HashX11 Create()
        {
            return new HashX11();
        }

        public uint256 Hash(byte[] input)
        {
            var buffer = input;

            lock (this.hashLock)
            {
                foreach (var hasher in this.hashers)
                {
                    buffer = hasher.ComputeBytes(buffer).GetBytes();
                }
            }

            uint256 a = new uint256(buffer.Take(32).ToArray());
            return a;
        }
    }





    public sealed class HashX13
    {
        private readonly List<IHash> hashers;

        private readonly object hashLock;

        private static readonly Lazy<HashX13> SingletonInstance = new Lazy<HashX13>(LazyThreadSafetyMode.PublicationOnly);





        public HashX13()
        {
            this.hashers = new List<IHash>
            {
                HashFactory.Crypto.SHA3.CreateBlake512(),
                HashFactory.Crypto.SHA3.CreateBlueMidnightWish512(),
                HashFactory.Crypto.SHA3.CreateGroestl512(),
                HashFactory.Crypto.SHA3.CreateSkein512_Custom(),
                HashFactory.Crypto.SHA3.CreateJH512(),
                HashFactory.Crypto.SHA3.CreateKeccak512(),
                HashFactory.Crypto.SHA3.CreateLuffa512(),
                HashFactory.Crypto.SHA3.CreateCubeHash512(),
                HashFactory.Crypto.SHA3.CreateSHAvite3_512_Custom(),
                HashFactory.Crypto.SHA3.CreateSIMD512(),
                HashFactory.Crypto.SHA3.CreateEcho512(),
                HashFactory.Crypto.SHA3.CreateHamsi512(),
                HashFactory.Crypto.SHA3.CreateFugue512(),
            };

            this.hashLock = new object();
            this.Multiplier = 1;
        }

        public uint Multiplier { get; private set; }

        /// <summary>
        /// using the instance method is not thread safe. 
        /// to calling the hashing method in a multi threaded environment use the create() method
        /// </summary>
        public static HashX13 Instance => SingletonInstance.Value;

        public static HashX13 Create()
        {
            return new HashX13();
        }

        public uint256 Hash(byte[] input)
        {
            byte[] buffer = input;

            lock (this.hashLock)
            {
                foreach (IHash hasher in this.hashers)
                {
                    buffer = hasher.ComputeBytes(buffer).GetBytes();
                }
            }

            return new uint256(buffer.Take(32).ToArray());
        }
    }

    // Tack on sealed class for sphlib
    // $Id: DigestEngine.java 229 2010-06-16 20:22:27Z tp $


    public abstract class DigestEngine : Digest
    {
        public abstract uint getDigestLength();
        public abstract Digest copy();
        public abstract uint getBlockLength();
        public abstract string toString();


        /**             * Reset the hash algorithm state.             */
        protected abstract void engineReset();

        protected abstract void processBlock(byte[] data);
        protected abstract void doPadding(byte[] buf, uint off);
        protected abstract void doInit();

        private uint digestLen, blockLen, inputLen;
        private byte[] inputBuf, outputBuf;
        private ulong blockCount;
        public DigestEngine()
        {
            doInit();
            digestLen = getDigestLength();
            blockLen = getInternalBlockLength();
            inputBuf = new byte[blockLen];
            outputBuf = new byte[digestLen];
            inputLen = 0;
            blockCount = 0;
        }

        private void adjustDigestLen()
        {
            if (digestLen == 0)
            {
                digestLen = getDigestLength();
                outputBuf = new byte[digestLen];
            }
        }

        /** @see Digest */
        public byte[] digest()
        {
            adjustDigestLen();
            byte[] result = new byte[digestLen];
            digest(result, 0, digestLen);
            return result;
        }

        /** @see Digest */
        public byte[] digest(byte[] input)
        {
            update(input, 0, (uint)input.Length);
            return digest();
        }

        /** @see Digest */
        public uint digest(byte[] buf, uint offset, uint len)
        {
            adjustDigestLen();
            if (len >= digestLen)
            {
                doPadding(buf, offset);
                reset();
                return digestLen;
            }
            else
            {
                doPadding(outputBuf, 0);
                Array.Copy(outputBuf, 0, buf, (int)offset, (int)len);
                reset();
                return len;
            }
        }

        /** @see Digest */
        public void reset()
        {
            engineReset();
            inputLen = 0;
            blockCount = 0;
        }

        /** @see Digest */
        public void update(byte input)
        {
            inputBuf[inputLen++] = (byte)input;
            if (inputLen == blockLen)
            {
                processBlock(inputBuf);
                blockCount++;
                inputLen = 0;
            }
        }

        /** @see Digest */
        public void update(byte[] input)
        {
            update(input, 0, (uint)input.Length);
        }

        /** @see Digest */
        public void update(byte[] input, uint offset, uint len)
        {
            while (len > 0)
            {
                uint copyLen = blockLen - inputLen;
                if (copyLen > len)
                    copyLen = len;
                Array.Copy(input, (int)offset, inputBuf, (int)inputLen, (int)copyLen);
                offset += copyLen;
                inputLen += copyLen;
                len -= copyLen;
                if (inputLen == blockLen)
                {
                    processBlock(inputBuf);
                    blockCount++;
                    inputLen = 0;
                }
            }
        }

        protected uint getInternalBlockLength()
        {
            return getBlockLength();
        }

        protected uint flush()
        {
            return inputLen;
        }

        protected byte[] getBlockBuffer()
        {
            return inputBuf;
        }

        protected ulong getBlockCount()
        {
            return blockCount;
        }

        protected Digest copyState(DigestEngine dest)
        {
            dest.inputLen = inputLen;
            dest.blockCount = blockCount;
            Array.Copy(inputBuf, 0, dest.inputBuf, 0,
              (int)(uint)inputBuf.Length);
            adjustDigestLen();
            dest.adjustDigestLen();
            Array.Copy(outputBuf, 0, dest.outputBuf, 0,
              (int)(uint)outputBuf.Length);
            return dest;
        }
    }


    public interface Digest
    {

        void update(byte @in);
        void update(byte[] inbuf);
        void update(byte[] inbuf, uint off, uint len);

        byte[] digest();

        byte[] digest(byte[] inbuf);
        uint digest(byte[] outbuf, uint off, uint len);
        uint getDigestLength();

        void reset();
        Digest copy();
        uint getBlockLength();
        string toString();
    }





}
