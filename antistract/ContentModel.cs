using antistract.Main;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace antistract
{
    public class ContentModel : ObservableObject
    {
        private string _content;

        public string Content
        {
            get { return _content; }
            set 
            { 
                _content = value; 
                OnPropertyChanged();
            }
        }

        /*public ContentModel()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Random r = new Random();
                    Content = r.Next(1, 1000).ToString();
                    Debug.WriteLine($"Content: {Content}");
                    Thread.Sleep(500);
                }
            });
        }*/
    }
}
