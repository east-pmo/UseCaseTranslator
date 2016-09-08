using System;
using System.Collections.Generic;

using System.Diagnostics.Contracts;

namespace East.Tool
{
    /// <summary>
    /// 操作抽象基本クラス
    /// </summary>
    public abstract class Operator
    {
        //
        // フィールド・プロパティ
        //

        /// <summary>
        /// パラメーターの辞書
        /// </summary>
        protected readonly Dictionary<string, object> OperationParameters;

        //
        // メソッド
        //

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="parameters">パラメーター</param>
        protected Operator(IDictionary<string, object> parameters)
        {
            Contract.Requires(parameters != null);

            OperationParameters = new Dictionary<string, object>(parameters);
        }

        //
        // 再定義必須メソッド
        //

        /// <summary>
        /// 操作可能かを返す
        /// </summary>
        public abstract bool CanOperate();

        /// <summary>
        /// 操作を実行する
        /// </summary>
        public abstract void Operate();
    }
}
