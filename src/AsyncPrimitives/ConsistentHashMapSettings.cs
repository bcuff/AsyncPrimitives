using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AsyncPrimitives
{
    public class ConsistentHashMapSettings<TKey, TValue>
    {
        public static Func<string, ulong> CreateHashFunction(Func<HashAlgorithm> hashAlgorithmFactory)
        {
            return key =>
            {
                using (var algorithm = hashAlgorithmFactory())
                {
                    var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(key));
                    ulong result = 0L;
                    for (int i = 0, n = hash.Length / sizeof(ulong); i < n; ++i)
                    {
                        result
                            ^= (ulong)hash[i]
                            | (ulong)hash[i + 1] << 8
                            | (ulong)hash[i + 2] << 16
                            | (ulong)hash[i + 3] << 24
                            | (ulong)hash[i + 4] << 32
                            | (ulong)hash[i + 5] << 40
                            | (ulong)hash[i + 6] << 48
                            | (ulong)hash[i + 7] << 56;
                    }
                    for (int i = (hash.Length / sizeof(ulong)) * sizeof(ulong), n = hash.Length % sizeof(ulong); i < n; ++i)
                    {
                        var mask = 0xffu << (i * 8);
                        result ^= mask & (ulong)hash[i] << (i * 8);
                    }
                    return result;
                }
            };
        }

        private Func<string, ulong> _hashFunction;
        public Func<string, ulong> HashFunction
        {
            get { return _hashFunction ?? CreateHashFunction(MD5.Create); }
            set { _hashFunction = value; }
        }

        private Func<TValue, int> _virtualNodeCountProvider;
        public Func<TValue, int> VirtualNodeCountProvider
        {
            get { return _virtualNodeCountProvider ?? (v => 1000); }
            set { _virtualNodeCountProvider = value; }
        }

        private Func<TValue, string> _valueHashDataProvider;
        public Func<TValue, string> ValueHashDataProvider
        {
            get { return _valueHashDataProvider ?? (v => v.ToString()); }
            set { _valueHashDataProvider = value; }
        }

        private Func<TKey, string> _keyHashDataProvider;
        public Func<TKey, string> KeyHashDataProvider
        {
            get { return _keyHashDataProvider ?? (k => k.ToString()); }
            set { _keyHashDataProvider = value; }
        }
    }

}
