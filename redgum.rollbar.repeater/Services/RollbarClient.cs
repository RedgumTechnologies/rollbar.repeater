using Newtonsoft.Json;
using Rollbar;
using Rollbar.Diagnostics;
using Rollbar.DTOs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace redgum.rollbar.repeater.Services
{
    //this class is an almost copy of the 'real' RollBar client at https://github.com/rollbar/Rollbar.NET/blob/master/Rollbar/RollbarClient.cs
    //it's been modified to take a dynamic instead of a Rollbar.DTOs.Payload for PostAsJsonAsync and it doesn't do scrubbing cause thats not the job of a repeater
    //because of that its had some unused code removed (commented out)
    internal class RollbarClient 
    {
        public RollbarConfig Config { get; }

        public RollbarClient(RollbarConfig config)
        {
            Assumption.AssertNotNull(config, nameof(config));

            Config = config;
        }

        //public RollbarResponse PostAsJson(Payload payload, IEnumerable<string> scrubFields)
        //{
        //    Assumption.AssertNotNull(payload, nameof(payload));

        //    var task = this.PostAsJsonAsync(payload, scrubFields);

        //    task.Wait();

        //    return task.Result;
        //}

        //public async Task<RollbarResponse> PostAsJsonAsync(Payload payload, IEnumerable<string> scrubFields)
        //{
        //    Assumption.AssertNotNull(payload, nameof(payload));

        //    using (var httpClient = this.BuildWebClient())
        //    {
        //        var jsonData = JsonConvert.SerializeObject(payload);
        //        //jsonData = ScrubPayload(jsonData, scrubFields); //change from original, we don't do scrub fields at this level

        //        httpClient.DefaultRequestHeaders
        //            .Add("X-Rollbar-Access-Token", payload.AccessToken);

        //        httpClient.DefaultRequestHeaders
        //            .Accept
        //            .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header

        //        var postPayload = 
        //            new StringContent(jsonData, Encoding.UTF8, "application/json"); //CONTENT-TYPE header
        //        var uri = new Uri($"{Config.EndPoint}item/");
        //        var postResponse = await httpClient.PostAsync(uri, postPayload);

        //        RollbarResponse response = null;
        //        if (postResponse.IsSuccessStatusCode)
        //        {
        //            string reply = await postResponse.Content.ReadAsStringAsync();
        //            response = JsonConvert.DeserializeObject<RollbarResponse>(reply);
        //            response.HttpDetails = 
        //                $"Response: {postResponse}" 
        //                + Environment.NewLine 
        //                + $"Request: {postResponse.RequestMessage}" 
        //                + Environment.NewLine
        //                ;
        //        }
        //        else
        //        {
        //            postResponse.EnsureSuccessStatusCode();
        //        }

        //        return response;
        //    }
        //}

        ////change from original, using a dynamic for payload instead of Rollbar.DTOs.Payload
        public async Task<RollbarResponse> PostAsJsonAsync(dynamic payload, IEnumerable<string> scrubFields)
        {
            Assumption.AssertNotNull(payload, nameof(payload));

            using (var httpClient = this.BuildWebClient())
            {
                var jsonData = JsonConvert.SerializeObject(payload);
                //jsonData = ScrubPayload(jsonData, scrubFields); //change from original, we don't do scrub fields at this level

                httpClient.DefaultRequestHeaders
                    .Add("X-Rollbar-Access-Token", payload.access_token); //change from original, we're using a dynamic that has been through a json serialiser, AccessToken is serialised as access_token

                httpClient.DefaultRequestHeaders
                    .Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json")); //ACCEPT header

                var postPayload =
                    new StringContent(jsonData, Encoding.UTF8, "application/json"); //CONTENT-TYPE header
                var uri = new Uri($"{Config.EndPoint}item/");
                var postResponse = await httpClient.PostAsync(uri, postPayload);

                RollbarResponse response = null;
                if (postResponse.IsSuccessStatusCode)
                {
                    string reply = await postResponse.Content.ReadAsStringAsync();
                    response = JsonConvert.DeserializeObject<RollbarResponse>(reply);
                    response.HttpDetails =
                        $"Response: {postResponse}"
                        + Environment.NewLine
                        + $"Request: {postResponse.RequestMessage}"
                        + Environment.NewLine
                        ;
                }
                else
                {
                    postResponse.EnsureSuccessStatusCode();
                }

                return response;
            }
        }
        //private string ScrubPayload(string payload, IEnumerable<string> scrubFields)
        //{
        //    var jObj = JsonScrubber.CreateJsonObject(payload);
        //    var dataProperty = JsonScrubber.GetChildPropertyByName(jObj, "data");
        //    JsonScrubber.ScrubJson(dataProperty, scrubFields);
        //    var scrubbedPayload = jObj.ToString();
        //    return scrubbedPayload;
        //}

        private HttpClient BuildWebClient()
        {
            HttpClient webClient = null;

            var webProxy = this.BuildWebProxy();

            if (webProxy != null)
            {
                var httpHandler = new HttpClientHandler();
                httpHandler.Proxy = webProxy;
                httpHandler.PreAuthenticate = true;
                httpHandler.UseDefaultCredentials = false;

                webClient = new HttpClient(httpHandler);
            }
            else
            {
                webClient = new HttpClient();
            }

            return webClient;
        }

        private IWebProxy BuildWebProxy()
        {
            if (!string.IsNullOrWhiteSpace(this.Config.ProxyAddress))
            {
                return new WebProxy(this.Config.ProxyAddress);
            }

            return null;
        }
    }
}