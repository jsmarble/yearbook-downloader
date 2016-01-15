using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace YearbookDownloader
{
    public class YearbookDownloaderViewModel : INotifyPropertyChanged
    {
        public YearbookDownloaderViewModel()
        {
            this.DownloadCommand = new AsyncDelegateCommand(DownloadAsync);
            this.DownloadDirectory = YearbookClient.DefaultDownloadRoot;
        }

        public ICommand DownloadCommand { get; }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                if (username == value)
                    return;

                username = value;
                OnPropertyChanged();
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                if (password == value)
                    return;

                password = value;
                OnPropertyChanged();
            }
        }

        private string downloadDirectory;
        public string DownloadDirectory
        {
            get { return downloadDirectory; }
            set
            {
                if (downloadDirectory == value)
                    return;

                downloadDirectory = value;
                OnPropertyChanged();
            }
        }

        private int school;
        public int School
        {
            get { return school; }
            set
            {
                if (school == value)
                    return;

                school = value;
                OnPropertyChanged();
            }
        }

        private int year;
        public int Year
        {
            get { return year; }
            set
            {
                if (year == value)
                    return;

                year = value;
                OnPropertyChanged();
            }
        }

        private string status;
        public string Status
        {
            get { return status; }
            private set
            {
                if (status == value)
                    return;

                status = value;
                OnPropertyChanged();
            }
        }

        private async Task DownloadAsync(object o)
        {
            var vm = Validate();
            if (vm.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, vm), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            YearbookClient yb = new YearbookClient(Username, Password);
            this.Status = "Downloading ...";
            yb.ImageDownloaded += (sender, e) => this.Status = $"Downloaded {e.ImageNumber} ...";
            await yb.DownloadYearbook(School, Year);
            this.Status = "Download Complete";
        }

        private List<string> Validate()
        {
            List<string> m = new List<string>();

            if (string.IsNullOrWhiteSpace(Username))
                m.Add("Please enter a username.");
            if (string.IsNullOrWhiteSpace(Password))
                m.Add("Please enter a password.");
            if (School < 1)
                m.Add("Please enter a school.");
            if (Year < 1800)
                m.Add("Please enter a year.");

            return m;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
