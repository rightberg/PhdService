using Serilog;
using SmboPhdService.Services;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace SmboPhdService.Controllers
{
    public class PhdServiceController : ApiController
    {
        private static readonly ILogger _log_api = Log.ForContext("api", true);
        private static readonly ILogger _log = Log.ForContext("phd-api", true);

        public class PutTagRequest
        {
            public string TagName { get; set; }
            public double Value { get; set; }
            public DateTime? Timestamp { get; set; }
            public sbyte? Confidence { get; set; }
            public string Units { get; set; }
        }

        public class TagRequest
        {
            public string SampleType { get; set; } = "Raw";
            public string ReductionType { get; set; } = "None";
            public string StartTime { get; set; } = "NOW-1H";
            public string EndTime { get; set; } = "NOW";
            public List<string> Tags { get; set; }
        }

        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            _log_api.Information("[{rid}][{ip}] Ping", rid, ip);

            return Ok("pong");
        }

        [HttpGet]
        [Route("api/tags/find")]
        public IHttpActionResult FindTags([FromUri] string pattern)
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            if (string.IsNullOrWhiteSpace(pattern))
            {
                _log.Error("[{rid}][{ip}] Incorrect data: tags/find", rid, ip);
                return BadRequest("Search pattern is empty");
            }

            try
            {
                var fetcher = new PhdDataFetcher();
                var result = fetcher.FindTags(pattern);

                _log.Information("[{rid}][{ip}] Complete successfully: tags/find (pattern={pattern})", rid, ip, pattern);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{rid}][{ip}] Error: tags/find", rid, ip);

                if (ex.Message.Contains("PHD Error"))
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, ex.Message);
                }

                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/tags/find/with-tagno")]
        public IHttpActionResult FindTagsExtended([FromUri] string pattern)
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            if (string.IsNullOrWhiteSpace(pattern))
            {
                _log.Error("[{rid}][{ip}] Incorrect data: tags/find/with-tagno", rid, ip);
                return BadRequest("Search pattern is empty");
            }

            try
            {
                var fetcher = new PhdDataFetcher();
                var result = fetcher.FindTagsExtended(pattern);

                _log.Information("[{rid}][{ip}] Complete successfully: tags/find/with-tagno (pattern={pattern})", rid, ip, pattern);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{rid}][{ip}] Error: tags/find/with-tagno", rid, ip);

                if (ex.Message.Contains("PHD Error"))
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, ex.Message);
                }

                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/tags/list")]
        public IHttpActionResult GetDataList([FromBody] TagRequest request)
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            if (request?.Tags == null || request.Tags.Count == 0)
            {
                _log.Error("[{rid}][{ip}] Incorrect data: tags/list", rid, ip);
                return BadRequest("Tag list is empty");
            }

            try
            {
                var fetcher = new PhdDataFetcher();
                var settings = new PhdFetchSettings
                {
                    Sampletype = request.SampleType,
                    ReductionType = request.ReductionType,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime
                };
                    
                    var results = fetcher.FetchDataList(request.Tags, settings);

                    _log.Information("[{rid}][{ip}] Complete successfully: tags/list {tags}", rid, ip, request.Tags);

                    return Ok(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{rid}][{ip}] Error: tags/list {tags}", rid, ip, request.Tags);

                if (ex.Message.Contains("PHD Error"))
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, ex.Message);
                }

                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/tags/last")]
        public IHttpActionResult FindLastDataTags([FromUri] string pattern)
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            if (string.IsNullOrWhiteSpace(pattern))
            {
                _log.Error("[{rid}][{ip}] Incorrect data: tags/last", rid, ip);
                return BadRequest("Search pattern is empty");
            }

            try
            {
                var fetcher = new PhdDataFetcher();
                var result = fetcher.FetchLastDataByPattern(pattern);

                _log.Information("[{rid}][{ip}] Complete successfully: tags/last (pattern={pattern})", rid, ip, pattern);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{rid}][{ip}] Error: tags/last", rid, ip);

                if (ex.Message.Contains("PHD Error"))
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, ex.Message);
                }

                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/tags/put")]
        public IHttpActionResult PutTagData([FromBody] PutTagRequest request)
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            if (request == null || string.IsNullOrWhiteSpace(request.TagName))
            {
                _log.Error("[{rid}][{ip}] Incorrect data: putdata", rid, ip);
                return BadRequest("Tag name is empty");
            }   

            try
            {
                var fetcher = new PhdDataFetcher();
                var result = fetcher.PutTagValue(request.TagName, request.Value, request.Timestamp, request.Confidence, request.Units);

                _log.Information("[{rid}][{ip}] Complete successfully: tags/put", rid, ip);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{rid}][{ip}] Error: tags/put", rid, ip);

                if (ex.Message.Contains("PHD Error"))
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, ex.Message);
                }

                return InternalServerError(ex);
            }
        }

        [HttpPost]
        [Route("api/descriptions/list")]
        public IHttpActionResult GetDescriptionsList([FromBody] TagRequest request)
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            if (request?.Tags == null || request.Tags.Count == 0)
            {
                _log.Error("[{rid}][{ip}] Incorrect data: descriptions/list", rid, ip);
                return BadRequest("Tag list is empty");
            }

            try
            {
                var fetcher = new PhdDataFetcher();
                var results = fetcher.FetchDescriptionList(request.Tags);

                _log.Information("[{rid}][{ip}] Complete successfully: descriptions/list", rid, ip);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{rid}][{ip}] Error: descriptions/list", rid, ip);

                if (ex.Message.Contains("PHD Error"))
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, ex.Message);
                }

                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("api/descriptions/by-pattern")]
        public IHttpActionResult GetDescriptionsByPattern([FromUri] string pattern)
        {
            var rid = LogHelper.Requestid;
            var ip = LogHelper.RequestIp;

            if (string.IsNullOrWhiteSpace(pattern))
            {
                _log.Error("[{rid}][{ip}] Incorrect data: descriptions/by-pattern", rid, ip);
                return BadRequest("Search pattern is empty");
            }

            try
            {
                var fetcher = new PhdDataFetcher();
                var results = fetcher.FetchDescriptionsByPattern(pattern);

                _log.Information("[{rid}][{ip}] Complete successfully: descriptions/by-pattern (pattern={pattern})", rid, ip, pattern);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "[{rid}][{ip}] Error: descriptions/by-pattern", rid, ip);

                if (ex.Message.Contains("PHD Error"))
                {
                    return Content(System.Net.HttpStatusCode.BadGateway, ex.Message);
                }

                return InternalServerError(ex);
            }
        }
    }
}