using Microsoft.Extensions.Http;

namespace Tests.Shared.Fixtures;

public class DelegateHttpClientFactory : IHttpClientFactory
{
    private readonly Func<string, HttpClient> _httpClientProvider;

    public DelegateHttpClientFactory(Func<string, HttpClient> httpClientProvider)
    {
        _httpClientProvider = httpClientProvider;
    }

    public HttpClient CreateClient(string name)
    {
        return _httpClientProvider(name);
    }
}

// public class  M:ITypedHttpClientFactory<>
