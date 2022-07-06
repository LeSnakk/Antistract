using antistract.Main;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace antistract.Core
{
    public class CurrentlySelectedPlan : ObservableObject
    {
        private string _selectedPlan;

        public string SelectedPlan
        {
            get { return _selectedPlan; }
            set 
            { 
                _selectedPlan = value; 
                OnPropertyChanged();
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged()
        {
            //
        }
    }
}