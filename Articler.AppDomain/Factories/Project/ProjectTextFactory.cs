using Articler.AppDomain.Models.Project;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Factories.Project
{
    public static class ProjectTextFactory
    {
        [GenerateSerializer]
        public class ProjectText(string text) : IProjectText
        {
            [Id(0)]
            public string Text { get; } = text;
        }

        public static IProjectText CreateProjectText(string text) => new ProjectText(text);
    }
}
