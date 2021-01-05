using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudNine.Core.Multisearch.Builders
{
    public class SearchBuilder
    {
        public string? Basic { get; set; }
        public string? Title { get; set; }

        public List<string>? Authors { get; set; }
        public List<string>? Characters { get; set; }
        public List<string>? Relationships { get; set; }
        public List<string>? Fandoms { get; set; }
        public List<string>? OtherTags { get; set; }

        public Tuple<int, int>? Likes { get; set; }
        public Tuple<int, int>? Views { get; set; }
        public Tuple<int, int>? Comments { get; set; }
        public Tuple<int, int>? WordCount { get; set; }

        public Tuple<DateTime, DateTime>? UpdateBefore { get; set; }
        public Tuple<DateTime, DateTime>? PublishBefore { get; set; }

        public SearchDirection? Direction { get; set; }
        public SearchBy? SearchFicsBy { get; set; }
        public Raiting? FicRaiting { get; set; }
        public FicStatus? Status { get; set; }
        public CrossoverStatus? Crossover { get; set; }

        public SearchBuilder()
        {
            
        }

        public SearchBuilder SetBasic(string basicSearch)
        {
            this.Basic = basicSearch;
            return this;
        }

        public SearchBuilder SetTitle(string title)
        {
            this.Title = title;
            return this;
        }

        public SearchBuilder WithAuthor(string author)
        {
            if (this.Authors is null)
                this.Authors = new();

            this.Authors.Add(author);
            return this;
        }

        public SearchBuilder WithCharacter(string character)
        {
            if (this.Characters is null)
                this.Characters = new();

            this.Characters.Add(character);
            return this;
        }

        public SearchBuilder WithRelationships(string relationship)
        {
            if (this.Relationships is null)
                this.Relationships = new();

            this.Relationships.Add(relationship);
            return this;
        }

        public SearchBuilder WithFandom(string fandom)
        {
            if (this.Fandoms is null)
                this.Fandoms = new();

            this.Fandoms.Add(fandom);
            return this;
        }

        public SearchBuilder WithOtherTag(string otherTag)
        {
            if (this.OtherTags is null)
                this.OtherTags = new();

            this.OtherTags.Add(otherTag);
            return this;
        }

        public SearchBuilder SetLikes(int min, int max = -1)
        {
            this.Likes = new(min, max);
            return this;
        }

        public SearchBuilder SetViews(int min, int max = -1)
        {
            this.Views = new(min, max);
            return this;
        }

        public SearchBuilder SetComments(int min, int max = -1)
        {
            this.Comments = new(min, max);
            return this;
        }

        public SearchBuilder SetWordCount(int min, int max = -1)
        {
            this.WordCount = new(min, max);
            return this;
        }

        public SearchBuilder SetUpdateBefore(DateTime? start = null, DateTime? end = null)
        {
            if (start is null && end is null)
                throw new InvalidDateTimeConfigurationException("Both start and end dates can not be null.");

            this.UpdateBefore = new(start ?? DateTime.MinValue, end ?? DateTime.MinValue);
            return this;
        }

        public SearchBuilder SetPublishBetween(DateTime? start = null, DateTime? end = null)
        {
            if (start is null && end is null)
                throw new InvalidDateTimeConfigurationException("Both start and end dates can not be null.");

            this.PublishBefore = new(start ?? DateTime.MinValue, end ?? DateTime.MinValue);
            return this;
        }

        public SearchBuilder SetDirection(SearchDirection direction)
        {
            this.Direction = direction;
            return this;
        }

        public SearchBuilder SetSearchBy(SearchBy searchBy)
        {
            this.SearchFicsBy = searchBy;
            return this;
        }

        public SearchBuilder SetRating(Raiting raiting)
        {
            this.FicRaiting = raiting;
            return this;
        }

        public SearchBuilder SetFicStatus(FicStatus status)
        {
            this.Status = status;
            return this;
        }

        public SearchBuilder SetCrossoverStatus(CrossoverStatus crossoverStatus)
        {
            this.Crossover = crossoverStatus;
            return this;
        }

        public Search Build()
        {
            var serach = new Search // Build search from passed form results
            {
                Basic = this.Basic,
                Title = this.Title,
                Authors = this.Authors,
                Characters = this.Characters,
                Relationships = this.Relationships,
                Fandoms = this.Fandoms,
                OtherTags = this.OtherTags,
                Likes = this.Likes,
                Views = this.Views,
                Comments = this.Comments,
                WordCount = this.WordCount,
                UpdateBefore = this.UpdateBefore,
                PublishBefore = this.PublishBefore,
                Direction = this.Direction,
                SearchFicsBy = this.SearchFicsBy,
                FicRaiting = this.FicRaiting,
                Status = this.Status,
                Crossover = this.Crossover
            };

            return serach;
        }
    }
}
