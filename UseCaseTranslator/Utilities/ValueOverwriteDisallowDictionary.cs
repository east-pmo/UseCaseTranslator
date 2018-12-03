using System.Collections;
using System.Collections.Generic;

namespace East.Tool.UseCaseTranslator.Utilities
{
    /// <summary>
    /// 値の上書きを許さないDictionary
    /// </summary>
    /// <typeparam name="TKey">キーの型</typeparam>
    /// <typeparam name="TValue">値の型</typeparam>
    public sealed class ValueOverwriteDisallowDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        //
        // クラスメソッド
        //

        /// <summary>
        /// Dictionary型の値を返す
        /// </summary>
        /// <param name="value">変換元の値</param>
        public static implicit operator Dictionary<TKey, TValue>(ValueOverwriteDisallowDictionary<TKey, TValue> value)
        {
            return value.AsDictionary;
        }

        //
        // フィールド
        //

        /// <summary>
        /// 実装
        /// </summary>
        private readonly Dictionary<TKey, TValue> impl = new Dictionary<TKey, TValue>();

        //
        // プロパティ
        //

        public Dictionary<TKey, TValue> AsDictionary
        {
            get {
                return new Dictionary<TKey, TValue>(impl);
            }
        }

        //
        // 再定義プロパティ
        //

        /// <summary>
        /// インデクサ
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>値</returns>
        public TValue this[TKey key]
        {
            get => impl[key];
            set => impl.Add(key, value);
        }

        /// <summary>
        /// キーの集合
        /// </summary>
        public ICollection<TKey> Keys => impl.Keys;

        /// <summary>
        /// 値の集合
        /// </summary>
        public ICollection<TValue> Values => impl.Values;

        /// <summary>
        /// 項目数
        /// </summary>
        public int Count => impl.Count;

        /// <summary>
        /// 読み取り専用フラグ
        /// </summary>
        public bool IsReadOnly => false;

        //
        // 再定義メソッド
        //

        /// <summary>
        /// 値を追加する
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        public void Add(TKey key, TValue value)
        {
            impl.Add(key, value);
        }

        /// <summary>
        /// 値を追加する
        /// </summary>
        /// <param name="item">キーと値のペア</param>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            (impl as ICollection<KeyValuePair<TKey, TValue>>).Add(item);
        }

        /// <summary>
        /// 項目をクリアする
        /// </summary>
        public void Clear()
        {
            impl.Clear();
        }

        /// <summary>
        /// 項目が含まれるかを返す
        /// </summary>
        /// <param name="item">キーと値のペア</param>
        /// <returns>含まれるときtrue</returns>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return (impl as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        /// <summary>
        /// キーが含まれるかを返す
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>含まれるときtrue</returns>
        public bool ContainsKey(TKey key)
        {
            return impl.ContainsKey(key);
        }

        /// <summary>
        /// 項目を配列にコピーする
        /// </summary>
        /// <param name="array">コピー先</param>
        /// <param name="arrayIndex">インデックス</param>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            (impl as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// 指定キーの項目を削除する
        /// </summary>
        /// <param name="key">キー</param>
        /// <returns>削除したときtrue</returns>
        public bool Remove(TKey key)
        {
            return impl.Remove(key);
        }

        /// <summary>
        /// 指定項目を削除する
        /// </summary>
        /// <param name="item">項目</param>
        /// <returns>削除したときtrue</returns>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return (impl as ICollection<KeyValuePair<TKey, TValue>>).Remove(item);
        }

        /// <summary>
        /// 指定キーの値の取得を試みる
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">取得した値</param>
        /// <returns>取得したときtrue</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return impl.TryGetValue(key, out value);
        }

        /// <summary>
        /// 列挙子を返す
        /// </summary>
        /// <returns>列挙子</returns>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (impl as ICollection<KeyValuePair<TKey, TValue>>).GetEnumerator();
        }

        /// <summary>
        /// 列挙子を返す
        /// </summary>
        /// <returns>列挙子</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return impl.GetEnumerator();
        }
    }
}

