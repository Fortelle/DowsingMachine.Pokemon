using GFMSG;
using PBT.DowsingMachine;
using PBT.DowsingMachine.UI;

namespace DowsingMachine.Pokemon.Launcher;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        typeof(PBT.DowsingMachine.Pokemon.Core.PokemonProjectNS).GetType();

        DowsingMachineApp.Initialize();

        Application.Run(CreateProjectBrowser());
    }

    private static ProjectBrowser CreateProjectBrowser()
    {
        var frm = new ProjectBrowser();
        frm.LoadProjects();

        frm.FormClosed += (s, e) =>
        {
            if (frm.SelectedProject != null && frm.SelectedProject.Id != DowsingMachineApp.Config.LastSelectedProjectId)
            {
                DowsingMachineApp.Config.LastSelectedProjectId = frm.SelectedProject.Id;
            }

            DowsingMachineApp.Finalize();
        };

        if (!string.IsNullOrEmpty(DowsingMachineApp.Config.LastSelectedProjectId))
        {
            var proj = DowsingMachineApp.GetProject(DowsingMachineApp.Config.LastSelectedProjectId);
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
