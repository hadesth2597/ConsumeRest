using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Taller.Models;
using System.IO;
using System.ComponentModel;
using System.Timers;

namespace Taller.Common
{


    public class ManualTimer : System.Timers.Timer
    {
        public int InternalCount = 0;
        public ManualTimer()
        {
            this.Interval = 10;
            //Default
            this.Enabled = false;
            this.Elapsed += new ElapsedEventHandler(Handler);
        }

        public virtual void Handler(object sender, ElapsedEventArgs e)
        {
            this.InternalCount += 1;
        }
    }

    public class Response
    {
        public string message;
        public object data;
        public type_of_responses type_of_response;
        public ResponseStatus response_status;
        public string peticion;
    }

    public class Request
    {
        public string response;
        public string absolute_path;
        public type_of_responses type_of_response;
        public ResponseStatus response_status;
        public string request_name { get; set; }
        public Uri response_uri { get; set; }
    }

    public enum type_of_responses
    {
        OK = 0,
        ErrorResponse = 1,
        Other = 2,
        TimeOutException = 3
    }

    public class ApiRequests
    {

        public string sApiURL { get; set; } = Configurations.ServerUrl;
        /// <summary>
        /// Internal device for logs
        /// </summary>
        public Nullable<int> internal_device_id;
        public string sRequestName = "";

        private Request Responses = new Request();
        public string HeaderName
        {
            get { return m_HeaderName; }
            set { m_HeaderName = value; }
        }
        private string m_HeaderName = Configurations.HeaderName;
        public string HeaderValue
        {
            get { return m_HeaderValue; }
            set { m_HeaderValue = value; }
        }
        public Dictionary<string, string> Headers;
        private string m_HeaderValue = Configurations.HeaderValue;
        public object parameters;
        //Public parameters() As Newtonsoft.Json.Linq.JObject
        public Method Method = RestSharp.Method.POST;

        public string serialize = null;

        /// <summary>
        /// Sends sync requests
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        public Request Send()
        {
            {
                Request ServerRequest = new Request();
                try
                {
                    RestClient client = new RestClient();
                    //client.Proxy = Nothing
                    //Responses.AbsolutePath = sApiURL;
                    IRestRequest Request = new RestRequest();
                    switch (this.Method)
                    {
                        case Method.GET:
                            if (this.parameters != null)
                            {
                                client.BaseUrl = new Uri(this.sApiURL + this.parameters.ToString());
                            }
                            else
                            {
                                client.BaseUrl = new Uri(this.sApiURL);
                            }
                            break;
                        case Method.POST:
                            Request.Method = Method.POST;
                            client.BaseUrl = new Uri(this.sApiURL);
                            if (this.serialize == null && this.parameters != null)
                            {
                                Request.AddObject(this.parameters);
                            }
                            else
                            {
                                var json = JsonConvert.SerializeObject(this.parameters);
                                Request.AddParameter(this.serialize, json, ParameterType.RequestBody);
                            }
                            //Request.AddBody(parameters);
                            //Request.AddJsonBody(parameters);
                            break;
                        default:
                            break;
                    }
                    Request.Timeout = 25000;
                    if (Headers != null)
                    {
                        if (Headers.Count > 0)
                        {
                            foreach (var header in Headers)
                            {
                                Request.AddHeader(header.Key, header.Value);
                            }
                        }
                    }
                    var response = client.Execute(Request);
                    ServerRequest.type_of_response = response.ResponseStatus == RestSharp.ResponseStatus.Completed ? type_of_responses.OK : type_of_responses.ErrorResponse;
                    ServerRequest.response = response.Content;
                }
                catch (WebException ex)
                {
                    if (ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                        ServerRequest.type_of_response = type_of_responses.TimeOutException;
                    else
                        ServerRequest.type_of_response = type_of_responses.ErrorResponse;
                }
                catch (Exception ex)
                {
                    ServerRequest.type_of_response = type_of_responses.ErrorResponse;
                }
                return ServerRequest;
            }
        }

        /// <summary>
        /// Sends Async requests
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        public Request SendAsync()
        {
            Request ServerRequest = new Request();
            try
            {
                RestClient client = new RestClient();
                IRestRequest Request = new RestSharp.RestRequest();
                Request.Method = this.Method;
                switch (this.Method)
                {
                    case Method.GET:
                        client.BaseUrl = new Uri(this.sApiURL + this.parameters.ToString());
                        break; // TODO: might not be correct. Was : Exit Select
                    case Method.POST:
                        client.BaseUrl = new Uri(this.sApiURL);
                        if (this.serialize == null)
                        {
                            //Request.AddBody(ApiRequest.parameters)
                            string jsonString = JsonConvert.SerializeObject(this.parameters);
                            Request.AddParameter("text/json", jsonString, ParameterType.RequestBody);
                        }
                        else
                        {
                            string jsonString = JsonConvert.SerializeObject(this.parameters);
                            Request.AddParameter(this.serialize, jsonString, ParameterType.RequestBody);
                        }
                        //Request.AddBody(parameters);
                        //Request.AddJsonBody(parameters);
                        break; // TODO: might not be correct. Was : Exit Select

                    default:
                        break; // TODO: might not be correct. Was : Exit Select

                }
                Request.Timeout = 25000;
                Request.AddHeader(this.HeaderName, this.HeaderValue);
                client.ExecuteAsync(Request, response =>
                {
                    try
                    {
                        Responses.type_of_response = type_of_responses.ErrorResponse;
                        Responses.response_status = response.ResponseStatus;

                        switch (response.ResponseStatus)
                        {
                            case ResponseStatus.Completed:
                                Responses.type_of_response = type_of_responses.OK;
                                Responses.response = response.Content;
                                Responses.absolute_path = response.ResponseUri.AbsolutePath;
                                break;
                            case ResponseStatus.Error:
                                Responses.response = "Request error";
                                break;
                            case ResponseStatus.TimedOut:
                                Responses.type_of_response = type_of_responses.TimeOutException;
                                Responses.response = "No connection to server";
                                break;
                            case ResponseStatus.Aborted:
                                Responses.response = "Connection aborted";
                                break;
                        }

                        DoAsyncActions(Responses);

                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                {
                    ServerRequest.type_of_response = type_of_responses.TimeOutException;
                }
                else
                {
                    ServerRequest.type_of_response = type_of_responses.ErrorResponse;
                }
            }
            catch (Exception ex)
            {
                ServerRequest.type_of_response = type_of_responses.ErrorResponse;
            }
            return ServerRequest;
        }

        /// <summary>
        /// Sends async requests and do nothing
        /// </summary>
        /// <param name="ApiRequest"></param>
        /// <returns></returns>
        public Request SendAsync2()
        {
            Request ServerRequest = new Request();
            try
            {
                RestClient client = new RestClient();
                IRestRequest Request = new RestSharp.RestRequest();
                Request.Method = this.Method;
                switch (this.Method)
                {
                    case Method.GET:
                        client.BaseUrl = new Uri(this.sApiURL + this.parameters.ToString());
                        break; // TODO: might not be correct. Was : Exit Select
                    case Method.POST:
                        client.BaseUrl = new Uri(this.sApiURL);
                        if (this.serialize == null)
                        {
                            //Request.AddBody(ApiRequest.parameters)
                            string jsonString = JsonConvert.SerializeObject(this.parameters);
                            Request.AddParameter("text/json", jsonString, ParameterType.RequestBody);
                        }
                        else
                        {
                            string jsonString = JsonConvert.SerializeObject(this.parameters);
                            Request.AddParameter(this.serialize, jsonString, ParameterType.RequestBody);
                        }
                        //Request.AddBody(parameters);
                        //Request.AddJsonBody(parameters);
                        break; // TODO: might not be correct. Was : Exit Select
                    default:
                        break; // TODO: might not be correct. Was : Exit Select
                }
                Request.Timeout = 25000;
                Request.AddHeader(this.HeaderName, this.HeaderValue);
                client.ExecuteAsync(Request, response =>
                {
                    try
                    {
                        Request Responses = new Request();
                        Responses.type_of_response = type_of_responses.ErrorResponse;
                        Responses.response_status = response.ResponseStatus;

                        switch (response.ResponseStatus)
                        {
                            case ResponseStatus.Completed:
                                Responses.type_of_response = type_of_responses.OK;
                                Responses.response = response.Content;
                                Responses.absolute_path = response.ResponseUri.AbsolutePath;
                                break;
                            case ResponseStatus.Error:
                                Responses.response = "Request error";
                                break;
                            case ResponseStatus.TimedOut:
                                Responses.type_of_response = type_of_responses.TimeOutException;
                                Responses.response = "No connection to server";
                                break;
                            case ResponseStatus.Aborted:
                                Responses.response = "Connection aborted";
                                break;
                        }
                        DoAsyncActions2(Responses);
                    }
                    catch (Exception ex)
                    {
                    }
                });
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.Timeout)
                {
                    ServerRequest.type_of_response = type_of_responses.TimeOutException;
                }
                else
                {
                    ServerRequest.type_of_response = type_of_responses.ErrorResponse;
                }
            }
            catch (Exception ex)
            {
                ServerRequest.type_of_response = type_of_responses.ErrorResponse;
            }
            return ServerRequest;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="Responses"></param>
        public void DoAsyncActions(Request Responses)
        {
            try
            {
                Responses.absolute_path = (Responses.absolute_path == null ? sRequestName : Responses.absolute_path);
                //If Responses.type_of_response = type_of_responses.OK Then
                if ((sRequestName == "UpdateOutDate"))
                {
                    //Reservation Reservation = CheckReservationResponse(Responses);
                    //CommonClass.set_log("DoAsynActions -> AbsoluthPath -> type_of_responses:" + Reservation.type_of_responses.ToString());
                }

            }
            catch (Exception ex)
            {
            }
        }

        public void DoAsyncActions2(Request Responses)
        {
            try
            {
                Responses.absolute_path = (Responses.absolute_path == null ? sRequestName : Responses.absolute_path);
                //If Responses.type_of_response = type_of_responses.OK Then
                if ((sRequestName == "UpdateOutDate"))
                {
                    //Reservation Reservation = CheckReservationResponse(Responses);
                    //CommonClass.set_log("DoAsynActions -> AbsoluthPath -> type_of_responses:" + Reservation.type_of_responses.ToString());
                }

            }
            catch (Exception ex)
            {
            }
        }
    }

}