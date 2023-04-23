using LogicSimulator.Models;
using ReactiveUI;

namespace LogicSimulator.ViewModels {
    public class ViewModelBase: ReactiveObject {
        protected readonly static Mapper map = new();
        protected static Project? current_proj;
    }
}