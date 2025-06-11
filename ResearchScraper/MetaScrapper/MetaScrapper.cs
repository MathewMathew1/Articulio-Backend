using HtmlAgilityPack;
using ResearchScrapper.Api.Models;


namespace ResearchScrapper.Api.Service
{
    public class MetaScraper
    {

        private readonly ILogger<MetaScraper> _logger;


        public MetaScraper(ILogger<MetaScraper> logger)
        {

            _logger = logger;


        }

        public ScrappedMetaData GetMetaDataFromUrl(string url)
        {
            try
            {
                var webGet = new HtmlWeb();
                var document = webGet.Load(url);
                var metaTags = document.DocumentNode.SelectNodes("//meta");

                string? description = null;
                string? title = null;

                if(metaTags == null) return new ScrappedMetaData { Description = null, Title = null };

                foreach (var tag in metaTags)
                {
                    var tagNameAttr = tag.Attributes["name"];
                    var tagPropertyAttr = tag.Attributes["property"];
                    var tagContentAttr = tag.Attributes["content"];

                    if (tagContentAttr == null) continue;

                    string content = tagContentAttr.Value;

                    if (tagNameAttr != null)
                    {
                        string name = tagNameAttr.Value.ToLower();

                        if (name == "title")
                        {
                            title = content;
                        }
                        else if (name == "description")
                        {
                            description = content;
                        }
                    }
                    else if (tagPropertyAttr != null)
                    {
                        string property = tagPropertyAttr.Value.ToLower();

                        if (property == "og:title")
                        {
                            title = content;
                        }
                        else if (property == "og:description")
                        {
                            description = content;
                        }
                    }
                }


                return new ScrappedMetaData { Description = description, Title = title };
            }
            catch (Exception e)
            {

                _logger.LogError($"{e}");
                return new ScrappedMetaData { Description = null, Title = null };
            }
        }
    }
}