using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace NBB.Mediator.OpenFaaS
{
    public class OpenFaaSMediator : IMediator
    {
        private readonly string _gatewayUrl, _events_format, _commands_format;
        private readonly ILogger<OpenFaaSMediator> _logger;

        public OpenFaaSMediator(IConfiguration configuration, ILogger<OpenFaaSMediator> logger)
        {
            _gatewayUrl = configuration.GetSection("OpenFaaS")["gateway_url"];
            _events_format = configuration.GetSection("OpenFaaS")["events_format"];
            _commands_format = configuration.GetSection("OpenFaaS")["commands_format"];
            _logger = logger;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task Send(IRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            var functionName = string.Format(_commands_format, request.GetType().Name.ToLower());
            var url = $"{_gatewayUrl}/{functionName}";
            await InvokeFunction(url, cancellationToken);
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification
        {
            var functionName = string.Format(_events_format, notification.GetType().Name.ToLower());
            var url = $"{_gatewayUrl}/{functionName}";
            await InvokeFunction(url, cancellationToken);
        }

        private async Task InvokeFunction(string url, CancellationToken cancellationToken)
        {
            using (HttpClient cl = new HttpClient())
            {
                try
                {
                    var get = await cl.GetAsync(url, cancellationToken);
                    get.EnsureSuccessStatusCode();
                }
                catch (HttpRequestException e)
                {
                    _logger.LogError($"Function error: {url}");
                    _logger.LogError(e.Message);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    throw;
                }
            }
        }
    }
}