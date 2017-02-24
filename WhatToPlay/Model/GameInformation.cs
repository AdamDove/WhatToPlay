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
        public GameInformation(string name, long id)
        {
            Name = name;
            AppId = id;
        }

        public string Name { get; set; }
        public long AppId { get; set; }
        public List<string> Tags { get; private set; }
        public string CurrentPrice { get; private set; }
        public string Discount { get; private set; }

        public string StoreLink
        {
            get
            {
                return string.Format("http://store.steampowered.com/app/{0}/", AppId);
            }
        }

        public void RetrieveStoreInformation()
        {
            try
            {
                if (Tags == null)
                    Tags = new List<string>();
                else
                    Tags.Clear();

                HtmlWeb web = new HtmlWeb();
                HtmlDocument storePage = web.Load(StoreLink);

                //Get Tags
                List<HtmlNode> allTags = storePage.DocumentNode.SelectNodes("//div[@class='game_area_details_specs']").ToList();
                foreach (HtmlNode node in allTags)
                {
                    string tag = node.InnerText;
                    Tags.Add(tag);
                }

                //Get Price Information
                HtmlNode price = storePage.DocumentNode.SelectSingleNode("//div[@class='game_purchase_action_bg']");
                HtmlNode noDiscountPrice = price.SelectSingleNode("//div[@class='game_purchase_price price']");
                if (noDiscountPrice != null)
                {
                    CurrentPrice = noDiscountPrice.InnerText;
                    Discount = String.Empty;
                }
                else
                {
                    HtmlNode discountPrice = price.SelectSingleNode("//div[@class='discount_block game_purchase_discount']");
                    Discount = discountPrice.SelectSingleNode("//div[@class='discount_pct']").InnerText;
                    CurrentPrice = discountPrice.SelectSingleNode("//div[@class='discount_final_price']").InnerText;
                }
            }
            catch (Exception generalException)
            {
                Console.WriteLine($"Exception retrieving store information: {generalException.Message}");
                Console.WriteLine($"StackTrace: {generalException.StackTrace}");
            }

        }
    }
}
