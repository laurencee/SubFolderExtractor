using System;

namespace SubFolderExtractor.Interfaces
{
    public interface IContextMenuRegistrator
    {
        event EventHandler<RegistrationExceptionEvent> RegistrationExceptionEvent;

        void AddRegistration(string contextDisplayText);

        void RemoveRegistration();

        bool IsContextMenuRegistered();

        bool IsRegistrationLocationCorrect();
    }
}