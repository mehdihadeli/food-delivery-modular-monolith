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
        if (name == "k8s-cluster-service" || name == "health-checks-webhooks" || name == "health-checks")
            return new HttpClient();

        return _httpClientProvider(name);
    }
}
