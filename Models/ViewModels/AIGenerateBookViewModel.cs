namespace EBookDashboard.Models.ViewModels
{
    public class AIGenerateBookViewModel
    {
        // to use many Models in Razor View
        public AIBookRequest BookRequest { get; set; }
        public IEnumerable<Plans> AvailablePlans { get; set; }

    }
}
