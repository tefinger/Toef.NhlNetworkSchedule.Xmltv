using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using Toef.NhlNetworkSchedule.Xmltv.Models;

namespace Toef.NhlNetworkSchedule.Xmltv.Services
{
    public class ScheduleService
    {
        // https://www-league.nhlstatic.com/nhlNetwork/epg/nhlBAM.xml
        private static readonly HttpClient client = new HttpClient();

        public ScheduleService()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246");
        }

        public async Task Update(string outputFilePath)
        {
            var xmlSchedule = await FetchSchedule();
            var shows = ParseXmlSchedule(xmlSchedule);
            var xmltv = GenerateXmltv(shows);
            await File.WriteAllTextAsync(outputFilePath, xmltv);
        }

        async Task<string> FetchSchedule()
        {
            var xmlContent = await client.GetStringAsync("https://www-league.nhlstatic.com/nhlNetwork/epg/nhlBAM.xml");
            return xmlContent;
        }

        IList<Show> ParseXmlSchedule(string xmlSchedule)
        {
            XDocument doc = XDocument.Parse(xmlSchedule);
            var nhlElement = doc.Element("NHL");
            var nhlShows = nhlElement.Elements();
            var shows = new List<Show>();

            foreach (var show in nhlShows)
            {
                var startString = $"{show.Element("StartDate").Value} {show.Element("StartTime").Value} -5:00";
                var endString = $"{show.Element("EndDate").Value} {show.Element("EndTime").Value} -5:00";

                var s = new Show();
                s.Start = DateTimeOffset.Parse(startString);
                s.End = DateTimeOffset.Parse(endString);
                s.ShowTitle = show.Element("ShowTitle").Value;
                s.ShowDescription = show.Element("ShowDescription").Value;
                s.Live = show.Element("Live").Value;
                s.Icon = show.Element("ShowImage").Value;
                shows.Add(s);
            }
            return shows;
        }

        string GenerateXmltv(IList<Show> shows)
        {
            const string channelId = "nhl_network";
            const string channelName = "NHL Network";

            var programme = new StringBuilder();

            foreach (var show in shows)
            {
                var start = show.Start.ToUniversalTime().ToString("yyyyMMddHHmmss");
                var end = show.End.ToUniversalTime().ToString("yyyyMMddHHmmss");

                programme.Append($"  <programme start=\"{start}\" stop=\"{end}\" channel=\"{channelId}\">");
                programme.Append(Environment.NewLine);
                programme.Append($"    <title lang=\"en\">{HttpUtility.HtmlEncode(show.ShowTitle)}</title>");
                if (!string.IsNullOrEmpty(show.Live))
                {
                    programme.Append(Environment.NewLine);
                    programme.Append($"    <sub-title lang=\"en\">{show.Live}</sub-title>");
                }
                programme.Append(Environment.NewLine);
                programme.Append($"    <desc lang=\"en\">{HttpUtility.HtmlEncode(show.ShowDescription)}</desc>");
                programme.Append(Environment.NewLine);
                programme.Append($"    <category lang=\"en\">Sports</category>");
                programme.Append(Environment.NewLine);
                programme.Append($"    <icon src=\"https://cms.nhl.bamgrid.com/nhlNetwork/images/{show.Icon}\" />");
                programme.Append(Environment.NewLine);
                programme.Append($"  </programme>");
                programme.Append(Environment.NewLine);
            }

            var programmeString = programme.ToString();

            var xmltv = new StringBuilder();
            xmltv.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xmltv.Append(Environment.NewLine);
            xmltv.Append("<tv generator-info-name=\"Toef.NhlNetworkSchedule.Xmltv\" generator-info-url=\"https://efinger.dev\">");
            xmltv.Append(Environment.NewLine);
            xmltv.Append($"  <channel id=\"{channelId}\">");
            xmltv.Append(Environment.NewLine);
            xmltv.Append($"    <display-name lang=\"en\">{channelName}</display-name>");
            xmltv.Append(Environment.NewLine);
            xmltv.Append($"  </channel>");
            xmltv.Append(Environment.NewLine);
            xmltv.Append(programmeString);
            xmltv.Append($"</tv>");

            return xmltv.ToString();
        }
    }
}
