using System.Collections.Generic;
using EthicalLab.Shared;

namespace EthicalLab.Domain
{
    public sealed class InvestigationReport
    {
        public MissionId Mission { get; }
        public string Prompt { get; }
        public IReadOnlyList<string> Options { get; }
        public int CorrectIndex { get; }
        public int SelectedIndex { get; }
        public bool Accepted { get; }

        public InvestigationReport(MissionId mission, string prompt, IReadOnlyList<string> options, int correctIndex, int selectedIndex, bool accepted)
        {
            Mission = mission;
            Prompt = prompt ?? "";
            Options = options ?? new string[0];
            CorrectIndex = correctIndex;
            SelectedIndex = selectedIndex;
            Accepted = accepted;
        }
    }
}
