using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace CloudNine.Core.Blog
{
    public class BlogPost
    {
        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("id")]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; init; }

        [JsonProperty("featured")]
        public bool Featured { get; set; }

        [JsonProperty("posted_on")]
        public DateTime PostedOn { get; init; }

        [JsonProperty("last_update")]
        public DateTime? LastUpdate { get; private set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; private set; }

        [JsonProperty("author")]
        public string Author { get; private set; }

        [JsonProperty("editors")]
        public List<string> Editors { get; init; }
        /// <summary>
        /// Base path to any saved image/video files.
        /// </summary>
        [JsonProperty("markdown")]
        public string Markdown { get; private set; }

        internal BlogPost()
        {
            Name = "";
            Id = 0;
            Featured = false;
            PostedOn = DateTime.UtcNow;
            LastUpdate = null;
            Tags = new();
            Author = "";
            Editors = new();
            Markdown = "";
        }

        public BlogPost(string name, ulong id, string author, string markdown, List<string>? tags = null)
        {
            this.Name = name;
            this.Id = id;
            this.PostedOn = DateTime.UtcNow;
            this.LastUpdate = null;
            this.Tags = tags ?? new();
            this.Author = author;
            this.Editors = new();
            this.Markdown = markdown;
        }

        public void Update(BlogPost post, string editor)
            => Update(editor, post.Author, post.Name, post.Markdown, post.Tags);

        public void Update(string editor, string newAuthor, string newName, string newMarkdown, List<string> newTagList)
        {
            this.Name = newName;
            this.Markdown = newMarkdown;
            this.Author = newAuthor;

            if (!editor.Equals(this.Author))
                this.Editors.Add(editor);

            this.Tags = newTagList;
        }
    }
}
