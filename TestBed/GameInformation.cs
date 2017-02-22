using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WhatToPlay.Model
{
    public class GameInformation
    {
        public string Name { get; set; }
        public long AppId { get; set; }
        public List<string> Tags { get; set; }
        public string StoreLink {
            get
            {
                return string.Format("http://store.steampowered.com/app/{0}/", AppId);
            }
        }
        
        public void RetrieveTags()
        {
            if (Tags == null)
                Tags = new List<string>();
            else
                Tags.Clear();

            HtmlWeb web = new HtmlWeb();
            HtmlDocument document = web.Load(StoreLink);
            List<HtmlNode> allTags = document.DocumentNode.SelectNodes("//div[@class='game_area_details_specs']").ToList();
            foreach(HtmlNode node in allTags)
            {
                string tag = node.InnerText;
                Tags.Add(tag);
            }
        }
    }
}
