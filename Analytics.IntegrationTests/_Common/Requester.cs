using Newtonsoft.Json;
using System.Text;
using System.Web;

namespace Analytics.IntegrationTests._Common;
public class Requester(HttpClient client)
{
    private readonly HttpClient _client = client;

    public async Task<HttpResponseMessage> GetAsync(
        Uri uri,
        object query = default,
        CancellationToken ct = default
    )
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"{uri}?{GetUrlString(query)}"),
            Method = HttpMethod.Get
        };

        return await _client.SendAsync(request, ct);
    }

    public async Task<(HttpResponseMessage httpMessage, TResponse response)> GetAsync<TResponse>(
        Uri uri,
        object query = default,
        CancellationToken ct = default
    )
    {
        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"{uri}?{GetUrlString(query)}"),
            Method = HttpMethod.Get
        };

        var httpMessage = await _client.SendAsync(request, ct);

        return await GetResultAsync<TResponse>(httpMessage, ct);
    }

    public async Task<HttpResponseMessage> PostAsync(
        Uri uri,
        object data,
        object query = default,
        CancellationToken ct = default
    )
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Default, "application/json");

        return await _client.PostAsync(new Uri($"{uri}?{GetUrlString(query)}"), content, ct);
    }

    public async Task<(HttpResponseMessage httpMessage, TResponse response)> PostAsync<TResponse>(
        Uri uri,
        object data,
        object query = default,
        CancellationToken ct = default
    )
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Default, "application/json");

        var httpMessage = await _client.PostAsync(new Uri($"{uri}?{GetUrlString(query)}"), content, ct);

        return await GetResultAsync<TResponse>(httpMessage, ct);
    }

    public async Task<HttpResponseMessage> PutAsync(
        Uri uri,
        object data,
        object query = default,
        CancellationToken ct = default
    )
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Default, "application/json");

        return await _client.PutAsync(new Uri($"{uri}?{GetUrlString(query)}"), content, ct);
    }

    public async Task<(HttpResponseMessage httpMessage, TResponse response)> PutAsync<TResponse>(
        Uri uri,
        object data,
        object query = default,
        CancellationToken ct = default
    )
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Default, "application/json");

        var httpMessage = await _client.PutAsync(new Uri($"{uri}?{GetUrlString(query)}"), content, ct);

        return await GetResultAsync<TResponse>(httpMessage, ct);
    }

    public async Task<HttpResponseMessage> PatchAsync(
        Uri uri,
        object data = default,
        object query = default,
        CancellationToken ct = default
    )
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Default, "application/json");

        return await _client.PatchAsync(new Uri($"{uri}?{GetUrlString(query)}"), content, ct);
    }

    public async Task<(HttpResponseMessage httpMessage, TResponse response)> PatchAsync<TResponse>(
        Uri uri,
        object data = default,
        object query = default,
        CancellationToken ct = default
    )
    {
        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.Default, "application/json");

        var httpMessage = await _client.PatchAsync(new Uri($"{uri}?{GetUrlString(query)}"), content, ct);

        return await GetResultAsync<TResponse>(httpMessage, ct);
    }

    public async Task<HttpResponseMessage> DeleteAsync(
        Uri uri,
        object query = default,
        CancellationToken ct = default
    )
    {
        return await _client.DeleteAsync(new Uri($"{uri}?{GetUrlString(query)}"), ct);
    }

    public async Task<(HttpResponseMessage httpMessage, TResponse response)> DeleteAsync<TResponse>(
        Uri uri,
        object query = default,
        CancellationToken ct = default
    )
    {
        var httpMessage = await _client.DeleteAsync(new Uri($"{uri}?{GetUrlString(query)}"), ct);

        return await GetResultAsync<TResponse>(httpMessage, ct);
    }

    private static async Task<(HttpResponseMessage httpMessage, TResponse response)> GetResultAsync<TResponse>(HttpResponseMessage httpMessage, CancellationToken ct = default)
    {
        var json = await httpMessage.Content.ReadAsStringAsync(ct);

        try
        {
            var settings = new JsonSerializerSettings
            {
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };

            var result = JsonConvert.DeserializeObject<TResponse>(json, settings);

            return (httpMessage, result);
        }
        catch (Exception e)
        {
            throw new Exception($"Could not deserialize object. Current JSON: {json}", e);
        }
    }

    private static string GetUrlString(object data = default)
    {
        if (data == null) return string.Empty;

        var parameters = new List<string>();
        var properties = data.GetType().GetProperties();
        foreach (var property in properties)
        {
            var value = property.GetValue(data);
            if (value == null) continue;

            var valueJson = value is string stringValue ? stringValue : JsonConvert.SerializeObject(value);
            parameters.Add($"{HttpUtility.UrlEncode(property.Name)}={HttpUtility.UrlEncode(valueJson)}");
        }

        return string.Join("&", parameters);
    }
}
