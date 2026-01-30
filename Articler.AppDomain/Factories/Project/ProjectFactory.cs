using Articler.AppDomain.Models.Project;
using Orleans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Articler.AppDomain.Factories.Project
{
    public static class ProjectFactory
    {
        [GenerateSerializer]
        public class Project : IProject
        {
            [Id(0)]
            public Guid Id { get; init; }

            [Id(1)]
            public ProjectState State { get; private set; }

            [Id(2)]
            public string Title { get; private set; }
            [Id(3)]
            public string Description { get; private set; }

            [Id(4)]
            public ProjectLanguage Language { get; init; }

            [Id(5)]
            public int Tokens { get; init; }

            [Id(6)]
            public DateTime CreatedDate { get; init; }

            public Project(Guid id, ProjectState state, 
                string title, string description, ProjectLanguage language,
                int tokens, DateTime createdDate)
            {
                Id = id;
                State = state;

                Title = title;
                Description = description;
                Language = language;

                Tokens = tokens;
                CreatedDate = createdDate;
            }
        }

        public static IProject CreateProject(
            Guid id, ProjectState state,
            string title, string description, ProjectLanguage lang,
            int tokens, DateTime createdDate)
        {
            return new Project(id, state, title, description, lang, tokens, createdDate);
        }
    }
}
