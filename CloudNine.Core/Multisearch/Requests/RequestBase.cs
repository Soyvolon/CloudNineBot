﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Configuration;

using HtmlAgilityPack;

namespace CloudNine.Core.Multisearch.Requests
{
    public class RequestBase
    {
        public HtmlDocument Result { get; set; }

        public string? Query { get; set; }
        public string? Title { get; set; }

        public List<string>? Authors { get; set; }
        public List<string>? Characters { get; set; }
        public List<string>? Relationships { get; set; }
        public List<string>? Fandoms { get; set; }
        public List<string>? OtherTags { get; set; }

        public string? Likes { get; set; }
        public string? Views { get; set; }
        public string? Comments { get; set; }
        public string? WordCount { get; set; }

        public string? UpdateBefore { get; set; }
        public string? PublishBefore { get; set; }

        public string? SortDir { get; set; }
        public string? SortBy { get; set; }
        public string? Rating { get; set; }
        public string? Status { get; set; }
        public string? Crossover { get; set; }

        public RequestBase()
        {
            Result = new HtmlDocument();
            Authors = new List<string>();
            Characters = new List<string>();
            Fandoms = new List<string>();
            OtherTags = new List<string>();
        }

        public RequestBase(Search search) : this() // load attribute lists
        {
            Query = search.Basic;
        }

        public virtual string GetRequestString(int pageNumber = 1)
        {
            return null;
        }

        public virtual List<FanFic> DecodeHTML(SearchOptions searchOptions)
        {
            return null;
        }

        public void FixBasicErrors()
        {
            var html = Result.Text;

            var errors = Result.ParseErrors.ToList();
            foreach(var error in errors)
            {
                switch(error.Code)
                {
                    case HtmlParseErrorCode.TagNotOpened:
                        html = $"<html>\n{html}";
                        break;
                }
            }

            html = CleanupCharCodes(html);

            Result.LoadHtml(html);
        }

        static string CleanupCharCodes(string s)
        {
            // replace characters with their equivalents
            s = s.Replace("&#160;", " ");
            s = s.Replace("&nbsp;", " ");
            s = s.Replace("&amp;", "&");
            s = s.Replace("&#09;", "    ");
            s = s.Replace("&#13;", "\n");
            // Add any more replacements you want to do here
            return s;
        }
    }
}
