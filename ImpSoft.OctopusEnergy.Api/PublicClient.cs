﻿using ImpSoft.OctopusEnergy.Api.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ImpSoft.OctopusEnergy.Api
{
    internal class PublicClient : IPublicClient
    {
        public PublicClient()
        {
            AuthenticationHeader = null;
        }

        protected PublicClient(string apiKey)
        {
            AuthenticationHeader =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(apiKey + ":")));
        }

        private AuthenticationHeaderValue AuthenticationHeader { get; }

        public string BaseUrl { get; } = "https://api.octopus.energy";

        public async Task<IEnumerable<Product>> GetProductsAsync(
            DateTimeOffset? availableAt = null, bool? isVariable = null, bool? isGreen = null, bool? isTracker = null,
            bool? isPrepay = null, bool? isBusiness = null)
        {
            var uri = new Uri($"{BaseUrl}/v1/products/")
                .AddQueryParam("available_at", availableAt)
                .AddQueryParam("is_variable", isVariable)
                .AddQueryParam("is_green", isGreen)
                .AddQueryParam("is_tracker", isTracker)
                .AddQueryParam("is_prepay", isPrepay)
                .AddQueryParam("is_business", isBusiness);

            return await GetCollectionAsync<Product>(uri);
        }

        public async Task<ProductDetail> GetProductAsync(string productCode, DateTimeOffset? tariffsActiveAt = null)
        {
            Preconditions.IsNotNullOrWhiteSpace(productCode, nameof(productCode));

            var uri = new Uri($"{BaseUrl}/v1/products/{productCode}/")
                .AddQueryParam("tariffs_active_at ", tariffsActiveAt);

            return await GetAsync<ProductDetail>(uri);
        }

        public async Task<IEnumerable<GridSupplyPoint>> GetGridSupplyPointsAsync(string postcode = null)
        {
            var uri = new Uri($"{BaseUrl}/v1/industry/grid-supply-points/")
                .AddQueryParam("postcode", postcode);

            return await GetCollectionAsync<GridSupplyPoint>(uri);
        }

        public async Task<MeterPointGridSupplyPoint> GetGridSupplyPointAsync(string mpan)
        {
            Preconditions.IsNotNullOrWhiteSpace(mpan, nameof(mpan));

            var uriString = $"{BaseUrl}/v1/electricity-meter-points/{mpan}/";

            return await GetAsync<MeterPointGridSupplyPoint>(new Uri(uriString));
        }

        public async Task<IEnumerable<Charge>> GetElectricityUnitRatesAsync(string productCode, string tariffCode,
            ElectricityUnitRate rate = ElectricityUnitRate.Standard, DateTimeOffset? from = null,
            DateTimeOffset? to = null)
        {
            Preconditions.IsNotNullOrWhiteSpace(productCode, nameof(productCode));
            Preconditions.IsNotNullOrWhiteSpace(tariffCode, nameof(tariffCode));

            var uri = new Uri($"{BaseUrl}/v1/products/{productCode}/electricity-tariffs/{tariffCode}/{GetRateString()}-unit-rates/")
                .AddQueryParam("period_from", from)
                .AddQueryParam("period_to", to);

            return await GetCollectionAsync<Charge>(uri);

            string GetRateString()
            {
                switch (rate)
                {
                    case ElectricityUnitRate.Standard: return "standard";
                    case ElectricityUnitRate.Day: return "day";
                    case ElectricityUnitRate.Night: return "night";
                    default: throw new ArgumentOutOfRangeException(nameof(rate));
                }
            }
        }

        public async Task<IEnumerable<Charge>> GetElectricityUnitRatesAsync(Tariff tariff,
            ElectricityUnitRate rate = ElectricityUnitRate.Standard, DateTimeOffset? from = null,
            DateTimeOffset? to = null)
        {
            Preconditions.IsNotNull(tariff, nameof(tariff));

            var link = tariff.Links.SingleOrDefault(l => l.Rel == GetRateString());

            if (link == null)
                return Enumerable.Empty<Charge>();

            var uri = new Uri(link.HRef)
                .AddQueryParam("period_from", from)
                .AddQueryParam("period_to", to);

            return await GetCollectionAsync<Charge>(uri);

            string GetRateString()
            {
                switch (rate)
                {
                    case ElectricityUnitRate.Standard: return "standard_unit_rates";
                    case ElectricityUnitRate.Day: return "day_unit_rates";
                    case ElectricityUnitRate.Night: return "night_unit_rates";
                    default: throw new ArgumentOutOfRangeException(nameof(rate));
                }
            }
        }

        public async Task<IEnumerable<Charge>> GetElectricityStandingChargesAsync(string productCode, string tariffCode,
            DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            Preconditions.IsNotNullOrWhiteSpace(productCode, nameof(productCode));
            Preconditions.IsNotNullOrWhiteSpace(tariffCode, nameof(tariffCode));

            var uri = new Uri($"{BaseUrl}/v1/products/{productCode}/electricity-tariffs/{tariffCode}/standing-charges/")
                .AddQueryParam("period_from", from)
                .AddQueryParam("period_to", to);

            return await GetCollectionAsync<Charge>(uri);
        }

        public async Task<IEnumerable<Charge>> GetGasUnitRatesAsync(string productCode, string tariffCode,
            DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            Preconditions.IsNotNullOrWhiteSpace(productCode, nameof(productCode));
            Preconditions.IsNotNullOrWhiteSpace(tariffCode, nameof(tariffCode));

            var uri = new Uri($"{BaseUrl}/v1/products/{productCode}/gas-tariffs/{tariffCode}/standard-unit-rates/")
                .AddQueryParam("period_from", from)
                .AddQueryParam("period_to", to);

            return await GetCollectionAsync<Charge>(uri);
        }

        public async Task<IEnumerable<Charge>> GetGasStandingChargesAsync(string productCode, string tariffCode,
            DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            Preconditions.IsNotNullOrWhiteSpace(productCode, nameof(productCode));
            Preconditions.IsNotNullOrWhiteSpace(tariffCode, nameof(tariffCode));

            var uri = new Uri($"{BaseUrl}/v1/products/{productCode}/gas-tariffs/{tariffCode}/standing-charges/")
                .AddQueryParam("period_from", from)
                .AddQueryParam("period_to", to);

            return await GetCollectionAsync<Charge>(uri);
        }

        public async Task<IEnumerable<Charge>> GetElectricityStandingChargesAsync(Tariff tariff,
            DateTimeOffset? from = null, DateTimeOffset? to = null)
        {
            Preconditions.IsNotNull(tariff, nameof(tariff));

            var link = tariff.Links.SingleOrDefault(l => l.Rel == "standing_charges");

            if (link == null)
                return Enumerable.Empty<Charge>();

            var uri = new Uri(link.HRef)
                .AddQueryParam("period_from", from)
                .AddQueryParam("period_to", to);

            return await GetCollectionAsync<Charge>(uri);
        }

        protected async Task<IEnumerable<TResult>> GetCollectionAsync<TResult>(Uri uri, [CallerMemberName] string caller = null)
        {
            var results = Enumerable.Empty<TResult>();

            var pages = 0;

            while (uri != null)
            {
                pages++;

                var response = await GetAsync<PagedResults<TResult>>(uri, caller);

                results = results.Concat(response.Results ?? Enumerable.Empty<TResult>());

                uri = string.IsNullOrEmpty(response.Next) ? null : new Uri(response.Next);
            }

            Debug.WriteLine($"Pages fetched: {pages}");

            return results;
        }

        private async Task<TResult> GetAsync<TResult>(Uri uri, [CallerMemberName] string caller = null)
        {
            Debug.WriteLine(uri.ToString());

            using (var handler = new HttpClientHandler())
            {
#if NETCOREAPP
                handler.AutomaticDecompression = DecompressionMethods.All;
#else
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
#endif

                using (var client = new HttpClient(handler))
                {
                    client.DefaultRequestHeaders.Authorization = AuthenticationHeader;
                    client.BaseAddress = uri;

                    using (var httpResponse = await client.GetAsync(uri))
                    {
                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new UriGetException(GetErrorMessage(httpResponse), uri);
                        }

                        return JsonConvert.DeserializeObject<TResult>(await httpResponse.Content.ReadAsStringAsync());
                    }
                }
            }

            string GetErrorMessage(HttpResponseMessage response)
            {
                caller = caller?.StripAsyncSuffix() ?? Resources.UnknownMethod;

                return string.Format(CultureInfo.CurrentCulture,
                    Resources.HttpRequestFailed, caller, response.StatusCode, response.ReasonPhrase);
            }
        }
    }
}