using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using Taller.Common;
using Taller.Models;

namespace Taller.RestModel
{

    public class Condition
    {
        public string code { get; set; }
        public string date { get; set; }
        public string temp { get; set; }
        public string text { get; set; }
    }

    public class Item
    {
        public Condition condition { get; set; }
    }

    public class Channel
    {
        public Item item { get; set; }
    }

    public class Results
    {
        public Channel channel { get; set; }
    }

    public class Query
    {
        public int count { get; set; }
        public DateTime created { get; set; }
        public string lang { get; set; }
        public Results results { get; set; }
    }

    public class RootObject
    {
        public Query query { get; set; }
    }

    public class TallerModel
    {
        public Response GetClima(string url)
        {
            Response res = new Response { type_of_response = type_of_responses.OK, message = "Success" };
            try
            {
                ApiRequests ApiRequest = new ApiRequests()
                {
                    sApiURL = url,
                    Method = RestSharp.Method.GET
                    //Headers = new Dictionary<string, string>(),
                };
                //ApiRequest.Headers.Add("Api-Token", api_token);
                Request req = ApiRequest.Send();
                var debugg = req.response;
                RootObject clima = new JavaScriptSerializer().Deserialize<RootObject>(req.response);
                //if (authuser.result.FirstOrDefault().CR.CodRet == "0399")
                //{
                //    res.type_of_response = type_of_responses.OK;
                //    res.message = authuser.result.FirstOrDefault().CR.DescCodRet;
                //}
                //else
                //{
                //    res.type_of_response = type_of_responses.ErrorResponse;
                //    res.message = authuser.result.FirstOrDefault().CR.DescCodRet;
                //    res.data = null;
                //}
                res.data = clima;
            }
            catch (Exception ex)
            {
                res.type_of_response = type_of_responses.ErrorResponse;
                res.message = ex.Message;
                res.data = null;
            }
            return res;
        }
    }
}