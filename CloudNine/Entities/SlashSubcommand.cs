﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CloudNine.Commands;

namespace CloudNine.Entities
{
    public class SlashSubcommand
    {
        public string Name { get; init; }
        public MethodInfo ExecutionMethod { get; init; }
        public SlashCommandBase BaseCommand { get; init; }

        public SlashSubcommand(string name, MethodInfo method, SlashCommandBase commandInstance)
        {
            Name = name;
            ExecutionMethod = method;
            BaseCommand = commandInstance;
        }

        public Task ExecuteCommand(params object[] args)
        {
            ExecutionMethod.Invoke(BaseCommand, args);

            return Task.CompletedTask;
        }
    }
}
