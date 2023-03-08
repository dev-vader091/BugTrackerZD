﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugHunterBugTrackerZD.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project? Project { get; set; }
        public MultiSelectList? UsersList { get; set; }
        public List<string>? SelectedMembers { get; set; }
    }
}
