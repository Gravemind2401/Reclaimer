using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclaimer.Utilities;

namespace Reclaimer.Resources.Terminology
{
    public static class Menu
    {
        static Menu()
        {
            Localiser.ConfigureResourceType(typeof(Menu));
        }

        public static string File { get; set; }
        public static string RecentFiles { get; set; } = "Recent Files";
        public static string Edit { get; set; }
        public static string View { get; set; }
        public static string Output { get; set; }
        public static string AppDirectory { get; set; } = "App Directory";
        public static string AppDataDirectory { get; set; } = "AppData Directory";
        public static string Tools { get; set; }
        public static string Settings { get; set; }
        public static string CheckForUpdates { get; set; } = "Check for updates";
        public static string ViewUpdateDetails { get; set; } = "View update details";
        public static string Themes { get; set; }
        public static string Help { get; set; }
        public static string SubmitAnIssue { get; set; } = "Submit an issue";
    }

    public static class Status
    {
        static Status()
        {
            Localiser.ConfigureResourceType(typeof(Status));
        }

        public static string Ready { get; set; }
        public static string CheckingForUpdates { get; set; } = "Checking for updates...";
        public static string UpdateAvailable { get; set; } = "There is an update available";
    }

    public static class Message
    {
        static Message()
        {
            Localiser.ConfigureResourceType(typeof(Message));
        }

        public static string ErrorCheckingUpdates { get; set; } = "Error checking for updates.";
        public static string NoUpdatesAvailable { get; set; } = "No updates available.";
    }

    public static class UI
    {
        static UI()
        {
            Localiser.ConfigureResourceType(typeof(UI));
        }

        public static string Reclaimer { get; set; }

        //general
        public static string OK { get; set; }
        public static string Cancel { get; set; }

        //open with dialog
        public static string OpenWith { get; set; } = "Open With";
        public static string Default { get; set; }
        public static string ChoosePlugin { get; set; } = "Choose the plugin you want to use to open this file:";
        public static string SetAsDefault { get; set; } = "Set as Default";

        //update dialog
        public static string ReclaimerUpdates { get; set; } = "Reclaimer Updates";
        public static string CurrentVersion { get; set; } = "Current version:";
        public static string LatestVersion { get; set; } = "Latest version:";
        public static string ReleaseDate { get; set; } = "Release Date:";
        public static string ViewDownload { get; set; } = "View Download";
    }
}
