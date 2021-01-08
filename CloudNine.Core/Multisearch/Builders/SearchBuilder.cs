using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Configuration;

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

        public SearchConfiguration SearchConfiguration { get; set; }

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

        public SearchBuilder WithAuthors(IList<string> authors)
        {
            if (this.Authors is null)
                this.Authors = new();

            this.Authors.AddRange(authors);
            return this;
        }

        public SearchBuilder WithCharacter(string character)
        {
            if (this.Characters is null)
                this.Characters = new();

            this.Characters.Add(character);
            return this;
        }

        public SearchBuilder WithCharacters(IList<string> characters)
        {
            if (this.Characters is null)
                this.Characters = new();

            this.Characters.AddRange(characters);
            return this;
        }

        public SearchBuilder WithRelationship(string relationship)
        {
            if (this.Relationships is null)
                this.Relationships = new();

            this.Relationships.Add(relationship);
            return this;
        }

        public SearchBuilder WithRelationships(IList<string> relationships)
        {
            if (this.Relationships is null)
                this.Relationships = new();

            this.Relationships.AddRange(relationships);
            return this;
        }

        public SearchBuilder WithFandom(string fandom)
        {
            if (this.Fandoms is null)
                this.Fandoms = new();

            this.Fandoms.Add(fandom);
            return this;
        }

        public SearchBuilder WithFandoms(IList<string> fandoms)
        {
            if (this.Fandoms is null)
                this.Fandoms = new();

            this.Fandoms.AddRange(fandoms);
            return this;
        }

        public SearchBuilder WithOtherTag(string otherTag)
        {
            if (this.OtherTags is null)
                this.OtherTags = new();

            this.OtherTags.Add(otherTag);
            return this;
        }

        public SearchBuilder WithOtherTags(IList<string> otherTags)
        {
            if (this.OtherTags is null)
                this.OtherTags = new();

            this.OtherTags.AddRange(otherTags);
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
            this.SearchConfiguration.Direction = direction;
            return this;
        }

        public SearchBuilder SetSearchBy(SearchBy searchBy)
        {
            this.SearchConfiguration.SearchFicsBy = searchBy;
            return this;
        }

        public SearchBuilder SetRating(Raiting raiting)
        {
            this.SearchConfiguration.FicRaiting = raiting;
            return this;
        }

        public SearchBuilder SetFicStatus(FicStatus status)
        {
            this.SearchConfiguration.Status = status;
            return this;
        }

        public SearchBuilder SetCrossoverStatus(CrossoverStatus crossoverStatus)
        {
            this.SearchConfiguration.Crossover = crossoverStatus;
            return this;
        }

        public SearchBuilder SetSearchConfiguration(SearchConfiguration searchConfiguration)
        {
            this.SearchConfiguration = searchConfiguration;
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
                Direction = this.SearchConfiguration.Direction,
                SearchFicsBy = this.SearchConfiguration.SearchFicsBy,
                FicRaiting = this.SearchConfiguration.FicRaiting,
                Status = this.SearchConfiguration.Status,
                Crossover = this.SearchConfiguration.Crossover
            };

            return serach;
        }
    }
}
