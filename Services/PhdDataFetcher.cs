using System;
using System.Collections.Generic;
using Uniformance.PHD;

namespace SmboPhdService.Services
{
    public class PhdDataPoint
    {
        public string Tag { get; set; }
        public DateTime Timestamp { get; set; }
        public string Value { get; set; }
        public short Confidence { get; set; }
    }

    public class PhdDescriptionPoint
    {
        public string Tag { get; set; }
        public string Description { get; set; }
    }

    public class PhdFetchSettings
    {
        public string Sampletype { get; set; }
        public string ReductionType { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class PhdTagBaseInfo
    {
        public string TagName { get; set; }
        public int TagNo { get; set; }
    }

    public class PhdDataFetcher
    {
        public List<string> FindTags(string pattern, uint max_tags = 0, bool fabric = false)
        {
            try
            {
                using (var phd = new PHDHistorian())
                {
                    if (!fabric) 
                        phd.DefaultServer = PhdConnectionFactory.GetServer();
                    else 
                        phd.DefaultServer = PhdConnectionFactory.CreateServer();

                    var filter = new TagFilter { Tagname = $"*{pattern.Replace("*", "")}*" };
                    var tags = phd.BrowsingTagsList(max_tags, filter);
                    var result = new List<string>();

                    foreach (var t in tags)
                    {
                        if (t is TagListStruct tagStruct)
                            result.Add(tagStruct.Tagname);
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PHD Error find tags: " + ex.Message);
            }
        }

        public List<string> FetchTagsByPatterns(IEnumerable<string> patterns, uint max_tags = 0, bool fabric = false)
        {
            var result = new List<string>();

            try
            {
                using (var phd = new PHDHistorian())
                {
                    if (!fabric)
                        phd.DefaultServer = PhdConnectionFactory.GetServer();
                    else
                        phd.DefaultServer = PhdConnectionFactory.CreateServer();

                    foreach (var pattern in patterns)
                    {
                        var filter = new TagFilter { Tagname = $"*{pattern.Replace("*", "")}*" };
                        var tags = phd.BrowsingTagsList(max_tags, filter);
                        foreach (var t in tags)
                            if (t is TagListStruct tagStruct)
                                result.Add(tagStruct.Tagname);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PHD Error fetch tags by pattern: " + ex.Message);
            }
            return result;
        }

        public List<PhdDataPoint> FetchDataList(List<string> tagsList, PhdFetchSettings settings, bool fabric = false)
        {
            try
            {
                using (var phd = new PHDHistorian())
                {
                    if (!fabric)
                        phd.DefaultServer = PhdConnectionFactory.GetServer();
                    else
                        phd.DefaultServer = PhdConnectionFactory.CreateServer();

                    phd.Sampletype = Enum.TryParse(settings.Sampletype, true, out SAMPLETYPE st) ? st : SAMPLETYPE.Raw;
                    phd.ReductionType = Enum.TryParse(settings.ReductionType, true, out REDUCTIONTYPE rt) ? rt : REDUCTIONTYPE.None;
                    phd.StartTime = settings.StartTime ?? "NOW-1H";
                    phd.EndTime = settings.EndTime ?? "NOW";

                    var tags = new Tags();
                    foreach (var tagName in tagsList)
                        tags.Add(new Tag(tagName));

                    var ds = phd.FetchRowData(tags);
                    var results = new List<PhdDataPoint>();

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        foreach (System.Data.DataRow row in ds.Tables[0].Rows)
                        {
                            results.Add(new PhdDataPoint
                            {
                                Tag = row["TagName"].ToString(),
                                Timestamp = row["TimeStamp"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(row["TimeStamp"]),
                                Value = row["Value"].ToString(),
                                Confidence = row["Confidence"] == DBNull.Value ? (short)-1 : Convert.ToInt16(row["Confidence"])
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PHD Error fetch data list: " + ex.Message);
            }
        }

        public PhdDataPoint PutTagValue(string tagName, double value, DateTime? timestamp = null, sbyte? confidence = null, string units = null, bool fabric = false)
        {
            try
            {
                using (var phd = new PHDHistorian())
                {

                    if (!fabric)
                        phd.DefaultServer = PhdConnectionFactory.GetServer();
                    else
                        phd.DefaultServer = PhdConnectionFactory.CreateServer();

                    phd.DefaultServer = PhdConnectionFactory.GetServer();

                    var tag = new Tag { TagName = tagName };
                    var ts = timestamp ?? DateTime.UtcNow;
                    var conf = confidence ?? (sbyte)100;
                    var unit = units ?? string.Empty;

                    phd.PutData(tag, value, ts, conf, unit);

                    return new PhdDataPoint
                    {
                        Tag = tagName,
                        Timestamp = ts,
                        Value = value.ToString(),
                        Confidence = conf
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PHD Error put tag value: " + ex.Message);
            }
        }

        public List<PhdDataPoint> FetchLastDataByPattern(string pattern)
        {
            var tags = FindTags(pattern);
            if (tags == null || tags.Count == 0)
                return new List<PhdDataPoint>();
            var settings = new PhdFetchSettings
            {
                Sampletype = "Raw",
                ReductionType = "None",
                StartTime = "NOW",
                EndTime = "NOW"
            };
            return FetchDataList(tags, settings);
        }

        public List<PhdDescriptionPoint> FetchDescriptionList(List<string> tagsList, bool fabric = false)
        {
            var results = new List<PhdDescriptionPoint>();
            try
            {
                using (var phd = new PHDHistorian())
                {
                    if (!fabric)
                        phd.DefaultServer = PhdConnectionFactory.GetServer();
                    else
                        phd.DefaultServer = PhdConnectionFactory.CreateServer();

                    var tags = new Tags();
                    foreach (var tagName in tagsList)
                        tags.Add(new Tag(tagName));

                    var ds = phd.TagDfn(tags);

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        var table = ds.Tables[0];
                        foreach (System.Data.DataRow row in table.Rows)
                        {
                            results.Add(new PhdDescriptionPoint
                            {
                                Tag = row["TagName"]?.ToString(),
                                Description = table.Columns.Contains("Description")
                                    ? row["Description"]?.ToString()
                                    : (table.Columns.Contains("L_DESCR")
                                        ? row["L_DESCR"]?.ToString()
                                        : "N/A")
                            });
                        }
                    }
                    return results;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PHD Error fetch description list: " + ex.Message);
            }
        }

        public List<PhdDescriptionPoint> FetchDescriptionsByPattern(string pattern)
        {
            var tags = FindTags(pattern);
            if (tags == null || tags.Count == 0)
                return new List<PhdDescriptionPoint>();
            return FetchDescriptionList(tags);
        }

        public List<PhdTagBaseInfo> FindTagsExtended(string pattern, uint max_tags = 0, bool fabric = false)
        {
            try
            {
                using (var phd = new PHDHistorian())
                {
                    if (!fabric)
                        phd.DefaultServer = PhdConnectionFactory.GetServer();
                    else
                        phd.DefaultServer = PhdConnectionFactory.CreateServer();

                    var filter = new TagFilter { Tagname = $"*{pattern.Replace("*", "")}*" };
                    var tags = phd.BrowsingTagsList(max_tags, filter);
                    var result = new List<PhdTagBaseInfo>();

                    foreach (var t in tags)
                    {
                        if (t is TagListStruct tagStruct)
                        {
                            result.Add(new PhdTagBaseInfo
                            {
                                TagName = tagStruct.Tagname,
                                TagNo = tagStruct.Tagno
                            });
                        }
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("PHD Error find tags extended: " + ex.Message);
            }
        }
    }
}