﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CloudNine.Enums;

namespace CloudNine.Entities.Builders
{
    public class ApplicationCommandOptionBuilder
    {
        public ApplicationCommandOptionType Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Default { get; set; }
        public bool? Required { get; set; }
        public List<ApplicationCommandOptionChoiceBuilder> Choices { get; set; }
        public List<ApplicationCommandOptionBuilder> Options { get; set; }

        public ApplicationCommandOptionBuilder()
        {
            Name = "";
            Description = "";
            Choices = new();
            Options = new();
        }

        public ApplicationCommandOptionBuilder WithType(ApplicationCommandOptionType type)
        {
            Type = type;
            return this;
        }

        public ApplicationCommandOptionBuilder WithName(string name)
        {
            if (name is null || name == "")
                throw new Exception("Name can not be null");

            if (name.Length < 1 || name.Length > 32)
                throw new Exception("Name must be between 1 and 32 characters.");

            Name = name;
            return this;
        }

        public ApplicationCommandOptionBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        public ApplicationCommandOptionBuilder IsDefault(bool defaultCmd)
        {
            Default = defaultCmd;
            return this;
        }

        public ApplicationCommandOptionBuilder IsRequired(bool required)
        {
            Required = required;
            return this;
        }

        public ApplicationCommandOptionBuilder AddChoice(ApplicationCommandOptionChoiceBuilder choices)
        {
            // I think. Not completely sure.
            if (Options.Count >= 10)
                throw new Exception("Cant have more than 10 choices.");

            Choices.Add(choices);
            return this;
        }

        public ApplicationCommandOptionBuilder AddOption(ApplicationCommandOptionBuilder options)
        {
            if (Options.Count >= 10)
                throw new Exception("Cant have more than 10 options.");

            Options.Add(options);
            return this;
        }

        public ApplicationCommandOption Build()
        {
            List<ApplicationCommandOptionChoice> choices = new();
            foreach (var ch in Choices)
                choices.Add(ch.Build());

            List<ApplicationCommandOption> options = new();
            foreach (var op in Options)
                options.Add(op.Build());

            return new ApplicationCommandOption()
            {
                Name = Name,
                Description = Description,
                Default = Default,
                Required = Required,
                Type = Type,
                Choices = choices.Count > 0 ? choices.ToArray() : null,
                Options = options.Count > 0 ? options.ToArray() : null
            };
        }
    }
}
