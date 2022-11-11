using GFMSG;
using PBT.DowsingMachine.Pokemon.Common;
using PBT.DowsingMachine.Pokemon.Core.Gen9;
using PBT.DowsingMachine.Projects;
using PBT.DowsingMachine.UI;

namespace DowsingMachine.Pokemon.Launcher;

internal static class ProjectManager
{
    public static List<DataProject> Projects = new();

    public static void Init()
    {
        var projects = ProjectLoader.Load();
        Projects.AddRange(projects);
    }

    public static ProjectBrowser CreateProjectBrowser()
    {
        var frm = new ProjectBrowser();
        frm.FormClosed += (s, e) =>
        {
            if (frm.SelectedProject != null && frm.SelectedProject.Key != Properties.Settings.Default.LastSelectedProject)
            {
                Properties.Settings.Default.LastSelectedProject = frm.SelectedProject.Key;
                Properties.Settings.Default.Save();
            }
        };
        frm.LoadProjects(Projects.ToArray());

        var lastSelectedProject = Properties.Settings.Default.LastSelectedProject;
        if (!string.IsNullOrEmpty(lastSelectedProject))
        {
            var proj = Projects.FirstOrDefault(x => x.Key == lastSelectedProject);
            if (proj != null)
            {
                frm.SelectProject(proj, true);
            }
        }

        frm.ExtendedPreview = (data) =>
        {
            switch (data)
            {
                case MultilingualCollection mc:
                    {
                        var frm = new GFMSG.GUI.ExplorerForm(mc.Version, false);
                        frm.LoadMessage(mc);
                        frm.Show();
                    }
                    return true;
                default:
                    return false;
            }

        };

        return frm;
    }
}
