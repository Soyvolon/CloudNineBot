using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CloudNine.Core.Multisearch.Builders;
using CloudNine.Core.Multisearch.Configuration;

using Soyvolon.Utilities.Converters.Strings;

namespace CloudNine.Core.Multisearch.Searching
{
    public class SearchParseSevice
    {
        private SearchBuilder Builder { get; set; }
        private SearchOptions Options { get; set; }

        public SearchParseSevice()
        {
            Builder = new();
            Options = new();
        }

        public void RegisterDefaults(SearchConfiguration searchConfiguration)
        {
            Builder.SetSearchConfiguration(searchConfiguration);
            Options.SearchConfiguration = searchConfiguration;
        }

        public SearchParseResult ParseSearch(params string[] args)
        {
            SearchParseResult? error;
            for(int i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--help":
                    case "-h":
                        return HelpArgument();

                    #region Search Fields
                    case "--title":
                    case "-t":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else Builder.Title = args[++i];
                        break;

                    case "--author":
                    case "-a":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = StringListArgument(args[++i], out var res)) is not null)
                            return error;
                        break;

                    case "--character":
                    case "-c":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = StringListArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.WithCharacters(res ?? new());
                        break;

                    case "--relationship":
                    case "-r":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = StringListArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.WithRelationships(res ?? new());
                        break;

                    case "--fandom":
                    case "-f":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = StringListArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.WithFandoms(res ?? new());
                        break;

                    case "--other":
                    case "-o":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = StringListArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.WithOtherTags(res ?? new());
                        break;

                    case "--likes":
                    case "-l":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = DualNumericArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.Likes = res;
                        break;

                    case "--views":
                    case "-v":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = DualNumericArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.Views = res;
                        break;

                    case "--comments":
                    case "-C":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = DualNumericArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.Comments = res;
                        break;

                    case "--words":
                    case "-w":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = DualNumericArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.WordCount = res;
                        break;

                    case "--updated":
                    case "-u":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = DateTimePairArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.UpdateBefore = res;
                        break;

                    case "--published":
                    case "-p":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = DateTimePairArgument(args[++i], out var res)) is not null)
                            return error;
                        else Builder.PublishBefore = res;
                        break;

                    case "--direction":
                    case "-D":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = EnumArgument<SearchDirection>(args[++i], out var res)) is not null)
                            return error;
                        else Builder.SearchConfiguration.Direction = res;
                        break;

                    case "--searchby":
                    case "-s":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = EnumArgument<SearchBy>(args[++i], out var res)) is not null)
                            return error;
                        else Builder.SearchConfiguration.SearchFicsBy = res;
                        break;

                    case "--rating":
                    case "-R":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = EnumArgument<Rating>(args[++i], out var res)) is not null)
                            return error;
                        else Builder.SearchConfiguration.FicRating = res;
                        break;

                    case "--status":
                    case "-S":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = EnumArgument<FicStatus>(args[++i], out var res)) is not null)
                            return error;
                        else Builder.SearchConfiguration.Status = res;
                        break;

                    case "--crossover":
                    case "-x":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = EnumArgument<CrossoverStatus>(args[++i], out var res)) is not null)
                            return error;
                        else Builder.SearchConfiguration.Crossover = res;
                        break;
                    #endregion

                    #region Search Options
                    case "--explicit":
                    case "-e":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = BooleanArgument(args[++i], out var res)) is not null)
                            return error;
                        else Options.AllowExplicit = res;
                        break;

                    case "--warnsingsnotusedaswarnings":
                    case "-W":
                        if ((error = VerifyArgCount(args, i, 1)) is not null)
                            return error;
                        else if ((error = BooleanArgument(args[++i], out var res)) is not null)
                            return error;
                        else Options.TreatWarningsNotUsedAsWarnings = res;
                        break;
                    #endregion

                    default:
                        Builder.Basic += $" {args[i]} ";
                        break;
                }
            }

            // complie final serach results
            Builder.Basic = Builder.Basic?.Trim();

            return new SearchParseResult()
            {
                DisplayHelp = false,
                Errored = false,
                SearchBuilder = Builder,
                SearchOptions = Options
            };
        }

        private static SearchParseResult? VerifyArgCount(string[] args, int pos, int needed)
        {
            if (pos + needed >= args.Length)
                return ParseError($"Failed to parse {args[pos]}, not enough arguments. " +
                    $"({needed} needed, {args.Length - (pos + needed + 1)} provided)");
            else return null;
        }

        private static SearchParseResult ParseError(string reason)
            => new SearchParseResult()
            {
                Errored = true,
                ErrorMessage = reason
            };

        private static SearchParseResult HelpArgument()
            => new SearchParseResult()
            { 
                DisplayHelp = true,
                Errored = false
            };

        private static SearchParseResult? DualNumericArgument(string arg, out Tuple<int, int>? value)
        {
            if(IntegerPairConverter.TryParse(arg, out value))
            {
                // return null on success
                return null;
            }
            else
            {
                return ParseError("Failed to parse input into an integer pair.");
            }
        }

        private static SearchParseResult? DateTimePairArgument(string arg, out Tuple<DateTime, DateTime>? value)
        {
            if (TimeSpanPairConverter.TryParse(arg, out var res))
            {
                value = res.ConvertToDateTimePair();
                return null;
            }
            else
            {
                value = null;
                return ParseError("Failed to parse input to a DateTime pair.");
            }
        }

        private static SearchParseResult? StringListArgument(string arg, out List<string>? value)
        {
            value = arg.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList();
            // return null on success
            return null;
        }

        public static SearchParseResult? EnumArgument<T>(string arg, out T? value) where T : Enum
        {
            var type = typeof(T);
            if (int.TryParse(arg, out var num))
            {
                try
                {
                    value = (T)Enum.ToObject(type, num);
                    return null;
                }
                catch
                {
                    value = default;
                    return ParseError($"Integer value was too high or two low to convert to {type.Name}");
                }
            }
            else
            {
                var names = type.GetEnumNames();
                var values = type.GetEnumValues();
                for (int i = 0; i < names.Length; i++)
                {
                    if(names[i].ToLower() == arg.ToLower())
                    {
                        try
                        {
                            value = (T)Enum.ToObject(type, values.GetValue(i) ?? throw new NullReferenceException("Enum value is null."));
                            return null;
                        }
                        catch
                        {
                            value = default;
                            return ParseError($"Failed to properly parse {arg} to {type.Name}.");
                        }
                    }
                }
            }

            value = default;
            return ParseError($"Failed to convert {arg} to a {type.Name} value.");
        }

        public static SearchParseResult? BooleanArgument(string arg, out bool res)
        {
            if(bool.TryParse(arg, out res))
            {
                return null;
            }
            else
            {
                return ParseError($"Failed to parse {arg} into a boolean value (true or false).");
            }
        }

        public static SearchParseResult? IntegerArgument(string arg, out int res)
        {
            if(int.TryParse(arg, out res))
            {
                return null;
            }
            else
            {
                return ParseError($"Failed to parse {arg} into an integer.");
            }
        }
    }
}
