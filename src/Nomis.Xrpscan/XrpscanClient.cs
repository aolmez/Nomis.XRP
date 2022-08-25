﻿using System.Net.Http.Json;

using Microsoft.Extensions.Options;
using Nomis.Xrpscan.Interfaces;
using Nomis.Xrpscan.Interfaces.Models;
using Nomis.Xrpscan.Interfaces.Settings;

namespace Nomis.Xrpscan
{
    /// <inheritdoc cref="IXrpscanClient"/>
    internal sealed class XrpscanClient :
        IXrpscanClient
    {
        private const int ItemsFetchLimit = 25;

        private const int MaxTransactions = 15000;

        private readonly HttpClient _client;

        /// <summary>
        /// Initialize <see cref="XrpscanClient"/>.
        /// </summary>
        /// <param name="xrpscanSettings"><see cref="XrpscanSettings"/>.</param>
        public XrpscanClient(
            IOptions<XrpscanSettings> xrpscanSettings)
        {
            _client = new()
            {
                BaseAddress = new(xrpscanSettings.Value.ApiBaseUrl ??
                                  throw new ArgumentNullException(nameof(xrpscanSettings.Value.ApiBaseUrl)))
            };
        }

        /// <inheritdoc/>
        public async Task<XrpscanAccount> GetAccountDataAsync(string address)
        {
            var response = await _client.GetAsync($"/api/v1/account/{address}");
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<XrpscanAccount>())!;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<XrpscanTransaction>> GetTransactionsDataAsync(string address)
        {
            var result = new List<XrpscanTransaction>();
            var transactionsData = await GetTxList(address);
            result.AddRange(transactionsData?.Transactions ?? new List<XrpscanTransaction>());
            while (transactionsData?.Transactions?.Count >= ItemsFetchLimit && result.Count <= MaxTransactions)
            {
                transactionsData = await GetTxList(address, transactionsData.Marker);
                result.AddRange(transactionsData?.Transactions ?? new List<XrpscanTransaction>());
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<XrpscanAsset>> GetAssetsDataAsync(string address)
        {
            var request =
                $"/api/v1/account/{address}/assets";
            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<XrpscanAsset>>() ?? new List<XrpscanAsset>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<XrpscanOrder>> GetOrdersDataAsync(string address)
        {
            var request =
                $"/api/v1/account/{address}/orders";
            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<XrpscanOrder>>() ?? new List<XrpscanOrder>();
        }

        /// <inheritdoc/>
        public async Task<XrpscanKyc> GetKycDataAsync(string address)
        {
            var request =
                $"/api/v1/account/{address}/xummkyc";
            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<XrpscanKyc>() ?? new XrpscanKyc
            {
                Account = address,
                KycApproved = false
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<XrpscanObligation>> GetObligationsDataAsync(string address)
        {
            var request =
                $"/api/v1/account/{address}/obligations";
            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<XrpscanObligation>>() ?? new List<XrpscanObligation>();
        }

        private async Task<XrpscanTransactions?> GetTxList(string address, string? marker = null)
        {
            var request =
                $"/api/v1/account/{address}/transactions";
            if (!string.IsNullOrWhiteSpace(marker))
            {
                request = $"{request}?marker={marker}";
            }

            var response = await _client.GetAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<XrpscanTransactions>();
        }
    }
}