﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.Controls.Dialogs;
using Manager.Services;
using Services.Exceptions.Launcher;

namespace Manager.ViewModel.Shared
{
    public class BaseViewModel : ViewModelBase
    {
        private bool _isBusy;

        private bool _hasNotification;

        private string _notification;

        private RelayCommand<string> _handleHyperLinkCommand;

        private readonly IDialogService _dialogService = SimpleIoc.Default.GetInstance<IDialogService>();

        public bool HasNotification
        {
            get => _hasNotification;
            set { Set(() => HasNotification, ref _hasNotification, value); }
        }

        public string Notification
        {
            get => _notification;
            set { Set(() => Notification, ref _notification, value); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { Set(() => IsBusy, ref _isBusy, value); }
        }

        public RelayCommand<string> HandleHyperLinkCommand
        {
            get
            {
                return _handleHyperLinkCommand
                       ?? (_handleHyperLinkCommand = new RelayCommand<string>(
                           link => { Process.Start(link); }));
            }
        }

        protected async Task HandleGameLauncherException(Exception ex, string defaultMessage = null)
        {
            Logger.Instance.Log(ex);
            string message = defaultMessage ?? string.Format(Properties.Resources.DialogErrorStartingCsgo, ex.Message);
            switch (ex)
            {
                case SteamExecutableNotFoundException _:
                    message = Properties.Resources.DialogSteamNotFound;
                    break;
                case HlaeNotFound _:
                    message = Properties.Resources.HlaeNotFound;
                    break;
                case KillCsgoException _:
                    message = string.Format(Properties.Resources.DialogErrorKillingCsgo, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    break;
                case KillHlaeException _:
                    message = string.Format(Properties.Resources.DialogErrorKillingHlae, ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                    break;
            }

            await _dialogService.ShowErrorAsync(message, MessageDialogStyle.Affirmative);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            IsBusy = false;
            HasNotification = false;
            Notification = string.Empty;
        }
    }
}
