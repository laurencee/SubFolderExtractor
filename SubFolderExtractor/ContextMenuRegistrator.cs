using System;
using System.Reflection;
using Microsoft.Win32;
using SubFolderExtractor.Interfaces;

namespace SubFolderExtractor
{
    public class ContextMenuRegistrator : IContextMenuRegistrator
    {
        public event EventHandler<RegistrationExceptionEvent> RegistrationExceptionEvent;

        private const string MenuName = @"Folder\shell\SubFolderExtractorMenu";
        private const string Command = @"Folder\shell\SubFolderExtractorMenu\command";
        private const string DestinationDirectoryParam = "\"%1\"";
        private readonly string _currentLocation;

        public ContextMenuRegistrator()
        {
            _currentLocation = Assembly.GetExecutingAssembly().Location;
        }

        public void AddRegistration(string contextDisplayText)
        {
            if (IsContextMenuRegistered()) return;

            string contextAction = string.Format("{0} {1}", _currentLocation, DestinationDirectoryParam);
            RegistryKey regmenu = null;
            RegistryKey regcmd = null;
            try
            {
                regmenu = Registry.ClassesRoot.CreateSubKey(MenuName);
                if (regmenu != null)
                    regmenu.SetValue("", contextDisplayText);
                regcmd = Registry.ClassesRoot.CreateSubKey(Command);
                if (regcmd != null)
                {
                    regcmd.SetValue("", contextAction);
                }
            }
            catch (Exception ex)
            {
                RaiseRegistrationExceptionEvent(new RegistrationExceptionEvent(ex));
            }
            finally
            {
                if (regmenu != null)
                    regmenu.Close();
                if (regcmd != null)
                    regcmd.Close();
            }
        }

        public void RemoveRegistration()
        {
            try
            {
                Registry.ClassesRoot.DeleteSubKey(Command, throwOnMissingSubKey: false);
                Registry.ClassesRoot.DeleteSubKey(MenuName, throwOnMissingSubKey: false);
            }
            catch (Exception ex)
            {
                RaiseRegistrationExceptionEvent(new RegistrationExceptionEvent(ex));
            }
        }

        public bool IsContextMenuRegistered()
        {
            string registrationLocation = GetRegistrationLocation();
            return registrationLocation != null;
        }

        public bool IsRegistrationLocationCorrect()
        {
            return GetRegistrationLocation() == _currentLocation;
        }

        private string GetRegistrationLocation()
        {
            string registrationLocation = null;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(Command);
                if (key != null)
                {
                    var keyValue = key.GetValue("");
                    if (keyValue != null)
                    {
                        string commandValue = keyValue.ToString();
                        registrationLocation = commandValue.Substring(0, commandValue.Length - DestinationDirectoryParam.Length).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseRegistrationExceptionEvent(new RegistrationExceptionEvent(ex));
            }
            finally
            {
                if (key != null)
                    key.Close();
            }

            return registrationLocation;
        }

        private void RaiseRegistrationExceptionEvent(RegistrationExceptionEvent e)
        {
            EventHandler<RegistrationExceptionEvent> handler = RegistrationExceptionEvent;
            if (handler != null)
                handler(this, e);
        }
    }
}