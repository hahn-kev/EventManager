using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EventCore;

namespace EventManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel()
        {

        }

        public EventsListViewModel EventsList { get; } = new EventsListViewModel();

        public async Task Load()
        {
            var modLoader = new ModLoader(@"D:\Games\FTL Stuff\EventManager\Tests\TestData\data");
            EventsList.Root.OnNext(await modLoader.Load());
        }
    }
}
