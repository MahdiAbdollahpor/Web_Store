using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web_Store.Application.Services.Fainances.Commands.ZarinPalService
{
    public class ZarinPalService
    {
        private readonly HttpClient _httpClient;

        public ZarinPalService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://sandbox.zarinpal.com/");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public async Task<ZarinPalRequestResponse> RequestPayment(ZarinPalRequest request)
        {
            var requestData = new
            {
                merchant_id = request.MerchantId,
                amount = request.Amount,
                callback_url = request.CallbackUrl,
                description = request.Description,
                metadata = new
                {
                    mobile = request.Mobile,
                    email = request.Email
                }
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("pg/v4/payment/request.json", content);
            var responseString = await response.Content.ReadAsStringAsync();

            // برای دیباگ، پاسخ را بررسی کنید
            Console.WriteLine("ZarinPal Response: " + responseString);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ZarinPal request failed: {responseString}");
            }

            var result = JsonConvert.DeserializeObject<ZarinPalRequestResponse>(responseString);

            if (result!.data?.code != 100)
            {
                throw new Exception($"ZarinPal error: {result.data?.message}");
            }

            return result;
        }

        public async Task<ZarinPalVerifyResponse> VerifyPayment(ZarinPalVerifyRequest request)
        {
            var requestData = new
            {
                merchant_id = request.MerchantId,
                amount = request.Amount,
                authority = request.Authority
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestData),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("pg/v4/payment/verify.json", content);
            var responseString = await response.Content.ReadAsStringAsync();

            Console.WriteLine("ZarinPal Verify Response: " + responseString);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"ZarinPal verify failed: {responseString}");
            }

            return JsonConvert.DeserializeObject<ZarinPalVerifyResponse>(responseString)!;
        }
    }

    // مدل‌های داده
    public class ZarinPalRequest
    {
        public string? MerchantId { get; set; }
        public int Amount { get; set; }
        public string? CallbackUrl { get; set; }
        public string? Description { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
    }

    public class ZarinPalRequestResponse
    {
        public Data? data { get; set; }
        public object? errors { get; set; }
    }

    public class ZarinPalVerifyRequest
    {
        public string? MerchantId { get; set; }
        public int Amount { get; set; }
        public string? Authority { get; set; }
    }

    public class ZarinPalVerifyResponse
    {
        public Data? data { get; set; }
        public object? errors { get; set; }
    }

    public class Data
    {
        public int code { get; set; }
        public string? message { get; set; }
        public string? authority { get; set; }
        public string? fee_type { get; set; }
        public int fee { get; set; }
        public long ref_id { get; set; }
    }
}
