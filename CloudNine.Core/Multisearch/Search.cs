using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Requests;

namespace CloudNine.Core.Multisearch
{
    public class Search
    {
        public string? Basic {get; set;}
        public string? Title {get; set;}

        public List<string>? Authors {get; set;}
        public List<string>? Characters {get; set;}
        public List<string>? Relationships {get; set;}
        public List<string>? Fandoms {get; set;}
        public List<string>? OtherTags {get; set;}

        public Tuple<int, int>? Likes {get; set;}
        public Tuple<int, int>? Views {get; set;}
        public Tuple<int, int>? Comments {get; set;}
        public Tuple<int, int>? WordCount {get; set;}

        public Tuple<DateTime, DateTime>? UpdateBefore {get; set;}
        public Tuple<DateTime, DateTime>? PublishBefore {get; set;}

        public SearchDirection? Direction {get; set;}
        public SearchBy? SearchFicsBy {get; set;}
        public Rating? FicRating {get; set;}
        public FicStatus? Status {get; set;}
        public CrossoverStatus? Crossover {get; set;}

        public List<RequestBase> Requests { get; private set; }

        public Search()
        {
            Requests = new List<RequestBase>();
        }

        [Obsolete("Use nullability of attributes instead", true)]
        public bool[] BuildUsedRequestArray()
        {
            // Set anything false that is not used. This will be used when guilding the query string.
            bool[] used = new bool[18] {true, true, true, true, true,
                    true, true, true, true, true, true, true, true, true,
                    true, true, true, true};

            if (Basic is null || Basic.Equals(""))
                used[0] = false;
            if (Title is null || Title.Equals(""))
                used[1] = false;
            if (Authors is null || Authors.Equals(""))
                used[2] = false;
            if (Characters is null || Characters.Equals(""))
                used[3] = false;
            if (Relationships is null || Relationships.Equals(""))
                used[4] = false;
            if (Fandoms is null || Fandoms.Equals(""))
                used[5] = false;
            if (OtherTags is null || OtherTags.Equals(""))
                used[6] = false;

            if (Likes.Item1 == 0 && Likes.Item2 == 0)
                used[7] = false;
            if (Views.Item1 == 0 && Views.Item2 == 0)
                used[8] = false;
            if (Comments.Item1 == 0 && Comments.Item2 == 0)
                used[9] = false;
            if (WordCount.Item1 == 0 && WordCount.Item2 == 0)
                used[10] = false;

            if (UpdateBefore.Item1 == default || UpdateBefore.Item2 == default)
                used[11] = false;
            if (PublishBefore.Item1 == default || PublishBefore.Item2 == default)
                used[12] = false;

            if (Direction == 0)
                used[13] = false;
            if (SearchFicsBy == 0)
                used[14] = false;
            if (FicRating == 0)
                used[15] = false;
            if (Status == 0)
                used[16] = false;
            if (Crossover == 0)
                used[17] = false;

            return used;
        }

        /// <summary>
        /// Removes old requests of the same request type, adds new reuqest to the list
        /// </summary>
        /// <param name="request">New Request</param>
        public void UpdateRequestString(RequestBase request)
        {
            int i = Requests.FindIndex(x => x.GetType() == request.GetType());
            if (i != -1) Requests.RemoveAt(i); // if there is an item, remove it.
            Requests.Add(request);
        }
        /// <summary>
        /// Tells the Search to compile the seprate search requests for different platforms
        /// </summary>
        public void BuildRequests()
        {
            BuildFanficRequest();
            //BuildAO3Request(); // disabled due to violation of robots.txt
            BuildWattpadRequest();
        }

        private void BuildFanficRequest()
        {
            FanfictionRequest fr = new FanfictionRequest(this);
            UpdateRequestString(fr);
        }

        private void BuildAO3Request()
        {
            AO3Request ar = new AO3Request(this);
            UpdateRequestString(ar);
        }

        private void BuildWattpadRequest()
        {
            WattpadRequest wr = new WattpadRequest(this);
            UpdateRequestString(wr);
        }
    }
}
