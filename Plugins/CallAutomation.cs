
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using CsvHelper.Configuration.Attributes;
using Microsoft.Azure.CognitiveServices.Search.WebSearch;
using Microsoft.SemanticKernel;
using Options;

namespace Plugins;

public class CallAutomation
{

    private string _apiKey;
    private string _baseUrl;
    private readonly HttpClient _client;
    // create a constructor and initialize _client insance with baseUrl
    public CallAutomation(CallAutomationApiOptions callAutomationApiOptions)
    {
        _apiKey = callAutomationApiOptions.ApiKey;
        _baseUrl = callAutomationApiOptions.BaseUrl;
        _client = new HttpClient()
        {
            BaseAddress = new Uri(_baseUrl)
        };
    }

    [KernelFunction("Call")]
    [Description("make an api call and provide the name of the person to call and the message to send them")]
    public async Task CallAsync(string name, string message)
    {
        var response = await _client.PostAsync($"CallAutomation/StartOutboundCall/customername/{name}/message/{message}/key/{_apiKey}", null).ConfigureAwait(false);
        if(!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to make call to {name}");
        }
    }


}