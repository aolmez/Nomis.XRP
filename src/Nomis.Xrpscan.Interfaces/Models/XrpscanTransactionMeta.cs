﻿using System.Text.Json.Serialization;

namespace Nomis.Xrpscan.Interfaces.Models
{
    /// <summary>
    /// Xrpscan transaction meta.
    /// </summary>
    public class XrpscanTransactionMeta
    {
        /// <summary>
        /// Transaction index.
        /// </summary>
        [JsonPropertyName("TransactionIndex")]
        public long TransactionIndex { get; set; }

        /// <summary>
        /// Transaction result.
        /// </summary>
        [JsonPropertyName("TransactionResult")]
        public string? TransactionResult { get; set; }
    }
}